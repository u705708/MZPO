using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class PPIELeadsProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly int _leadNumber;
        private readonly Log _log;
        private Lead lead;

        public PPIELeadsProcessor(int leadNumber, AmoAccount acc, ProcessQueue processQueue, Log log, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _leadNumber = leadNumber;
            _log = log;

            try
            {
                Thread.Sleep((int)TimeSpan.FromSeconds(3).TotalMilliseconds);
                lead = _leadRepo.GetById(leadNumber);
            }
            catch (Exception e)
            {
                _processQueue.Stop(leadNumber.ToString());
                _processQueue.Remove(leadNumber.ToString());
                _log.Add($"Error: Unable to create leadProcessor {leadNumber}: {e.Message}");
            }
        }

        int[] managers = new[]
        {
            7074307,
            7074316,
            7074319
        };

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                bool model = false;

                var lead = _leadRepo.GetById(_leadNumber);

                if (lead?._embedded?.tags is not null &&
                    lead._embedded.tags.Any(x => x.name == "WA"))
                {
                    var events = _leadRepo.GetEntityEvents(_leadNumber);

                    if (events.Any(x => x.type == "incoming_chat_message" &&
                                        x.value_after is not null &&
                                        x.value_after.Any(x => x.message is not null &&
                                                               x.message.origin == "wahelp.whatbot.1")))
                        model = true;
                }

                _leadRepo.Save(new Lead()
                {
                    id = lead.id,
                    name = "Новая сделка",
                    pipeline_id = model ? 4647979 : 4289935,
                    status_id = model ? 42656578 : 40102252
                });

                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to process ppie lead {lead.id}: {e.Message}");

                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.FromException(e);
            }
        }
    }
}