using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class SiteFormCorpProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly Log _log;
        private readonly FormRequest _formRequest;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;

        public SiteFormCorpProcessor(Amo amo, Log log, FormRequest formRequest, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName)
        {
            _amo = amo;
            _log = log;
            _formRequest = formRequest;
            _processQueue = processQueue;
            _token = token;
            _gSheets = gSheets;
            _taskName = taskName;

            var acc = amo.GetAccountById(19453687);
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
            _compRepo = acc.GetRepo<Company>();
        }

        private readonly Dictionary<string, int> fieldIds = new()
        {
            { "form_name_site", 748383 },
            { "site", 758213 },
            { "page_url", 758215 },
            //{ "page_title", 0 },
            { "roistat_marker", 748385 },
            { "roistat_visit", 758217 },
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

        private static bool IsValidField(string field)
        {
            return field is not null &&
                   field != "undefined" &&
                   field != "";
        }


        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskName);
                return Task.FromCanceled(_token);
            }
            try
            {
                #region Checking for contacts
                if (!IsValidField(_formRequest.email) &&
                    !IsValidField(_formRequest.phone))
                {
                    _log.Add("Request without contacts");
                    _processQueue.Remove(_taskName);
                    return Task.CompletedTask;
                }

                if (IsValidField(_formRequest.phone))
                    _formRequest.phone = _formRequest.phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                #endregion

                Lead lead = new()
                {
                    name = "Новая сделка",
                    responsible_user_id = 2375146,
                    _embedded = new()
                };

                string companyName = "";

                #region Checking company
                List<Company> similarCompanies = new();
                try
                {
                    if (IsValidField(_formRequest.phone))
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_formRequest.phone}"));

                    if (IsValidField(_formRequest.email))
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_formRequest.email}"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих компаний: {e}"); }

                if (similarCompanies.Any())
                {
                    _log.Add($"Найдена похожая компания: {similarCompanies.First().id}.");
                    companyName = similarCompanies.First().name;
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
                    if (IsValidField(_formRequest.phone))
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.phone}"));

                    if (IsValidField(_formRequest.email))
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

                    companyName = _compRepo.GetById(lead._embedded.companies.First().id).name;
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

                    if (IsValidField(_formRequest.email))
                        contact.AddNewCF(33577, _formRequest.email);

                    if (IsValidField(_formRequest.phone))
                        contact.AddNewCF(33575, _formRequest.phone);
                }

                lead._embedded.contacts = new() { contact };
                #endregion

                #region Setting pipeline and status if any
                int pipeline = 0;
                int status = 0;

                if (IsValidField(_formRequest.pipeline))
                {
                    int.TryParse(_formRequest.pipeline, out pipeline);

                    if (IsValidField(_formRequest.status))
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
                        IsValidField((string)p.GetValue(_formRequest)))
                    {
                        object value = p.GetValue(_formRequest);

                        if (p.Name == "date")
                        {
                            if (!DateTime.TryParse((string)value, out DateTime dt))
                                continue;
                            value = ((DateTimeOffset)dt).ToUnixTimeSeconds();
                        }

                        lead.AddNewCF(fieldIds[p.Name], value);
                    }
                #endregion

                try
                {
                    if (!string.IsNullOrEmpty(_formRequest.responsible) &&
                        int.TryParse(_formRequest.responsible, out int respId))
                        lead.responsible_user_id = respId;

                    int.TryParse(_formRequest.price, out int price);
                    lead.price = price;

                    var created = _leadRepo.AddNewComplex(lead);

                    if (!created.Any())
                        throw new InvalidOperationException("Returned 0 created leads from amo");

                    if (IsValidField(_formRequest.comment))
                        _leadRepo.AddNotes(created.First(), _formRequest.comment);

                    _log.Add($"Создана новая сделка {created.First()}");

                    bool.TryParse(_formRequest.events, out bool events);

                    if (events)
                    {
                        GSheetsProcessor leadProcessor = new(created.First(), _amo, _gSheets, _processQueue, _log, _token);
                        leadProcessor.Conference((int)lead.responsible_user_id, companyName, contact.name, _formRequest.email, _formRequest.phone, lead.status_id == 43009228, price, _formRequest.comment).Wait();
                        _log.Add($"Добавлены данные о сделке {created.First()} в таблицу.");
                    }

                }
                catch (Exception e)
                {
                    _log.Add($"Не получилось сохранить данные в амо: {e}.");
                    _log.Add($"POST: {JsonConvert.SerializeObject(_formRequest, Formatting.Indented)}");
                    throw;
                }
                
                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось добавить заявку с сайта: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}