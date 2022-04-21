using Google.Apis.Sheets.v4;
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
    internal abstract class AbstractReportProcessor : IReportProcessor
    {
        protected readonly ProcessQueue _processQueue;
        protected readonly AmoAccount _acc;
        protected readonly SheetsService _service;
        protected readonly string _spreadsheetId;
        protected readonly IAmoRepo<Lead> _leadRepo;
        protected readonly IAmoRepo<Company> _compRepo;
        protected readonly IAmoRepo<Contact> _contRepo;
        protected readonly CancellationToken _token;
        protected readonly int _dateFrom;
        protected readonly int _dateTo;
        protected readonly string _taskId;

        protected List<(int?, int, long, int?)> _longAnsweredLeads;

        internal AbstractReportProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskId, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _token = token;
            _service = gSheets.GetService();
            _spreadsheetId = spreadsheetId;
            _leadRepo = _acc.GetRepo<Lead>();
            _compRepo = _acc.GetRepo<Company>();
            _contRepo = _acc.GetRepo<Contact>();
            _dateFrom = (int)dateFrom;
            _dateTo = (int)dateTo;
            _taskId = taskId;
        }

        protected static readonly (int, string)[] managersRet = {
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (7448173, "Инна Апостол"),
            (7744360, "Володина Мария"),
            (2375152, "Карен Оганисян"),
            (3813670, "Федорова Александра"),
            (6102562, "Валерия Лукьянова"),
            (6929800, "Саида Исмаилова"),
            (7358368, "Лидия Ковш"),
            (7771945, "Сиренко Оксана"),
            (7744360, "Володина Мария"),
            (7824505, "Климчукова Жанна"),
            (8101333, "Орлова Мария"),
        };

        protected static readonly (int, string)[] managersCorp = {
            (2375116, "Киреева Светлана"),
            (7358626, "Саланович Эллада"),
            (2375131, "Алферова Лилия"),
            (6028753, "Алена Федосова"),
            (6697522, "Наталья Филатова"),
            (2884132, "Ирина Сорокина"),
            (7523557, "Бекташева Ленара"),
            (7532620, "Лоскутова Анастасия"),
        };

        protected static readonly int[] pipelinesRet = {
            3198184,    //Продажи(Розница)
            3566374,    //Отложенные
            3558964,    //Реанимация
            3558991,    //Вызревание
        };

        protected class Calls
        {
#pragma warning disable IDE1006 // Naming Styles
            public IEnumerable<Event> inCalls { get; private set; }
            public IEnumerable<Event> outCalls { get; private set; }
#pragma warning restore IDE1006 // Naming Styles

            public Calls((int, int) dataRange, IAmoRepo<Contact> contRepo, int manager_id)
            {
                List<Task> tasks = new();

                tasks.Add(Task.Run(() => outCalls = contRepo.GetEventsByCriteria($"filter[type]=outgoing_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[created_by][]={manager_id}")));
                tasks.Add(Task.Run(() => inCalls = contRepo.GetEventsByCriteria($"filter[type]=incoming_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[created_by][]={manager_id}")));

                Task.WhenAll(tasks).Wait();
            }
        }

        /// <summary>
        /// Вычисляет время ответа клиенту по сделке.
        /// </summary>
        /// <param name="lead">Объект сделки.</param>
        /// <param name="leadRepo">Репозиторий сделок.</param>
        /// <param name="contRepo">Репозиторий контактов.</param>
        /// <returns>Время ответа клиенту в секундах</returns>
        protected static long GetLeadResponseTime(Lead lead, IAmoRepo<Lead> leadRepo, IAmoRepo<Contact> contRepo)
        {
            List<long> replyTimestamps = new();

            long timeOfReference = (long)lead.created_at;

            #region Результат звонка
            string cfValue = lead.GetCFStringValue(644675);
            if (cfValue == "Принят" || cfValue == "Ручная сделка") return 0;
            #endregion

            #region Время суток
            var dt = DateTimeOffset.FromUnixTimeSeconds(timeOfReference).UtcDateTime;
            if (dt.Hour > 17)
                timeOfReference = ((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0).AddDays(1)).ToUnixTimeSeconds();
            else if (dt.Hour < 6)
                timeOfReference = ((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0)).ToUnixTimeSeconds();
            #endregion

            var allEvents = leadRepo.GetEntityEvents(lead.id).ToList();
            var allNotes = leadRepo.GetEntityNotes(lead.id).ToList();

            #region Смена ответственного
            var responsibilityChangeVents = allEvents.Where(x => x.type == "entity_responsible_changed");

            if (responsibilityChangeVents.Any(x => x.value_before[0].responsible_user.id == 2576764 &&                                          //Если меняли ответственного с Администартора
                x.value_after[0].responsible_user.id == lead.responsible_user_id))                                                              //На текущего
                timeOfReference = (long)responsibilityChangeVents.First(x => x.value_before[0].responsible_user.id == 2576764).created_at;

            else if (responsibilityChangeVents.Any(x => x.value_after[0].responsible_user.id == lead.responsible_user_id) &&                    //Если меняли на текущего
                responsibilityChangeVents.Any(x => x.value_before[0].responsible_user.id == 2576764))                                           //И с Администратора
                timeOfReference = (long)responsibilityChangeVents.First(x => x.value_before[0].responsible_user.id == 2576764).created_at;
            #endregion

            #region Собираем данные из контактов
            if (lead._embedded.contacts is not null)
                foreach(var contact in lead._embedded.contacts)
                {
                    allEvents.AddRange(contRepo.GetEntityEvents((int)contact.id).ToList());
                    allNotes.AddRange(contRepo.GetEntityNotes((int)contact.id).ToList());
                }
            #endregion

            #region Cообщения в чат
            foreach (var e in allEvents)
                if (e.type == "outgoing_chat_message" || e.type == "incoming_chat_message")
                    replyTimestamps.Add((long)e.created_at);
            #endregion

            #region Исходящее письмо
            foreach (var n in allNotes)
                if (n.note_type == "amomail_message" && n.parameters.income != true)
                    replyTimestamps.Add((long)n.created_at);
            #endregion

            #region Звонки
            foreach (var e in allEvents)
            {
                if ((e.type != "outgoing_call") && (e.type != "incoming_call")) continue;
                Note callNote = new();

                if (allNotes.Any(x => x.id == e.value_after[0].note.id))
                    callNote = allNotes.First(x => x.id == e.value_after[0].note.id);

                int duration = 0;

                if (callNote.parameters is not null && callNote.parameters.duration > 0)
                    duration = (int)callNote.parameters.duration;

                long actualCallTime = (long)e.created_at - duration;

                if ((e.type == "outgoing_call") && (actualCallTime > lead.created_at))
                    replyTimestamps.Add(actualCallTime);
                else if ((duration > 0) && (actualCallTime > lead.created_at))
                    replyTimestamps.Add(actualCallTime);
            }
            #endregion

            replyTimestamps.Add(timeOfReference + 86400);

            return replyTimestamps.AsParallel().Select(x => x - timeOfReference).Where(x => x > -600).Min();
        }

        /// <summary>
        /// Вычисляет среднее время ответа клиенту для списка сделок.
        /// </summary>
        /// <param name="leads">Список сделок.</param>
        /// <param name="longAnsweredLeads">Список в который добавляются сделки с временем ответа более часа.</param>
        /// <param name="leadRepo">Репозиторий сделок.</param>
        /// <param name="contRepo">Репозиторий контактов.</param>
        /// <returns>Среднее время ответа клиенту в секундах</returns>
        protected static double GetAverageResponseTime(IEnumerable<Lead> leads, List<(int?, int, long, int?)> longAnsweredLeads, IAmoRepo<Lead> leadRepo, IAmoRepo<Contact> contRepo)
        {
            List<long> responseTimes = new();

            Parallel.ForEach(
                leads,
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                x => {
                    var rTime = GetLeadResponseTime(x, leadRepo, contRepo);
                    responseTimes.Add(rTime);


                    if (rTime > 3600)
                    {
                        lock (longAnsweredLeads)
                        {
                            longAnsweredLeads.Add((x.responsible_user_id, x.id, rTime, x.status_id));
                        }
                    }
                });

            if (responseTimes.AsParallel().Any(x => (x > 0) && (x < 3600)))
                return responseTimes.AsParallel().Where(x => (x > 0) && (x < 3600)).Average();
            return 0;
        }

        /// <summary>
        /// Вычисляет среднее время ответа клиенту для списка сделок.
        /// </summary>
        /// <param name="leads">Список сделок.</param>
        /// <param name="leadRepo">Репозиторий сделок.</param>
        /// <param name="contRepo">Репозиторий контактов.</param>
        /// <returns>Среднее время ответа клиенту в секундах</returns>
        protected static double GetAverageResponseTime(IEnumerable<Lead> leads, IAmoRepo<Lead> leadRepo, IAmoRepo<Contact> contRepo)
        {
            List<long> responseTimes = new();

            Parallel.ForEach(
                leads,
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                x => {
                var rTime = GetLeadResponseTime(x, leadRepo, contRepo);
                responseTimes.Add(rTime);
            });

            if (responseTimes.AsParallel().Any(x => (x > 0) && (x < 3600)))
                return responseTimes.AsParallel().Where(x => (x > 0) && (x < 3600)).Average();
            return 0;
        }

        /// <summary>
        /// Обновляет гугл-таблицу переданным запросом.
        /// </summary>
        /// <param name="requestContainer">Список запросов на обновление.</param>
        /// <param name="service">Сервис google sheets.</param>
        /// <param name="spreadsheetId">Id таблицы для обновления.</param>
        /// <returns>Задачу обновления.</returns>
        protected static async Task UpdateSheetsAsync(List<Request> requestContainer, SheetsService service, string spreadsheetId)
        {
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = requestContainer
                };

                await service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).ExecuteAsync();
            }
        }

        /// <summary>
        /// Формирует запрос на добавление строки из списка ячеек.
        /// </summary>
        /// <param name="sheetId">Id листа, который необходимо обновить.</param>
        /// <param name="cells">Список ячеек, которые необходимо добавить.</param>
        /// <returns>Сформированный запрос на добавление.</returns>
        protected static Request GetRowRequest(int sheetId, CellData[] cells)
        {
            var rows = new List<RowData>
            {
                new RowData()
                {
                    Values = new List<CellData>(cells)
                }
            };

            return new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            };
        }

        public abstract Task Run();
    }
}