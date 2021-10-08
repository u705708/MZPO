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
    public class ConferencePaidProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;
        private readonly string _email;
        private readonly string _phone;

        public ConferencePaidProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, string phone, string email)
        {
            _amo = amo;
            _log = log;
            _processQueue = processQueue;
            _token = token;
            _gSheets = gSheets;
            _taskName = taskName;
            _phone = phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            _email = email.Trim().Replace(" ", "");

            var acc = amo.GetAccountById(19453687);
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
            _compRepo = acc.GetRepo<Company>();
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
                _processQueue.Remove(_taskName);
                return Task.FromCanceled(_token);
            }
            try
            {
                #region Checking for contacts
                if (!IsValidField(_email) &&
                    !IsValidField(_phone))
                {
                    _log.Add("Request without contacts");
                    _processQueue.Remove(_taskName);
                    return Task.CompletedTask;
                }
                #endregion

                #region Checking company
                List<Company> similarCompanies = new();
                try
                {
                    if (IsValidField(_phone))
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_phone}&with=leads"));

                    if (IsValidField(_email))
                        similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_email}&with=leads"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих компаний: {e}"); }
                #endregion

                #region Checking contact
                List<Contact> similarContacts = new();

                try
                {
                    if (IsValidField(_phone))
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_phone}&with=leads"));

                    if (IsValidField(_email))
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_email}&with=leads"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих контактов: {e}"); }
                #endregion

                #region Getting similar leads
                List<Lead> similarLeads = new();
                List<int> leadIds = new();

                foreach (var c in similarCompanies)
                    if (c._embedded is not null &&
                        c._embedded.leads is not null)
                        leadIds.AddRange(c._embedded.leads.Select(x => x.id));

                foreach (var c in similarContacts)
                    if (c._embedded is not null &&
                        c._embedded.leads is not null)
                        leadIds.AddRange(c._embedded.leads.Select(x => x.id));

                if (!leadIds.Any())
                {
                    _log.Add($"Не удалось учесть оплату для контакта {_phone}, {_email} - Не найдено сделок");
                    _processQueue.Remove(_taskName);
                    return Task.CompletedTask;
                }

                similarLeads.AddRange(_leadRepo.BulkGetById(leadIds.Distinct()).Where(x => x.pipeline_id == 4697182 && x.status_id == 43009222));
                
                if (!similarLeads.Any())
                {
                    _log.Add($"Не удалось учесть оплату для контакта {_phone}, {_email} - Не найдено неоплаченных сделок");
                    _processQueue.Remove(_taskName);
                    return Task.CompletedTask;
                }
                #endregion

                Lead lead = new() {
                    id = similarLeads.First().id,
                    pipeline_id = 4697182,
                    status_id = 43009228
                };

                try
                {
                    _leadRepo.Save(lead);
                }
                catch (Exception e)
                {
                    _log.Add($"Не удалось сохранить сделку {lead.id}: {e}.");
                    throw;
                }

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не удалось учесть оплату для сделки: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}