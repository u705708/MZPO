using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MZPO.Processors
{
    public class CorpReportProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly long _dateFrom;
        private readonly long _dateTo;
        private readonly BaseRepository<Lead> leadRepo;
        private readonly BaseRepository<Company> compRepo;
        protected readonly CancellationToken _token;

        public CorpReportProcessor(AmoAccount acc, TaskList processQueue, CancellationToken token, long dateFrom, long dateTo)
        {
            _acc = acc;
            _processQueue = processQueue;
            _dateFrom = dateFrom;
            _dateTo = dateTo;
            _token = token;
            leadRepo = _acc.GetRepo<Lead>();
            compRepo = _acc.GetRepo<Company>();
        }

        List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана"),
            (2375122, "Васина Елена"),
            (2375131, "Алферова Лилия"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (3953704, "Александр Пуцков")
        };
        #endregion

        #region Realization
        public void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("0");
                return;
            }

            foreach (var m in managers)
            {
                if (_token.IsCancellationRequested) return;

                var allLeads = leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={m.Item1}");

                var leads = allLeads.Where(x =>
                    (x.custom_fields_values != null) &&
                    (x.custom_fields_values.Any(y => y.field_id == 118675)) &&
                    ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value >= _dateFrom) &&
                    ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value <= _dateTo)
                    );

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using StreamWriter sw = new StreamWriter($"{m.Item2} {DateTime.Now.Month}-{DateTime.Now.Year}.csv", false, Encoding.GetEncoding("windows-1251"));
                sw.WriteLine("организация;назначение платежа;кол-во человек;стоимость, руб.;сумма, руб.;дата прихода;расчет;организация;номер сделки;% сделки;вознаграждение");

                foreach (var l in leads)
                {
                    if (_token.IsCancellationRequested) return;

                    string organization;
                    string payment_name;
                    int students;
                    long payment_date_unix;
                    DateTime payment_date;
                    string payment_type;
                    string provider;
                    int percent;

                    if (l._embedded.companies.Any())
                        organization = compRepo.GetById(l._embedded.companies.FirstOrDefault().id).name;
                    else
                        organization = "";

                    if (l.custom_fields_values.Any(x => x.field_id == 118509))
                        payment_name = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118509).values[0].value;
                    else
                        payment_name = "";

                    if (l.custom_fields_values.Any(x => x.field_id == 611005))
                        Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 611005).values[0].value, out students);
                    else
                        students = 1;

                    if (l.custom_fields_values.Any(x => x.field_id == 118675))
                        payment_date_unix = (long)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118675).values[0].value;
                    else
                        payment_date_unix = 0;
                    payment_date = DateTimeOffset.FromUnixTimeSeconds(payment_date_unix).UtcDateTime.AddHours(3);

                    if (l.custom_fields_values.Any(x => x.field_id == 118545))
                        payment_type = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118545).values[0].value;
                    else
                        payment_type = "";

                    if (l.custom_fields_values.Any(x => x.field_id == 162301))
                        provider = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 162301).values[0].value;
                    else provider = "";

                    if (l.custom_fields_values.Any(x => x.field_id == 613663))
                        Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 613663).values[0].value, out percent);
                    else
                        percent = 0;

                    sw.WriteLine($"{organization};{payment_name};{students};{l.price / students};{l.price};{payment_date.Day}.{payment_date.Month}.{payment_date.Year};{payment_type};{provider};{l.id};{percent};{l.price * percent / 100}");
                }
                GC.Collect();
            }
            _processQueue.Remove("0");
        }
        #endregion
    }
}