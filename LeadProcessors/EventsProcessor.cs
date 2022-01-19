using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class EventsProcessor : ILeadProcessor
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

        public EventsProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, int leadNumber)
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
            _contRepo = acc.GetRepo<Contact>();
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

                if (!lead.HasCF(725709))
                    throw new InvalidOperationException("Lead has no event_name");
                #endregion

                #region Getting event properties
                string eventName = lead.GetCFStringValue(725709);

                EventProperties eventProperties = new MZPOEvent(eventName).GetPropertiesAsync().GetAwaiter().GetResult();
                #endregion

                #region Adding properties to lead
                Lead result = new Lead()
                {
                    id = _leadNumber
                };

                result.AddNewCF(725711, eventProperties.event_address);
                result.AddNewCF(725709, eventProperties.page_name);
                result.AddNewCF(724347, ((DateTimeOffset)eventProperties.vrema).ToUnixTimeSeconds());
                #endregion

                #region Setting pipeline and status
                int pipeline_id = 5055190;
                int status_id = 45469723;

                if (eventProperties.page_name.Contains("Пробный урок"))
                {
                    pipeline_id = 4586602;
                    status_id = 42430264;
                }

                if (eventProperties.page_name.Contains("День открытых дверей"))
                {
                    pipeline_id = 4586602;
                    status_id = 42210046;
                }

                result.pipeline_id = pipeline_id;
                result.status_id = status_id;
                #endregion

                #region Saving lead
                _leadRepo.Save(result);
                _log.Add($"Информация о событии сохранена в сделку {_leadNumber}"); 
                #endregion

                #region Getting contact data
                if (lead._embedded?.contacts is null &&
                    !lead._embedded.contacts.Any())
                    throw new InvalidOperationException("Lead has no contacts");

                Contact contact = _contRepo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string email = contact.GetCFStringValue(264913);
                #endregion

                #region Adding to google sheets
                GSheetsProcessor leadProcessor = new(_leadNumber, _amo, _gSheets, _processQueue, _log, _token);
                leadProcessor.Webinar(eventProperties.vrema.ToShortDateString(), eventProperties.page_name, 0, name, phone, email).Wait();
                _log.Add($"Добавлены данные о сделке {_leadNumber} в таблицу."); 
                #endregion

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не удалось зарегистировать событие для сделки {_leadNumber}: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}