using Google.Apis.Sheets.v4;
using MZPO.AmoRepo;
using MZPO.Services;
using MZPO.webinar.ru;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class SiteFormRetailProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly FormRequest _formRequest;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskname;
        private readonly Webinars _webinars;

        public SiteFormRetailProcessor(Amo amo, Log log, FormRequest formRequest, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, Webinars webinars)
        {
            _amo = amo;
            _log = log;
            _formRequest = formRequest;
            _processQueue = processQueue;
            _token = token;
            _gSheets = gSheets;
            _taskname = taskName;
            _webinars = webinars;

            var acc = amo.GetAccountById(28395871);
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
        }

        private readonly Dictionary<string, int> fieldIds = new() {
            { "form_name_site", 639075 },
            { "site", 639081 },
            { "page_url", 639083 },
            { "page_title", 647653 },
            { "roistat_marker", 639085 },
            { "roistat_visit", 639073 },
            { "city_name", 639087 },
            { "course", 357005 },
            { "_ym_uid", 715049 },
            { "_ya_uid", 715049 },
            { "clid", 643439 },
            { "utm_source", 640697 },
            { "utm_medium", 640699 },
            { "utm_term", 640703 },
            { "utm_content", 643437 },
            { "utm_campaign", 640701 },
            { "date", 724347 },
            { "promocode", 725309 },
            { "event_name", 725709 },
            { "event_address", 725711 },
        };

        private class ContactsComparer : IEqualityComparer<Contact>
        {
            public bool Equals(Contact x, Contact y)
            {
                if (ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Contact c)
            {
                if (c is null) return 0;

                int hashProductCode = (int)c.id;

                return hashProductCode;
            }
        }

        private static void PopulateCFs(Lead lead, FormRequest formRequest, Dictionary<string, int> fieldIds)
        {
            foreach (var p in formRequest.GetType().GetProperties())
                if (fieldIds.ContainsKey(p.Name) &&
                    IsValidField((string)p.GetValue(formRequest)))
                {
                    object value = p.GetValue(formRequest);

                    if (p.Name == "date")
                    {
                        if (!DateTime.TryParse((string)value, out DateTime dt))
                                continue;
                        value = ((DateTimeOffset)dt).ToUnixTimeSeconds();
                    }

                    lead.AddNewCF(fieldIds[p.Name], value);
                }
        }

        private static IEnumerable<int> AddNewLead(List<Contact> similarContacts, int price, bool webinar, bool events, FormRequest formRequest, Dictionary<string, int> fieldIds, IAmoRepo<Lead> leadRepo, Log log, bool demoLesson)
        {
            Lead lead = new()
            {
                name = "Новая сделка",
                price = price,
                responsible_user_id = 2576764,
                _embedded = new()
            };

            Contact contact = new()
            {
                name = formRequest.name,
                responsible_user_id = 2576764,
            };

            if (similarContacts.Any())
            {
                contact.id = similarContacts.First().id;
                contact.responsible_user_id = similarContacts.First().responsible_user_id;
                lead.responsible_user_id = similarContacts.First().responsible_user_id;
                log.Add($"Найден похожий контакт: {contact.id}.");
            }
            else
            {
                contact.custom_fields_values = new();

                if (IsValidField(formRequest.email))
                    contact.AddNewCF(264913, formRequest.email);

                if (IsValidField(formRequest.phone))
                    contact.AddNewCF(264911, formRequest.phone);
            }

            lead._embedded.contacts = new() { contact };

            #region Setting Pipelines
            int pipeline = 0;
            int status = 0;

            if (price == 0 &&
                webinar)
            {
                pipeline = 3199819;
                status = 32544562;
            }

            if (demoLesson)
            {
                pipeline = 4586602;
                status = 42430264;
            }

            if (IsValidField(formRequest.pipeline))
            {
                int.TryParse(formRequest.pipeline, out pipeline);

                if (IsValidField(formRequest.status))
                    int.TryParse(formRequest.status, out status);
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

            #region Add tags
            if (webinar)
            {
                if (lead._embedded.tags is null) lead._embedded.tags = new();
                lead._embedded.tags.Add(new() { id = 276829 });
            }

            if (events)
            {
                if (lead._embedded.tags is null) lead._embedded.tags = new();
                lead._embedded.tags.Add(new() { id = 276831 });
            } 
            #endregion

            PopulateCFs(lead, formRequest, fieldIds);

            IEnumerable<int> processedIds = leadRepo.AddNewComplex(lead);

            log.Add($"Создана новая сделка {processedIds.First()}");

            return processedIds;
        }

        private static IEnumerable<int> UpdateFoundLead(Lead oldLead, FormRequest formRequest, Dictionary<string, int> fieldIds, IAmoRepo<Lead> leadRepo, Log log, bool demoLesson)
        {
            Lead lead = new()
            {
                id = oldLead.id,
                pipeline_id = demoLesson ? 4586602 : 3198184,
                status_id = demoLesson ? 42430264 : 32532880
            };

            //PopulateCFs(lead, formRequest, fieldIds);

            IEnumerable<int> processedIds = leadRepo.Save(lead).Select(x => x.id);

            log.Add($"Обновлена сделка {processedIds.First()}");

            return processedIds;
        }

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
                _processQueue.Remove(_taskname);
                return Task.FromCanceled(_token);
            }

            try
            {
                #region Checking for contacts
                if (!IsValidField(_formRequest.email) &&
                    !IsValidField(_formRequest.phone))
                {
                    _log.Add("Request without contacts");
                    _processQueue.Remove(_taskname);
                    return Task.CompletedTask;
                }

                if (IsValidField(_formRequest.phone))
                    _formRequest.phone = _formRequest.phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                #endregion

                #region Getting similar contact
                List<Contact> similarContacts = new();
                try
                {
                    if (IsValidField(_formRequest.phone))
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.phone}&with=leads"));

                    if (IsValidField(_formRequest.email))
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.email}&with=leads"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих контактов: {e.Message}"); }
                #endregion

                #region Getting similar leads
                List<Lead> similarLeads = new();

                if (similarContacts.Any())
                {
                    List<int> leadIds = new();

                    foreach (var c in similarContacts)
                        if (c._embedded is not null &&
                            c._embedded.leads is not null)
                            leadIds.AddRange(c._embedded.leads.Select(x => x.id));

                    if (leadIds.Any())
                        similarLeads.AddRange(_leadRepo.BulkGetById(leadIds.Distinct()).Where(x => x.status_id != 142 && x.status_id != 143));
                }
                #endregion

                #region Parsing webinars and events
                bool.TryParse(_formRequest.webinar, out bool webinar);
                bool.TryParse(_formRequest.events, out bool events);
                int.TryParse(_formRequest.price, out int price);

                bool demoLesson = _formRequest.form_name_site is not null && _formRequest.form_name_site.Contains("Пробный урок");
                #endregion

                try
                {
                    IEnumerable<int> processedIds;

                    if (similarLeads.Any() &&
                        price == 0 &&
                        !webinar &&
                        !IsValidField(_formRequest.pipeline) &&
                        !IsValidField(_formRequest.status) &&
                        similarLeads.First().pipeline_id != 3338257 &&
                        similarLeads.First().pipeline_id != 4234969)
                        processedIds = UpdateFoundLead(similarLeads.First(), _formRequest, fieldIds, _leadRepo, _log, demoLesson);
                    else
                        processedIds = AddNewLead(similarContacts, price, webinar, events, _formRequest, fieldIds, _leadRepo, _log, demoLesson);

                    if (processedIds.Any() &&
                        IsValidField(_formRequest.email) &&
                        long.TryParse(_formRequest.webinar_id, out long webinarId))
                    {
                        var webinarEvent = _webinars.GetEvent(webinarId).Result;

                        long eventSessionId = 0;
                        long eventDateTime = 0;

                        if (webinarEvent.eventSessions is not null &&
                            webinarEvent.eventSessions.Any())
                        {
                            eventSessionId = webinarEvent.eventSessions.First().id;
                            eventDateTime = ((DateTimeOffset)webinarEvent.eventSessions.First().startsAt).ToUnixTimeSeconds();
                        }

                        var response = _webinars.AddUserToEventSession(eventSessionId, _formRequest.email).Result;

                        long participtionId = response.participationId;
                        string link = response.link;

                        //long contactId = 0;
                        //if (response.contactId is null ||
                        //    response.contactId == 0)
                        //{
                        //    var users = _webinars.SearchUser(_formRequest.email).Result;
                        //    if (users.Any())
                        //        contactId = (long)users.First().id;

                        //}
                        //else contactId = (long)response.contactId;

                        Lead lead = new()
                        {
                            id = processedIds.First(),
                            custom_fields_values = new()
                        };

                        lead.AddNewCF(725627, link);                     //Ссылка на регистрацию
                        lead.AddNewCF(725629, eventDateTime);            //Дата мероприятия
                        lead.AddNewCF(725631, webinarId);                //EventId
                        lead.AddNewCF(725633, eventSessionId);           //EventSessionId
                        lead.AddNewCF(725635, participtionId);           //ParticipationId

                        _leadRepo.Save(lead);

                        _leadRepo.AddNotes(processedIds.First(), $"Ссылка для регистрации на вебинар: {link}");
                    }

                    if (processedIds.Any() &&
                        (IsValidField(_formRequest.comment) ||
                        IsValidField(_formRequest.title)))
                    {
                        if (IsValidField(_formRequest.title))
                            _leadRepo.AddNotes(processedIds.First(), _formRequest.title);
                        if (IsValidField(_formRequest.comment))
                            _leadRepo.AddNotes(new Note() { entity_id = processedIds.First(), note_type = "common", parameters = new Note.Params() { text = $"{_formRequest.date} {_formRequest.comment}" } });
                        _log.Add($"Добавлены комментарии в сделку {processedIds.First()}");

                        if (webinar)
                        {
                            GSheetsProcessor leadProcessor = new(processedIds.First(), _amo, _gSheets, _processQueue, _log, _token);
                            leadProcessor.Webinar(_formRequest.date, _formRequest.comment, price, _formRequest.name, _formRequest.phone, _formRequest.email).Wait();
                            _log.Add($"Добавлены данные о сделке {processedIds.First()} в таблицу.");
                        }
                        if (events)
                        {
                            GSheetsProcessor leadProcessor = new(processedIds.First(), _amo, _gSheets, _processQueue, _log, _token);
                            //leadProcessor.Events(_formRequest.date, _formRequest.comment, price, _formRequest.name, _formRequest.phone, _formRequest.email, processedIds.First()).Wait();
                            leadProcessor.Webinar(_formRequest.date, _formRequest.comment, price, _formRequest.name, _formRequest.phone, _formRequest.email).Wait();
                            _log.Add($"Добавлены данные о сделке {processedIds.First()} в таблицу.");
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Add($"Не получилось сохранить данные в амо: {e.Message}.");
                    _log.Add($"POST: {JsonConvert.SerializeObject(_formRequest, Formatting.Indented)}");
                }

                _processQueue.Remove(_taskname);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось добавить заявку с сайта: {e.Message}.");
                _processQueue.Remove(_taskname);
                return Task.FromException(e);
            }
        }
    }
}