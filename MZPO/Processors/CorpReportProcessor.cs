using MZPO.AmoRepo;
using MZPO.Services;
using OfficeOpenXml;
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

        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана"),
            (2375122, "Васина Елена"),
            (2375131, "Алферова Лилия"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (6630727, "Елена Зубатых")//,
            //(3770773, "Шталева Лидия"),
            //(6200629, "Харшиладзе Леван"),
            //(6346882, "Мусихина Юлия")
        };
        #endregion

        #region Realization
        public async void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("0");
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

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

                var worksheet = package.Workbook.Worksheets.Add(m.Item2);

                #region Header
                worksheet.Cells["A1"].Value = "Оганизация";
                worksheet.Cells["B1"].Value = "Назначение платежа";
                worksheet.Cells["C1"].Value = "Кол-во человек";
                worksheet.Cells["D1"].Value = "Стоимость, руб.";
                worksheet.Cells["E1"].Value = "Сумма, руб.";
                worksheet.Cells["F1"].Value = "Дата прихода";
                worksheet.Cells["G1"].Value = "Расчет";
                worksheet.Cells["H1"].Value = "Исполнитель";
                worksheet.Cells["I1"].Value = "Номер сделки";
                worksheet.Cells["J1"].Value = "% сделки";
                worksheet.Cells["K1"].Value = "Вознаграждение";
                #endregion

                int row = 1;
                foreach (var l in leads)
                {
                    if (_token.IsCancellationRequested) return;
                    row++;

                    #region Оганизация
                    if (l._embedded.companies.Any())
                        worksheet.Cells[$"A{row}"].Value = compRepo.GetById(l._embedded.companies.FirstOrDefault().id).name;
                    #endregion

                    #region Назначение платежа
                    if (l.custom_fields_values.Any(x => x.field_id == 118509))
                        worksheet.Cells[$"B{row}"].Value = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118509).values[0].value;
                    #endregion

                    #region Кол-во человек
                    int students;
                    if (l.custom_fields_values.Any(x => x.field_id == 611005))
                        Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 611005).values[0].value, out students);
                    else
                        students = 1;
                    worksheet.Cells[$"C{row}"].Value = students;
                    #endregion

                    #region Стоимость
                    worksheet.Cells[$"D{row}"].Formula = $"E{row}/C{row}";
                    #endregion

                    #region Сумма
                    worksheet.Cells[$"E{row}"].Value = l.price;
                    #endregion

                    #region Дата прихода
                    long payment_date_unix;
                    if (l.custom_fields_values.Any(x => x.field_id == 118675))
                        payment_date_unix = (long)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118675).values[0].value;
                    else
                        payment_date_unix = 0;
                    worksheet.Cells[$"F{row}"].Value = DateTimeOffset.FromUnixTimeSeconds(payment_date_unix).UtcDateTime.AddHours(3);
                    #endregion

                    #region Расчет
                    if (l.custom_fields_values.Any(x => x.field_id == 118545))
                        worksheet.Cells[$"G{row}"].Value = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118545).values[0].value;
                    #endregion

                    #region Испольнитель
                    if (l.custom_fields_values.Any(x => x.field_id == 162301))
                        worksheet.Cells[$"H{row}"].Value = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 162301).values[0].value;
                    #endregion

                    #region Номер сделки
                    worksheet.Cells[$"I{row}"].Value = l.id;
                    #endregion

                    #region % сделки
                    int percent;
                    if (l.custom_fields_values.Any(x => x.field_id == 613663))
                        Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 613663).values[0].value, out percent);
                    else
                        percent = 0;
                    worksheet.Cells[$"J{row}"].Value = percent;
                    #endregion

                    #region Вознаграждение
                    worksheet.Cells[$"K{row}"].Formula = $"E{row}*J{row}/100";
                    #endregion
                }

                #region Format

                worksheet.Cells[$"D2:E{row}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"K2:K{row}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"F2:F{row}"].Style.Numberformat.Format = "mm-dd-yy";

                worksheet.Column(1).Width = 35.84;
                worksheet.Column(2).Width = 11.2;
                worksheet.Column(3).Width = 7;
                worksheet.Column(4).Width = 9.8;
                worksheet.Column(5).Width = 11.76;
                worksheet.Column(6).Width = 13.44;
                worksheet.Column(7).Width = 12.6;
                worksheet.Column(8).Width = 13.44;
                worksheet.Column(9).Width = 14;
                worksheet.Column(10).Width = 9.24;
                worksheet.Column(11).Width = 16.8;

                worksheet.Cells["A1:K1"].Style.Font.Bold = true;
                worksheet.Cells[$"A{row + 1}:K{row + 1}"].Style.Font.Bold = true;
                #endregion

                #region Finals
                row++;

                worksheet.Cells[$"A{row}"].Value = "Итого:";
                worksheet.Cells[$"E{row}"].Formula = $"SUM(E2:E{row - 1})";
                worksheet.Cells[$"K{row}"].Formula = $"SUM(K2:K{row - 1})";

                worksheet.Calculate();
                #endregion

                GC.Collect();
            }

            #region Saving file
            package.Workbook.Properties.Title = "Отчёт о продажах корпоративного отдела";
            package.Workbook.Properties.Author = "mzpo2amo";

            package.Workbook.Properties.Company = "МЦПО";

            await package.SaveAsAsync(new FileInfo("report.xlsx"));
            #endregion

            _processQueue.Remove("report_corp");
        }
        #endregion
    }
}