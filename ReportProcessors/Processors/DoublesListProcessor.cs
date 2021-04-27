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

        internal DoublesListProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            dataRange =((int)dateFrom, (int)dateTo);
        }

        //private readonly List<(int, int)> dataRanges = new()
        //{
        //    (1559336400, 1567285199),
        //    (1567285200, 1569877199),
        //    (1569877200, 1572555599),
        //    (1572555600, 1575147599), //01-30.11.2019 ---
        //    (1575147600, 1577825999),
        //    (1577826000, 1580504399),
        //    (1580504400, 1583009999),
        //    (1583010000, 1585688399),
        //    (1585688400, 1588280399),
        //    (1588280400, 1590958799),
        //    (1590958800, 1593550799),
        //    (1593550800, 1596229199),
        //    (1596229200, 1598907599),
        //    (1598907600, 1601499599),
        //    (1601499600, 1604177999),
        //    (1604178000, 1606769999),
        //    (1606770000, 1609448399),
        //    (1609448400, 1612126799),
        //    (1612126800, 1614545999),
        //    (1614546000, 1617224399),
        //    (1617224400, 1619798399),
        //};

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
                _processQueue.Remove(_taskName);
                return;
            }

            var contRepo = _acc.GetRepo<Contact>();

            var criteria = $"filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}";

            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";

            _processQueue.UpdateTaskName($"{_taskName}", $"Doubles check: {dates}, getting contacts");

            IEnumerable<Contact> contacts = contRepo.GetByCriteria(criteria);

            _processQueue.UpdateTaskName($"{_taskName}", $"Doubles check: {dates}");

            List<(int, string)> doubleContacts = new();

            int i = 0;

            Parallel.ForEach(
                contacts,
                new ParallelOptions { MaxDegreeOfParallelism = 12 },
                c =>
                {
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
                        doubleContacts.Add(((int)c.id, (string)c.custom_fields_values.First(x => x.field_id == 264911).values[0].value));
                    if (contactsWithSimilarMail.Distinct().Count() > 1)
                        doubleContacts.Add(((int)c.id, (string)c.custom_fields_values.First(x => x.field_id == 264913).values[0].value));
                });

            _processQueue.UpdateTaskName($"{_taskName}", $"Doubles check: {dates}, finalizing results");

            var l1 = doubleContacts.GroupBy(x => x.Item1).Select(g => new { cid = g.Key, cont = g.First().Item2 }).ToList();
            var l2 = l1.GroupBy(x => x.cont).Select(g => new { cid = g.First().cid, cont = g.Key }).ToList();

            List<Request> requestContainer = new();

            foreach (var l in l2)
                requestContainer.Add(GetRowRequest(0, GetCellData(l.cid, l.cont.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", ""))));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove(_taskName);
        }
    }
}