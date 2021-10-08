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
    public class RetailPaidProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;
        private readonly string _email;
        private readonly string _phone;
        private readonly int _price;

        public RetailPaidProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, string phone, string email, string price)
        {
            _log = log;
            _processQueue = processQueue;
            _token = token;
            _taskName = taskName;
            _phone = phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            _email = email.Trim().Replace(" ", "");
            int.TryParse(price, out _price);

            var acc = amo.GetAccountById(28395871);
            _contRepo = acc.GetRepo<Contact>();
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
                    throw new ArgumentException("Запрос без контактов");
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
                catch (Exception e) 
                { 
                    throw new InvalidOperationException($"Не удалось осуществить поиск похожих контактов: {e}"); 
                }
                #endregion

                if (!similarContacts.Any())
                    throw new InvalidOperationException("Не найдено подходящитх контактов в amoCRM.");

                foreach (var c in similarContacts.Distinct(new ContactsComparer()))
                    _contRepo.AddNotes((int)c.id, $"Поступила оплата в размере {_price} от контакта {_phone}, {_email}");

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не удалось учесть оплату для контакта {_phone}, {_email}: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}