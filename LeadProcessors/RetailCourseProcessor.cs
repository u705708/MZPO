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
    public class RetailCourseProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _repo;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly int _leadNumber;
        private readonly Log _log;

        public RetailCourseProcessor(Amo amo, ProcessQueue processQueue, CancellationToken token, int leadNumber, Log log)
        {
            _repo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _leadNumber = leadNumber;
            _log = log;
        }

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"setCourse-{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                Lead lead = _repo.GetById(_leadNumber);
                
                if (lead._embedded is not null &&
                    lead._embedded.catalog_elements is not null &&
                    lead._embedded.catalog_elements.Any())
                {
                    CatalogElement catalogElement = _repo.GetCEById(lead._embedded.catalog_elements.First().id);

                    if (catalogElement is null ||
                        catalogElement.custom_fields is null ||
                        !catalogElement.custom_fields.Any(x => x.id == 647993))
                        throw new Exception("Incorrect catalog element, no article field.");

                    Lead result = new(){ id = _leadNumber };
                    result.AddNewCF(357005, catalogElement.custom_fields.First(x => x.id == 647993).values[0].value);

                    _repo.Save(result);

                    _processQueue.Remove($"setCourse-{_leadNumber}");
                    return Task.CompletedTask;
                }

                _repo.AddNotes(new Note() { entity_id = _leadNumber, note_type = "common", parameters = new Note.Params() { text = "Укажите, пожалуйста, товар в сделке." } });

                var events = _repo.GetEntityEvents(_leadNumber)
                                      .Where(x => x.type == "lead_status_changed")
                                      .OrderByDescending(x => x.created_at)
                                      .ToList();

                if (!events.Any())
                {
                    _repo.Save(new Lead() { 
                        id = _leadNumber,
                        pipeline_id = 3198184,
                        status_id = 32532880
                    });

                    _processQueue.Remove($"setCourse-{_leadNumber}");
                    return Task.CompletedTask;
                }

                int recentPipelineId = events.First().value_before.First().lead_status.pipeline_id;
                int recentStatusId = events.First().value_before.First().lead_status.id;

                int[] forbiddenStatuses = new int[] {
                    142,
                    143,
                    32532886,
                    32533195,
                    32533198,
                    32533201,
                    32533204,
                    33625285,
                    33817816
                };

                if (!forbiddenStatuses.Contains(recentStatusId))
                    _repo.Save(new Lead()
                    {
                        id = _leadNumber,
                        pipeline_id = recentPipelineId,
                        status_id = recentStatusId 
                    });

                _processQueue.Remove($"setCourse-{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"setCourse-{_leadNumber}");
                _log.Add($"Не получилось установить курс в сделке {_leadNumber}: {e.Message}.");
                return Task.FromException(e);
            }
        }
    }
}