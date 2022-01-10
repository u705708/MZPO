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
    public class CheckAdmissionProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;
        private readonly Webinars _webinars;
        private readonly int _leadNumber;

        public CheckAdmissionProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, Webinars webinars, string taskName, int leadNumber)
        {
            _log = log;
            _processQueue = processQueue;
            _token = token;
            _taskName = taskName;
            _webinars = webinars;
            _leadNumber = leadNumber;

            var acc = amo.GetAccountById(28395871);
            _leadRepo = acc.GetRepo<Lead>();
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
                Lead lead = _leadRepo.GetById(_leadNumber);

                long eventSessionId = lead.GetCFIntValue(725633);
                long participationId = lead.GetCFIntValue(725635);
                long eventDateTime = lead.GetCFIntValue(725629);

                var users = _webinars.GetParticipants(eventSessionId).Result;

                bool visited = false;

                if (users.Any(x => x.id == participationId && x.visited))
                    visited = true;

                if (lead.pipeline_id == 4977523)
                    _leadRepo.Save(new Lead() { 
                        id = lead.id,
                        pipeline_id = lead.pipeline_id,
                        status_id = visited? 45252577 : 45252580
                    });

                if (lead.pipeline_id == 3199819)
                    _leadRepo.Save(new Lead()
                    {
                        id = lead.id,
                        pipeline_id = lead.pipeline_id,
                        status_id = visited ? 45252571 : 45252574
                    });

                string note = string.Format("Слушатель{0} присутствовал на вебинаре {1} - {2}", visited ? "" : " не", DateTimeOffset.FromUnixTimeSeconds(eventDateTime).UtcDateTime.AddHours(3).ToShortDateString(), DateTimeOffset.FromUnixTimeSeconds(eventDateTime).UtcDateTime.AddHours(3).ToShortTimeString());

                _leadRepo.AddNotes(_leadNumber, note);

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не удалось проверить участие в вебинаре для сделки {_leadNumber}: {e.Message}");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}