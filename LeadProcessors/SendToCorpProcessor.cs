using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{ 
    public class SendToCorpProcessor : ILeadProcessor
    {
        private readonly Log _log;
        private readonly int _leadNumber;
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;

        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly IAmoRepo<Company> _compRepo;

        private readonly IAmoRepo<Lead> _sourceLeadRepo;
        private readonly IAmoRepo<Contact> _sourceContRepo;

        public SendToCorpProcessor(Amo amo, Log log, TaskList processQueue, int leadNumber, CancellationToken token)
        {
            _log = log;
            _leadNumber = leadNumber;
            _processQueue = processQueue;
            _token = token;

            _leadRepo = amo.GetAccountById(19453687).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(19453687).GetRepo<Contact>();
            _compRepo = amo.GetAccountById(19453687).GetRepo<Company>();

            _sourceLeadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _sourceContRepo = amo.GetAccountById(28395871).GetRepo<Contact>();
        }

        private int GetResponsibleUserId(int id)
        {
            switch (id)
            {
                case 5761144:
                    return 2375131; //Алферова Лилия
                case 3903853:
                    return 2884132; //Ирина Сорокина
                default:
            return 2375146;
            }
        }

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                return Task.FromCanceled(_token);
            }
            try
            {
                #region Getting source entities
                Lead sourceLead = _sourceLeadRepo.GetById(_leadNumber);

                if (sourceLead._embedded is null ||
                    sourceLead._embedded.contacts is null ||
                    !sourceLead._embedded.contacts.Any())
                    return Task.CompletedTask;

                var sourceContacts = _sourceContRepo.BulkGetById(sourceLead._embedded.contacts.Select(x => (int)x.id));
                #endregion

                Lead lead = new()
                {
                    name = sourceLead.name,
                    responsible_user_id = GetResponsibleUserId((int)sourceLead.responsible_user_id),
                    _embedded = new()
                    {
                        tags = new()
                        {
                            new()
                            { name = "Сделка из розницы" }
                        }
                    }
                };

                List<Note> calls = new();
                List<Note> notes = new();

                foreach (var c in sourceContacts)
                {
                    #region Prepare contacts
                    string phone = "";
                    string email = "";

                    if (c.custom_fields_values is not null &&
                        c.custom_fields_values.Any(x => x.field_id == 264911))
                        phone = (string)c.custom_fields_values.First(x => x.field_id == 264911).values[0].value;

                    if (c.custom_fields_values is not null &&
                        c.custom_fields_values.Any(x => x.field_id == 264913))
                        email = (string)c.custom_fields_values.First(x => x.field_id == 264913).values[0].value;

                    phone = phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

                    if (phone == "" && email == "") continue;
                    #endregion

                    var contactNotes = _sourceContRepo.GetEntityNotes((int)c.id);
                    notes.AddRange(contactNotes.Where(x => x.note_type == "common"));
                    calls.AddRange(contactNotes.Where(x => x.note_type == "call_in" || x.note_type == "call_out"));

                    #region Checking for companies
                    List<Company> similarCompanies = new();
                    try
                    {
                        if (phone != "")
                            similarCompanies.AddRange(_compRepo.GetByCriteria($"query={phone}"));

                        if (email != "")
                            similarCompanies.AddRange(_compRepo.GetByCriteria($"query={email}"));
                    }
                    catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих компаний: {e}"); }

                    if (similarCompanies.Any() &&
                        lead._embedded.companies is null)
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

                    #region Checking for contacts
                    List<Contact> similarContacts = new();

                    Contact contact = new()
                    {
                        name = c.name,
                        responsible_user_id = 2375146,
                    };

                    try
                    {
                        if (phone != "")
                            similarContacts.AddRange(_contRepo.GetByCriteria($"query={phone}"));

                        if (email != "")
                            similarContacts.AddRange(_contRepo.GetByCriteria($"query={email}"));
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
                        _log.Add($"Найден похожий контакт: {similarContacts.First().id}.");
                    }
                    else
                    {
                        contact.custom_fields_values = new();

                        if (email != "")
                            contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                            {
                                field_id = 33577,
                                values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = email } }
                            });

                        if (phone != "")
                            contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                            {
                                field_id = 33575,
                                values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = phone } }
                            });
                    }

                    lead._embedded.contacts = new() { contact };
                    break;
                    #endregion
                }

                #region Setting pipeline and status if any
                lead.pipeline_id = 3558781;
                lead.status_id = 35001112;
                #endregion

                #region Getting comments
                var leadNotes = _sourceLeadRepo.GetEntityNotes(_leadNumber);
                notes.AddRange(leadNotes.Where(x => x.note_type == "common"));
                calls.AddRange(leadNotes.Where(x => x.note_type == "call_in" || x.note_type == "call_out"));

                StringBuilder sb = new();

                if (sourceLead.custom_fields_values is not null &&
                    sourceLead.custom_fields_values.Any(x => x.field_id == 639075))
                    sb.Append($"{sourceLead.custom_fields_values.First(x => x.field_id == 639075).values[0].value}\r\n");

                foreach (var n in notes)
                    sb.Append($"{n.parameters.text}\r\n");

                string comment = sb.ToString();
                #endregion

                #region Tags
                List<Tag> tags = new();
                if (sourceLead._embedded is not null &&
                    sourceLead._embedded.tags is not null &&
                    sourceLead._embedded.tags.Any())
                    foreach (var t in sourceLead._embedded.tags)
                        tags.Add(new() { name = t.name });

                if (tags.Any())
                    lead._embedded.tags.AddRange(tags);
                #endregion

                var created = _leadRepo.AddNewComplex(lead);

                #region Adding notes
                if (created.Any() &&
                    comment != "")
                    _leadRepo.AddNotes(new Note() {
                                                entity_id = created.First(),
                                                note_type = "common",
                                                parameters = new Note.Params() { text = $"{comment}" }
                    });

                if (created.Any() &&
                    calls.Any())
                    foreach (var n in calls.Select(x => new Note() {
                                                                entity_id = created.First(),
                                                                note_type = x.note_type,
                                                                parameters = x.parameters
                                                                    }))
                        _leadRepo.AddNotes(n);
                #endregion

                _log.Add($"Создана новая сделка {created.First()}");

                _processQueue.Remove($"ret2corp-{_leadNumber}");

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                _log.Add($"Не получилось перенести сделку {_leadNumber} из розницы в корп.: {e}.");
                return Task.FromException(e);
            }
        }
    }
}