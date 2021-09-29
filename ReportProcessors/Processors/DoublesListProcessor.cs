using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    internal class DoublesListProcessor : AbstractReportProcessor, IReportProcessor
    {
        private readonly(int, int) dataRange;
        private readonly object _locker;

        internal DoublesListProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            dataRange =((int)dateFrom, (int)dateTo);
            _locker = new();
        }

        private class ContactsComparer : IEqualityComparer<Contact>
        {
            public bool Equals(Contact x, Contact y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Contact c)
            {
                if (c is null) return 0;

                int hashProductCode = (int)c.id;

                return hashProductCode;
            }
        }

        private static CellData[] GetCellData(int A, string B)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = A},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = B},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            };
        }

        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskId);
                return;
            }

            var contRepo = _acc.GetRepo<Contact>();

            var criteria = $"filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}";

            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";

            _processQueue.UpdateTaskName($"{_taskId}", $"Doubles check: {dates}, getting contacts");

            IEnumerable<Contact> contacts = contRepo.GetByCriteria(criteria);

            _processQueue.UpdateTaskName($"{_taskId}", $"Doubles check: {dates}");

            List<(int, string)> doubleContacts = new();

            int i = 0;

            Parallel.ForEach(
                contacts,
                new ParallelOptions { MaxDegreeOfParallelism = 12 },
                c =>
                {
                    if (_token.IsCancellationRequested)
                    {
                        return;
                    }

                    i++;

                    if (i % 60 == 0)
                        GC.Collect();

                    List<int> contactsWithSimilarPhone = new();
                    List<int> contactsWithSimilarMail = new();

                    if (c.custom_fields_values is null) return;

                    if (c.custom_fields_values.Any(x => x.field_id == 264911))
                        foreach (var v in c.custom_fields_values.First(x => x.field_id == 264911).values)
                            if ((string)v.value != "" &&
                                (string)v.value != "0")
                                contactsWithSimilarPhone.AddRange(contRepo.GetByCriteria($"query={v.value}").Select(x => (int)x.id));

                    if (c.custom_fields_values.Any(x => x.field_id == 264913))
                        foreach (var v in c.custom_fields_values.First(x => x.field_id == 264913).values)
                            if ((string)v.value != "" &&
                                (string)v.value != "0")
                                contactsWithSimilarMail.AddRange(contRepo.GetByCriteria($"query={v.value}").Select(x => (int)x.id));

                    if (contactsWithSimilarPhone.Distinct().Count() > 1)
                        lock (_locker) doubleContacts.Add(((int)c.id, c.GetCFStringValue(264911).Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")));
                    if (contactsWithSimilarMail.Distinct().Count() > 1)
                        lock (_locker) doubleContacts.Add(((int)c.id, c.GetCFStringValue(264913).Trim()));
                });

            _processQueue.UpdateTaskName($"{_taskId}", $"Doubles check: {dates}, finalizing results");

            var l1 = doubleContacts.GroupBy(x => x.Item1).Select(g => new { cid = g.Key, cont = g.First().Item2 }).ToList();
            var l2 = l1.GroupBy(x => x.cont).Select(g => new { cid = g.First().cid, cont = g.Key }).ToList();

            List<Request> requestContainer = new();

            foreach (var l in l2)
                requestContainer.Add(GetRowRequest(0, GetCellData(l.cid, l.cont)));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove(_taskId);
        }
    }
}