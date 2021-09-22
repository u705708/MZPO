using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;
        private readonly Uber _uber;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        private Object locker;

        public TestingController(Amo amo, ProcessQueue processQueue, GSheets gSheets, Log log, Cred1C cred1C, RecentlyUpdatedEntityFilter filter, Uber uber)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
            _cred1C = cred1C;
            _filter = filter;
            locker = new();
            _uber = uber;
        }

        public class Entry
        {
            public int payment_received;
            public int payment_amount;
            public string receipt_number;
            public string manager;
            public string client_name;
        }

        private static string GetManager(int id)
        {
            List<(int, string)> managersCorp = new()
            {
                (2375116, "Киреева Светлана"),
                (2375122, "Васина Елена"),
                (7358626, "Саланович Эллада"),
                (2375131, "Алферова Лилия"),
                (6630727, "Елена Зубатых"),
                (6028753, "Алена Федосова"),
                (6697522, "Наталья Филатова"),
                (2884132, "Ирина Сорокина"),
                (2375146, "Администратор")
            };

            return managersCorp.Any(x => x.Item1 == id) ? managersCorp.First(x => x.Item1 == id).Item2 : id.ToString();
        }

        private static bool CheckEventsRecent(List<(string, long)> events, DateTime refDT, out long lastContactEventTime)
        {
            lastContactEventTime = 0;

            if (!events.Any(e => e.Item1 == "outgoing_chat_message" ||
                                 e.Item1 == "incoming_chat_message" ||
                                 e.Item1 == "outgoing_call" ||
                                 e.Item1 == "incoming_call"))
                return false;

            lastContactEventTime = events.Where(e => e.Item1 == "outgoing_chat_message" ||
                                                     e.Item1 == "incoming_chat_message" ||
                                                     e.Item1 == "outgoing_call" ||
                                                     e.Item1 == "incoming_call")
                                             .Select(x => x.Item2)
                                             .Max();

            return DateTimeOffset.FromUnixTimeSeconds(lastContactEventTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckNotesRecent(List<(string, long)> notes, DateTime refDT, out long lastNoteEventTime)
        {
            lastNoteEventTime = 0;

            if (!notes.Any(n => n.Item1 == "amomail_message"))
                return false;

            lastNoteEventTime = notes.Where(n => n.Item1 == "amomail_message")
                                     .Select(x => x.Item2)
                                     .Max();

            return DateTimeOffset.FromUnixTimeSeconds(lastNoteEventTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckLeadRecent(Lead lead, DateTime refDT, out long leadCreatedTime, out bool completed)
        {
            leadCreatedTime = 0;
            completed = false;

            completed = lead.status_id == 142 || lead.status_id == 35001244 || lead.status_id == 19529785;

            if (lead.created_at is null)
                return false;

            leadCreatedTime = (long)lead.created_at;

            return DateTimeOffset.FromUnixTimeSeconds(leadCreatedTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckCompanyRecent(Company company, DateTime refDT, out long companyCreatedTime)
        {
            companyCreatedTime = 0;

            if (company.created_at is null)
                return false;

            companyCreatedTime = (long)company.created_at;

            return DateTimeOffset.FromUnixTimeSeconds(companyCreatedTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckCompanyTasks(IEnumerable<AmoTask> tasks)
        {
            return tasks.Any(x => x.is_completed == false);

            var now = DateTime.UtcNow.AddHours(3);
            var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);

            return tasks.Any(x => x.is_completed == false &&
                                  DateTimeOffset.FromUnixTimeSeconds(x.complete_till).UtcDateTime.AddHours(3) >= today.AddDays(-7) &&
                                  DateTimeOffset.FromUnixTimeSeconds(x.complete_till).UtcDateTime.AddHours(3) < today.AddDays(14) &&
                                  (x.created_by == 2375131 || x.created_by == 2884132)
            );
        }

        public class IdDate
        {
            public int id;
            public string date;
        }

        private static IEnumerable<(long, long)> GetPeriods()
        {
            DateTime fromDate = new DateTime(2021, 4, 8, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);
            DateTime toDate = fromDate.AddMonths(1).AddSeconds(-1);
            DateTime now = DateTime.Now;

            while (fromDate < now)
            {
                long dateFrom = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                long dateTo = ((DateTimeOffset)toDate).ToUnixTimeSeconds();

                yield return (dateFrom, dateTo);

                fromDate = fromDate.AddMonths(1);
                toDate = toDate.AddMonths(1);
            }

            yield break;
        }

        // GET: api/testing
        [EnableCors]
        [HttpGet]
        public IActionResult Get()
        {
            //var repo = _amo.GetAccountById(29490250).GetRepo<Lead>();
            var _repo = _amo.GetAccountById(28395871).GetRepo<Lead>();
            //var _repo = _amo.GetAccountById(19453687).GetRepo<Contact>();

            //return Ok(JsonConvert.SerializeObject(_repo.GetTags(), Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            //var updater = new UpdateAmoIds(_amo, _log, _cred1C);
            //var contact = _repo.GetById(34566059);

            //if (contact is not null)
            //    updater.Run(contact);

            //return Ok();

            int i = 0;

            while (i < 12)
            {
                i++;
                UberLead lead = new() {
                    leadId = i,
                    leadName = $"Новая сделка {i}",
                    leadUri = $"https://mzpoeducationsale.amocrm.ru/leads/detail/{i}"
                };

                _uber.AddToQueue(lead);

                //Task.Delay(TimeSpan.FromSeconds(20)).Wait();
            }

            return Ok("𓅮 𓃟 𓏵 𓀠𓀡");

            #region MassTagging

            //using StreamReader sr = new("comps.json");
            //List<int> comp_ids = new();
            //JsonConvert.PopulateObject(sr.ReadToEnd(), comp_ids);

            //var repo = _amo.GetAccountById(19453687).GetRepo<Company>();

            //Parallel.ForEach(
            //    comp_ids,
            //    new ParallelOptions { MaxDegreeOfParallelism = 6 },
            //    c =>
            //    {
            //        Company company = new()
            //        {
            //            id = c,
            //            _embedded = new()
            //            {
            //                tags = new()
            //                {
            //                    new() { id = 1213927 }
            //                }
            //            }
            //        };

            //        repo.Save(company);
            //    });

            #endregion MassTagging

            #region CorpParse

            //var _spreadsheetId = "1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU";
            //var range = "Сводные!A:F";
            //var _service = _gSheets.GetService();
            //var request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);
            //request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            //var values = request.Execute().Values;

            //List<Entry> data = new();

            //if (values is not null)
            //    foreach (var row in values)
            //    {
            //        if ((string)row[0] == "дата") continue;
            //        DateTime A = Convert.ToDateTime(row[0]);
            //        int B = (int)Convert.ToDouble(row[1]);
            //        string C = Convert.ToString(row[2]);
            //        string D = Convert.ToString(row[3]);
            //        string E = Convert.ToString(row[4]);
            //        string F = Convert.ToString(row[5]);

            //        data.Add(new()
            //        {
            //            payment_received = (int)((DateTimeOffset)A.AddHours(2)).ToUnixTimeSeconds(),
            //            payment_amount = B,
            //            receipt_number = E,
            //            manager = F,
            //            client_name = C //Распарсить
            //        });
            //    }

            //return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));

            #endregion CorpParse

            #region ActualizatonResponsibleCheck

            //var leadRepo = _amo.GetAccountById(28395871).GetRepo<Lead>();

            //List<(int, string)> managers = new List<(int, string)>
            //{
            //    (2375107, "Кристина Гребенникова"),
            //    (2375143, "Екатерина Белоусова"),
            //    (2976226, "Вера Гладкова"),
            //    (3835801, "Наталья Кубышина"),
            //    (6158035, "Анастасия Матюк"),
            //    (2375152, "Карен Оганисян"),
            //    (3813670, "Федорова Александра"),
            //    (6102562, "Валерия Лукьянова"),
            //    (6410290, "Вероника Бармина"),
            //    (6699043, "Татьяна Ганоу"),
            //    (6729241, "Серик Айбасов")
            //};

            //var d2 = new DateTime(2021, 2, 3).AddHours(2).AddSeconds(-1);
            //var d1 = new DateTime(2021, 2, 2).AddHours(2);
            //var du2 = (int)((DateTimeOffset)d2).ToUnixTimeSeconds();
            //var du1 = (int)((DateTimeOffset)d1).ToUnixTimeSeconds();

            //var criteria = $"filter[created_at][from]={du1}&filter[created_at][to]={du2}&filter[entity][]=lead&filter[type][]=entity_responsible_changed&filter[value_after][responsible_user_id]=6158035";
            //var eventsList = new List<Event>();
            //var result = leadRepo.GetEventsByCriteria(criteria);

            //if (result is not null)
            //    eventsList.AddRange(result);

            //var leadIdList = eventsList.Select(x => (x._embedded.entity.id, x.value_before[0].responsible_user.id));

            //List<Lead> newLeads = new();

            //Parallel.ForEach(leadIdList, l =>
            //{
            //    int responsibleId = l.Item2;
            //    var lead = leadRepo.GetById(l.Item1);
            //    if (lead.pipeline_id != 3558922) return;
            //    if (lead.responsible_user_id == responsibleId) return;
            //    if (!managers.Any(x => x.Item1 == responsibleId)) responsibleId = 2375107;

            //    Lead newLead = new()
            //    {
            //        id = lead.id,
            //        responsible_user_id = responsibleId
            //    };
            //    newLeads.Add(newLead);
            //});

            //int i = 0;
            //List<Lead> leadsToSave = new();

            //foreach (var l in newLeads)
            //{
            //    leadsToSave.Add(l);
            //    i++;
            //    if (i % 10 != 0) continue;
            //    leadRepo.Save(leadsToSave);
            //    leadsToSave = new();
            //}

            //return Ok();

            #endregion ActualizatonResponsibleCheck

            #region AddCourses

            //var repo = _amo.GetAccountById(28395871).GetRepo<Lead>();
            //var course_ids = repo
            //                    .GetCEs()
            //                    .Where(x => x.custom_fields is not null &&
            //                                x.custom_fields.Any(y => y.id == 710407))
            //                    .Select(x => {
            //                        Guid.TryParse(x.custom_fields.First(y => y.id == 710407).values[0].value, out Guid result);
            //                        return result; });

            //int j = 0;
            //List<Course1C> processedCourses = new();
            //List<(Guid, string)> errorList = new();

            //Parallel.ForEach(
            //    course_ids,
            //    new ParallelOptions { MaxDegreeOfParallelism = 8 },
            //    c =>
            //    {
            //        j++;
            //        try
            //        {
            //            processedCourses.Add(new PopulateCourses(_amo, _log, _cred1C).Run(c));
            //        }
            //        catch (Exception e)
            //        {
            //            errorList.Add((c, e.Message));
            //        }
            //    });

            //using StreamWriter sw1 = new StreamWriter("processed_courses.json", false, System.Text.Encoding.Default);
            //sw1.WriteLine(JsonConvert.SerializeObject(processedCourses));

            //using StreamWriter sw2 = new StreamWriter("errors.csv", false, System.Text.Encoding.Default);
            //foreach (var e in errorList)
            //    sw2.WriteLine($"{e.Item1};{e.Item2}");

            //return Ok();

            #endregion AddCourses

            #region AbandonedCompanies

            //string startDT = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

            //var contRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
            //var compRepo = _amo.GetAccountById(19453687).GetRepo<Company>();
            //var leadRepo = _amo.GetAccountById(19453687).GetRepo<Lead>();

            //List<(int, int)> dataranges = new()
            //{
            //    (1525104000, 1541001599),   //05.2018-10.2018
            //    (1541001600, 1572537599),   //11.2018-10.2019
            //    (1572537600, 1588262399),   //11.2019-04.2020
            //    (1588262400, 1604159999),   //05.2020-10.2020
            //    (1604160000, 1619798399),   //11.2020-04.2021
            //    (1619798400, 1635695999),   //05.2021-10.2021
            //};

            //List<(int, string, string, string)> abandonedCompaniesResultsList = new();
            //DateTime referenceDateTime = DateTime.UtcNow.AddHours(3).AddMonths(-6);

            //IEnumerable<Company> companies = null;

            //foreach (var d in dataranges)
            //{
            //    if (companies is null)
            //    {
            //        companies = compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads");
            //        continue;
            //    }

            //    companies = companies.Concat(compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads"));
            //}

            //int i = 0;

            //Parallel.ForEach(
            //    companies,
            //    new ParallelOptions { MaxDegreeOfParallelism = 12 },
            //    c =>
            //    {
            //        i++;

            //        if (i % 60 == 0)
            //            GC.Collect();

            //        List<long> timeStamps = new();
            //        long contactTime = 0;

            //        #region Collecting company notes and events
            //        if (CheckCompanyRecent(c, referenceDateTime, out contactTime))
            //            return;
            //        timeStamps.Add(contactTime);

            //        if (CheckCompanyTasks(compRepo.GetEntityTasks(c.id)))
            //            return;

            //        if (CheckEventsRecent(compRepo.GetEntityEvents(c.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //            return;
            //        timeStamps.Add(contactTime);

            //        if (CheckNotesRecent(compRepo.GetEntityNotes(c.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //            return;
            //        timeStamps.Add(contactTime);
            //        #endregion

            //        #region Collecting associated leads notes and events
            //        if (c._embedded.leads is not null)
            //            foreach (var lead in c._embedded.leads.OrderByDescending(x => x.id))
            //            {
            //                if (CheckLeadRecent(leadRepo.GetById(lead.id), referenceDateTime, out contactTime))
            //                    return;
            //                timeStamps.Add(contactTime);

            //                if (CheckEventsRecent(leadRepo.GetEntityEvents(lead.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //                    return;
            //                timeStamps.Add(contactTime);

            //                if (CheckNotesRecent(leadRepo.GetEntityNotes(lead.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //                    return;
            //                timeStamps.Add(contactTime);
            //            }
            //        #endregion

            //        #region Collecting associated contacts notes and events
            //        if (c._embedded.contacts is not null)
            //            foreach (var contact in c._embedded.contacts.OrderByDescending(x => x.id))
            //            {
            //                if (CheckEventsRecent(contRepo.GetEntityEvents((int)contact.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //                    return;
            //                timeStamps.Add(contactTime);

            //                if (CheckNotesRecent(contRepo.GetEntityNotes((int)contact.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime))
            //                    return;
            //                timeStamps.Add(contactTime);
            //            }
            //        #endregion

            //        var lastContactTime = DateTimeOffset.FromUnixTimeSeconds(timeStamps.Max()).UtcDateTime.AddHours(3);

            //        abandonedCompaniesResultsList.Add((c.id, c.name.Replace(";", " ").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "), $"{lastContactTime.ToShortDateString()} {lastContactTime.ToShortTimeString()}", GetManager((int)c.responsible_user_id)));
            //    });

            //using StreamWriter sw = new("AC.csv", true, System.Text.Encoding.Default);

            //sw.WriteLine($"{startDT} -> {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            //sw.WriteLine($"ID компании;Название;Последний контакт;Ответственный");

            //foreach (var ac in abandonedCompaniesResultsList)
            //    sw.WriteLine($"{ac.Item1};{ac.Item2};{ac.Item3};{ac.Item4}");

            //return Ok(abandonedCompaniesResultsList.Count);

            #endregion AbandonedCompanies

            #region GetCorpContacts

            //IEnumerable<Contact> contacts = new List<Contact>();

            //foreach (var dataRange in GetPeriods())
            //{
            //    contacts = contacts.Concat(_repo.GetByCriteria($"filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&with=companies"));
            //}

            //Parallel.ForEach(
            //    contacts.Where(x => x._embedded is null ||
            //                        x._embedded.companies is null ||
            //                        !x._embedded.companies.Any()),
            //    new ParallelOptions { MaxDegreeOfParallelism = 8 },
            //    c => {
            //        if (c.created_by == 0/*2375146*/) return;

            //        string name = c.name.Replace("\n", "").Replace("\r", "");
            //        string dateCreated = DateTimeOffset.FromUnixTimeSeconds((long)c.created_at).UtcDateTime.AddHours(3).ToShortDateString();
            //        string phone = c.GetCFStringValue(33575).Replace("\n", "").Replace("\r", "").Replace(" ", "").Trim();
            //        string email = c.GetCFStringValue(33577).Replace("\n", "").Replace("\r", "").Replace(" ", "").Trim();

            //        if (!phone.Contains("@") &&
            //            !email.Contains("@"))
            //            return;

            //        lock (locker)
            //        {
            //            using StreamWriter sw = new("contacts.csv", true, System.Text.Encoding.Default);
            //            sw.WriteLine($"{dateCreated};{name};{phone};{email}");
            //        }
            //    });

            #endregion GetCorpContacts

            #region CompaniesLastContact

            //string startDT = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

            //var contRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
            //var compRepo = _amo.GetAccountById(19453687).GetRepo<Company>();
            //var leadRepo = _amo.GetAccountById(19453687).GetRepo<Lead>();

            //List<(int, int)> dataranges = new()
            //{
            //    (1525104000, 1541001599),   //05.2018-10.2018
            //    (1541001600, 1572537599),   //11.2018-10.2019
            //    (1572537600, 1588262399),   //11.2019-04.2020
            //    (1588262400, 1604159999),   //05.2020-10.2020
            //    (1604160000, 1619798399),   //11.2020-04.2021
            //    (1619798400, 1635695999),   //05.2021-10.2021
            //};

            //List<(int, string, string, string, bool)> abandonedCompaniesResultsList = new();
            //DateTime referenceDateTime = DateTime.UtcNow.AddHours(3).AddMonths(-6);

            //IEnumerable<Company> companies = null;

            //foreach (var d in dataranges)
            //{
            //    if (companies is null)
            //    {
            //        companies = compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads");
            //        continue;
            //    }

            //    companies = companies.Concat(compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads"));
            //}

            //int i = 0;

            //Parallel.ForEach(
            //    companies,
            //    new ParallelOptions { MaxDegreeOfParallelism = 12 },
            //    c =>
            //    {
            //        i++;

            //        if (i % 60 == 0)
            //            GC.Collect();

            //        List<long> timeStamps = new();
            //        long contactTime = 0;
            //        bool completed = false;

            //        #region Collecting company notes and events
            //        CheckCompanyRecent(c, referenceDateTime, out contactTime);
            //        timeStamps.Add(contactTime);

            //        CheckEventsRecent(compRepo.GetEntityEvents(c.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //        timeStamps.Add(contactTime);

            //        CheckNotesRecent(compRepo.GetEntityNotes(c.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //        timeStamps.Add(contactTime);
            //        #endregion

            //        #region Collecting associated leads notes and events
            //        if (c._embedded.leads is not null)
            //            foreach (var lead in c._embedded.leads.OrderByDescending(x => x.id))
            //            {
            //                CheckLeadRecent(leadRepo.GetById(lead.id), referenceDateTime, out contactTime, out completed);
            //                timeStamps.Add(contactTime);

            //                CheckEventsRecent(leadRepo.GetEntityEvents(lead.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //                timeStamps.Add(contactTime);

            //                CheckNotesRecent(leadRepo.GetEntityNotes(lead.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //                timeStamps.Add(contactTime);
            //            }
            //        #endregion

            //        #region Collecting associated contacts notes and events
            //        if (c._embedded.contacts is not null)
            //            foreach (var contact in c._embedded.contacts.OrderByDescending(x => x.id))
            //            {
            //                CheckEventsRecent(contRepo.GetEntityEvents((int)contact.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //                timeStamps.Add(contactTime);

            //                CheckNotesRecent(contRepo.GetEntityNotes((int)contact.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
            //                timeStamps.Add(contactTime);
            //            }
            //        #endregion

            //        var lastContactTime = DateTimeOffset.FromUnixTimeSeconds(timeStamps.Max()).UtcDateTime.AddHours(3);

            //        abandonedCompaniesResultsList.Add(
            //            (c.id,
            //             c.name.Replace(";", " ").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "),
            //             $"{lastContactTime.ToShortDateString()} {lastContactTime.ToShortTimeString()}",
            //             GetManager((int)c.responsible_user_id),
            //             completed
            //            ));
            //    });

            //using StreamWriter sw = new("AC.csv", true, System.Text.Encoding.Default);

            //sw.WriteLine($"{startDT} -> {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            //sw.WriteLine($"ID компании;Название;Последний контакт;Ответственный;Есть закрытая сделка");

            //foreach (var ac in abandonedCompaniesResultsList)
            //    sw.WriteLine($"{ac.Item1};{ac.Item2};{ac.Item3};{ac.Item4};{ac.Item5}");

            //return Ok(abandonedCompaniesResultsList.Count);

            #endregion CompaniesLastContact
        }

        // POST: api/testing
        [EnableCors]
        [HttpPost]
        public IActionResult Post()
        {
            using StreamReader sr = new(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now}----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine();

            return Request.Headers["x-requested-with"] == "XMLHttpRequest" ? Ok(new { Message = "SUCCESS" }) : Ok();
        }
    }
}