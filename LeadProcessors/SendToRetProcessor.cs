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
    public class SendToRetProcessor
    {
        private readonly Log _log;
        private readonly int _leadNumber;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;

        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;

        private readonly IAmoRepo<Lead> _sourceLeadRepo;
        private readonly IAmoRepo<Contact> _sourceContRepo;

        public SendToRetProcessor(Amo amo, Log log, ProcessQueue processQueue, int leadNumber, CancellationToken token)
        {
            _log = log;
            _leadNumber = leadNumber;
            _processQueue = processQueue;
            _token = token;

            _leadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(28395871).GetRepo<Contact>();

            _sourceLeadRepo = amo.GetAccountById(19453687).GetRepo<Lead>();
            _sourceContRepo = amo.GetAccountById(19453687).GetRepo<Contact>();
        }

        private int GetResponsibleUserId(int id)
        {
            return 2576764;
        }

        public Task Send()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"corp2ret-{_leadNumber}");
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

                Lead lead = new() {
                    name = sourceLead.name,
                    responsible_user_id = GetResponsibleUserId((int)sourceLead.responsible_user_id),
                    _embedded = new()
                };

                List<Note> calls = new();
                List<Note> notes = new();

                foreach (var c in sourceContacts)
                {
                    #region Prepare contacts
                    string phone = c.GetCFStringValue(33575);
                    string email = c.GetCFStringValue(33577);

                    phone = phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                    phone = phone.StartsWith("89") ? $"7{phone[1..]}" : phone;

                    email = email.Trim().Replace(" ", "");

                    if (phone == "" && email == "")
                        continue;
                    #endregion

                    var contactNotes = _sourceContRepo.GetEntityNotes((int)c.id);
                    notes.AddRange(contactNotes.Where(x => x.note_type == "common"));
                    calls.AddRange(contactNotes.Where(x => x.note_type == "call_in" || x.note_type == "call_out"));

                    #region Checking for contacts
                    List<Contact> similarContacts = new();

                    Contact contact = new() {
                        name = c.name,
                        responsible_user_id = lead.responsible_user_id,
                    };

                    try
                    {
                        if (phone != "")
                            similarContacts.AddRange(_contRepo.GetByCriteria($"query={phone}"));

                        if (email != "")
                            similarContacts.AddRange(_contRepo.GetByCriteria($"query={email}"));
                    }
                    catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих контактов: {e}"); }

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
                            contact.AddNewCF(264913, email);

                        if (phone != "")
                            contact.AddNewCF(264911, phone);
                    }

                    lead._embedded.contacts = new() { contact };
                    break;
                    #endregion
                }

                #region Setting pipeline and status if any
                lead.pipeline_id = 3198184;
                lead.status_id = 32532880;
                #endregion

                #region Getting comments
                var leadNotes = _sourceLeadRepo.GetEntityNotes(_leadNumber);
                notes.AddRange(leadNotes.Where(x => x.note_type == "common"));
                calls.AddRange(leadNotes.Where(x => x.note_type == "call_in" || x.note_type == "call_out"));

                StringBuilder sb = new();

                if (sourceLead.HasCF(748383))     //Тип обращения
                    sb.Append($"{sourceLead.GetCFStringValue(748383)}\r\n");

                foreach (var n in notes)
                    sb.Append($"{n.parameters.text}\r\n");

                string comment = sb.ToString();
                #endregion

                #region Tags
                List<Tag> tags = new() { TagList.GetTagByName("Сделка из корп. отдела") };
                if (sourceLead._embedded is not null &&
                    sourceLead._embedded.tags is not null &&
                    sourceLead._embedded.tags.Any())
                    foreach (var t in sourceLead._embedded.tags)
                        tags.Add(new() { name = t.name });

                lead._embedded.tags = new(tags);
                #endregion

                #region Custom fields
                lead.AddNewCF(724771, sourceLead.id);       //поле corp_id

                if (sourceLead.HasCF(748383))               //Тип обращения
                    lead.AddNewCF(639075, sourceLead.GetCFValue(748383));
                if (sourceLead.HasCF(758213))               //Сайт
                    lead.AddNewCF(639081, sourceLead.GetCFValue(758213));
                if (sourceLead.HasCF(758215))               //Посадочная страница
                    lead.AddNewCF(639083, sourceLead.GetCFValue(758215));
                if (sourceLead.HasCF(748385))               //Маркер
                    lead.AddNewCF(639085, sourceLead.GetCFValue(748385));
                if (sourceLead.HasCF(758217))               //roistat
                    lead.AddNewCF(639073, sourceLead.GetCFValue(758217)); 
                #endregion

                var created = _leadRepo.AddNewComplex(lead);

                _log.Add($"Создана новая сделка {created.First()}");

                #region Adding notes
                if (created.Any() &&
                    comment != "")
                    _leadRepo.AddNotes(created.First(), comment);

                if (created.Any() &&
                    calls.Any())
                    foreach (var n in calls.Select(x => new Note() {
                                                                        entity_id = created.First(),
                                                                        note_type = x.note_type,
                                                                        parameters = x.parameters
                    }))
                        _leadRepo.AddNotes(n);
                #endregion

                _processQueue.Remove($"corp2ret-{_leadNumber}");

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"corp2ret-{_leadNumber}");
                _log.Add($"Не получилось перенести сделку {_leadNumber} из корп. в розницу: {e.Message}.");
                return Task.FromException(e);
            }
        }

        public Task Success()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                Lead lead = _leadRepo.GetById(_leadNumber);

                var retLeadId = lead.GetCFIntValue(724771);

                if (retLeadId == 0)
                {
                    _processQueue.Remove($"ret2corp-{_leadNumber}");
                    return Task.CompletedTask;
                }

                _sourceLeadRepo.Save(new Lead() {
                    id = retLeadId,
                    pipeline_id = 3558781,
                    status_id = 142
                });

                _processQueue.Remove($"ret2corp-{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                _log.Add($"Не получилось синхронизировать сделку {_leadNumber} из корп. в розницу: {e.Message}.");
                return Task.FromException(e);
            }
        }

        public Task Fail()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                Lead lead = _leadRepo.GetById(_leadNumber);

                var retLeadId = lead.GetCFIntValue(724771);

                if (retLeadId == 0)
                {
                    _processQueue.Remove($"ret2corp-{_leadNumber}");
                    return Task.CompletedTask;
                }

                _sourceLeadRepo.Save(new Lead() {
                    id = retLeadId,
                    pipeline_id = 3558781,
                    status_id = 143
                });

                _processQueue.Remove($"ret2corp-{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"ret2corp-{_leadNumber}");
                _log.Add($"Не получилось синхронизировать сделку {_leadNumber} из корп. в розницу: {e.Message}.");
                return Task.FromException(e);
            }
        }
    }
}