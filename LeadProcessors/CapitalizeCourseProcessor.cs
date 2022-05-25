using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class CapitalizeCourseProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;
        private readonly int _leadNumber;

        public CapitalizeCourseProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, int leadNumber)
        {
            _amo = amo;
            _gSheets = gSheets;
            _log = log;
            _processQueue = processQueue;
            _token = token;
            _taskName = taskName;
            _leadNumber = leadNumber;

            var acc = amo.GetAccountById(28395871);
            _leadRepo = acc.GetRepo<Lead>();
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
                #region Getting lead
                Lead lead = _leadRepo.GetById(_leadNumber);
                #endregion

                if (lead.HasCF(357005))
                {
                    string course = lead.GetCFStringValue(357005);

                    Lead newLead = new() { id = lead.id };
                    newLead.AddNewCF(357005, course.Trim().ToUpper());

                    _leadRepo.Save(newLead);
                }

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не удалось изменить название курса для сделки {_leadNumber}: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}