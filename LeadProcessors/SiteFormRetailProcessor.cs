using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class SiteFormRetailProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly FormRequest _formRequest;
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;

        public SiteFormRetailProcessor(AmoAccount acc, Log log, FormRequest formRequest, TaskList processQueue, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
            _log = log;
            _formRequest = formRequest;
            _processQueue = processQueue;
            _token = token;
        }

        private Dictionary<string, int> fieldIds = new() {
            { "form_name_site", 639075 },
            { "site", 639081 },
            { "page_url", 639083 },
            { "roistat_marker", 639085 },
            { "roistat_visit", 639073 },
            { "city_name", 639087 },
            { "_ym_uid", 715049 },
            { "clid", 643439 },
        };

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"FormSiteRet");
                return Task.FromCanceled(_token);
            }

            try
            {
                #region Checking for contacts
                if ((_formRequest.email is null ||
                    _formRequest.email == "undefined" ||
                    _formRequest.email == "") &&
                    (_formRequest.phone is null ||
                    _formRequest.phone == "undefined" ||
                    _formRequest.phone == ""))
                {
                    _log.Add("Request without contacts");
                    return Task.CompletedTask;
                }

                if (_formRequest.phone is not null &&
                    _formRequest.phone != "undefined" &&
                    _formRequest.phone != "")
                    _formRequest.phone = _formRequest.phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                #endregion

                Lead lead = new()
                {
                    name = "Новая сделка",
                    responsible_user_id = 2576764,
                    _embedded = new()
                };

                #region Creating contact
                List<Contact> similarContacts = new();
                try
                {
                    if (_formRequest.phone is not null &&
                        _formRequest.phone != "undefined" &&
                        _formRequest.phone != "")
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.phone}"));

                    if (_formRequest.email is not null &&
                        _formRequest.email != "undefined" &&
                        _formRequest.email != "")
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.email}"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих контактов: {e}"); }

                Contact contact = new()
                {
                    name = _formRequest.name,
                    responsible_user_id = 2576764,
                };

                if (similarContacts.Any())
                {
                    contact.id = similarContacts.First().id;
                    contact.responsible_user_id = similarContacts.First().responsible_user_id;
                    lead.responsible_user_id = similarContacts.First().responsible_user_id;
                    _log.Add($"Найден похожий контакт: {contact.id}.");
                }
                else
                {
                    contact.custom_fields_values = new();

                    if (_formRequest.email is not null &&
                        _formRequest.email != "undefined" &&
                        _formRequest.email != "")
                        contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                        {
                            field_id = 264913,
                            values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = _formRequest.email } }
                        });

                    if (_formRequest.phone is not null &&
                        _formRequest.phone != "undefined" &&
                        _formRequest.phone != "")
                        contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                        {
                            field_id = 264911,
                            values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = _formRequest.phone } }
                        });
                }

                lead._embedded.contacts = new() { contact };
                #endregion

                #region Setting pipeline and status if any
                int pipeline = 0;
                int status = 0;

                if (_formRequest.pipeline is not null &&
                    _formRequest.pipeline != "undefined" &&
                    _formRequest.pipeline != "")
                {
                    int.TryParse(_formRequest.pipeline, out pipeline);

                    if (_formRequest.status is not null &&
                        _formRequest.status != "undefined" &&
                        _formRequest.status != "")
                        int.TryParse(_formRequest.status, out status);
                }

                if (pipeline > 0)
                {
                    lead.pipeline_id = pipeline;
                    if (status > 0)
                        lead.status_id = status;
                }
                else
                {
                    lead.pipeline_id = 3198184;
                    lead.status_id = 32532880;
                }
                #endregion

                #region Setting custom fields
                foreach (var p in _formRequest.GetType().GetProperties())
                    if (fieldIds.ContainsKey(p.Name) &&
                        p.GetValue(_formRequest) is not null &&
                        (string)p.GetValue(_formRequest) != "undefined" &&
                        (string)p.GetValue(_formRequest) != "")
                    {
                        if (lead.custom_fields_values is null) lead.custom_fields_values = new();
                        lead.custom_fields_values.Add(new Lead.Custom_fields_value()
                        {
                            field_id = fieldIds[p.Name],
                            values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = (string)p.GetValue(_formRequest) } }
                        });
                    }
                #endregion

                try
                {
                    var created = _leadRepo.AddNewComplex(lead);

                    if (created.Any() &&
                        _formRequest.comment is not null &&
                        _formRequest.comment != "undefined" &&
                        _formRequest.comment != "")
                        _leadRepo.AddNotes(new Note() { entity_id = created.First(), note_type = "common", parameters = new Note.Params() { text = $"{_formRequest.comment}" } });

                    _log.Add($"Создана новая сделка {created.First()}");
                }
                catch (Exception e)
                {
                    _log.Add($"Не получилось сохранить данные в амо: {e}.");
                    _log.Add($"POST: {JsonConvert.SerializeObject(_formRequest, Formatting.Indented)}");
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось добавить заявку с сайта: {e}.");
                _processQueue.Remove($"FormSiteRet");
                return Task.FromException(e);
            }
        }
    }
}