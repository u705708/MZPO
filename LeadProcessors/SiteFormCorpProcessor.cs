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
    public class SiteFormCorpProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly Log _log;
        private readonly FormRequest _formRequest;
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;

        public SiteFormCorpProcessor(AmoAccount acc, Log log, FormRequest formRequest, TaskList processQueue, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
            _compRepo = acc.GetRepo<Company>();
            _log = log;
            _formRequest = formRequest;
            _processQueue = processQueue;
            _token = token;
        }

        private readonly Dictionary<string, int> fieldIds = new()
        {
            //{ "form_name_site", 0 },
            //{ "site", 0 },
            //{ "page_url", 0 },
            //{ "page_title", 0 },
            //{ "roistat_marker", 0 },
            //{ "roistat_visit", 0 },
            //{ "city_name", 0 },
            //{ "_ym_uid", 0 },
            //{ "_ya_uid", 0 },
            //{ "clid", 0 },
            //{ "utm_source", 0 },
            //{ "utm_medium", 0 },
            //{ "utm_term", 0 },
            //{ "utm_content", 0 },
            //{ "utm_campaign", 0 },
        };

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"FormSiteCorp");
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
                    responsible_user_id = 2375146,
                    _embedded = new()
                };

                #region Checking company
                List<Company> similarCompanies = new();
                try
                {
                    if (_formRequest.phone is not null &&
                        _formRequest.phone != "undefined" &&
                        _formRequest.phone != "")
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_formRequest.phone}"));

                    if (_formRequest.email is not null &&
                        _formRequest.email != "undefined" &&
                        _formRequest.email != "")
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_formRequest.email}"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих компаний: {e}"); }

                if (similarCompanies.Any())
                {
                    _log.Add($"Найдена похожая компания: {similarCompanies.First().id}.");
                    lead._embedded.companies = new()
                    {
                        new()
                        {
                            responsible_user_id = similarCompanies.First().responsible_user_id,
                            id = similarCompanies.First().id
                        }
                    };
                    lead.responsible_user_id = similarCompanies.First().responsible_user_id;
                }
                #endregion

                #region Checking contact
                List<Contact> similarContacts = new();

                Contact contact = new()
                {
                    name = _formRequest.name,
                    responsible_user_id = 2375146,
                };

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

                if (!similarCompanies.Any() &&
                    lead._embedded.companies is null &&
                    similarContacts.Any(x => x._embedded.companies is not null &&
                                             x._embedded.companies.Any()))
                {
                    lead._embedded.companies = new()
                    {
                        new()
                        {
                            responsible_user_id = similarContacts.First(x => x._embedded.companies is not null &&
                                                            x._embedded.companies.Any())._embedded.companies.First().responsible_user_id,
                            id = similarContacts.First(x => x._embedded.companies is not null &&
                                                            x._embedded.companies.Any())._embedded.companies.First().id
                        }
                    };

                    lead.responsible_user_id = similarContacts.First(x => x._embedded.companies is not null &&
                                                        x._embedded.companies.Any())._embedded.companies.First().responsible_user_id;
                }

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
                            field_id = 33577,
                            values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = _formRequest.email } }
                        });

                    if (_formRequest.phone is not null &&
                        _formRequest.phone != "undefined" &&
                        _formRequest.phone != "")
                        contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                        {
                            field_id = 33575,
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
                    lead.pipeline_id = 3558781;
                    lead.status_id = 35001112;
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
                    throw;
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось добавить заявку с сайта: {e}.");
                _processQueue.Remove($"FormSiteCorp");
                return Task.FromException(e);
            }
        }
    }
}