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
    public class SmilarcompaniesCheckProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Company> _compRepo;
        private readonly CancellationToken _token;
        private readonly ProcessQueue _processQueue;
        private readonly int _companyNumber;
        private readonly Log _log;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public SmilarcompaniesCheckProcessor(int companyNumber, AmoAccount acc, ProcessQueue processQueue, Log log, CancellationToken token, RecentlyUpdatedEntityFilter filter)
        {
            _companyNumber = companyNumber;
            _compRepo = acc.GetRepo<Company>();
            _token = token;
            _log = log;
            _filter = filter;
            _processQueue = processQueue;
        }

        private List<int> _fields = new()
        {
            33577,
            33575,
            69123,
        };


        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_companyNumber.ToString());
                return Task.FromCanceled(_token);
            }

            try
            {
                Company company = _compRepo.GetById(_companyNumber);

                List<string> criteria = new();

                foreach (var f in _fields)
                    if (company.HasCF(f))
                    {
                        var value = company.GetCFStringValue(f);
                        var result = _compRepo.GetByCriteria($"query={value.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")}");
                        if (result.Any(x => x.id != company.id))
                            criteria.Add(value);
                    }

                if (criteria.Count == 0)
                {
                    _processQueue.Remove(_companyNumber.ToString());
                    return Task.CompletedTask;
                }

                StringBuilder sb = new StringBuilder("ВНИМАНИЕ! Проверьте компанию на дубли по следующим критериям: ");
                sb.AppendLine();

                foreach (var c in criteria)
                {
                    sb.AppendLine(c);
                }

                _compRepo.AddNotes(new Note() { entity_id = company.id, note_type = "common", parameters = new Note.Params() { text = sb.ToString() } });

                _filter.AddEntity(_companyNumber);
                _processQueue.Remove(_companyNumber.ToString());
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to check corp company for doubles {_companyNumber}: {e.Message}");
                _processQueue.Remove(_companyNumber.ToString());
                return Task.FromException(e);
            }
        }
    }
}