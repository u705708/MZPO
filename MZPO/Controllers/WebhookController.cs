using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.LeadProcessors;
using MZPO.Services;
using MZPO.webinar.ru;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh")]
    [Route("[controller]")]
    public class WebhookController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly RecentlyUpdatedEntityFilter _filter;
        private readonly GSheets _gSheets;
        private readonly Webinars _webinars;

        public WebhookController(Amo amo, ProcessQueue processQueue, Log log, RecentlyUpdatedEntityFilter filter, GSheets gSheets, Webinars webinars)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _filter = filter;
            _gSheets = gSheets;
            _webinars = webinars;
        }

        // GET wh/leadprocessor/5
        [Route("[action]/{id}")]
        [ActionName("LeadProcessor")]
        [HttpGet("{id}")]
        public IActionResult LeadProcessor(string id)                                                                                                      //Передаём вручную сделку в процессор (в дальнейшем заменить на POST)
        {
            if (!Int32.TryParse(id, out int leadNumber)) return BadRequest("Incorrect lead number.");

            Task task;
            var acc = _amo.GetAccountById(28395871);
            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;
            Lazy<ILeadProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new InitialLeadProcessor(leadNumber, acc, _amo, _gSheets, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"initial_{leadNumber}", acc.name, "WebHook");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/leadprocessor
        [Route("[action]")]
        [ActionName("LeadProcessor")]
        [HttpPost]
        public IActionResult LeadProcessor()
        {
            var col = Request.Form;
            int leadNumber = 0;
            AmoAccount acc;

            Lazy<ILeadProcessor> leadProcessor;
            Task task;
            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            #region Parsing hook
            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col["unsorted[delete][0][action]"] == "accept")                                                                            //Сделка принята из Неразобранного
            {
                if (!Int32.TryParse(col["unsorted[delete][0][accept_result][leads][0]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col.ContainsKey("unsorted[add][0][source_data][service]") &&                                                               //Сделка создана в Неразобранном
                     col["unsorted[add][0][source_data][service]"] != "com.wazzup24.wz" &&                                                      //Не из Wazzup
                     col["unsorted[add][0][source_data][service]"] != "com.wazzup24.insta")                                                     //Не из Wazzup
            {
                leadProcessor = new Lazy<ILeadProcessor>(() =>
                    new UnsortedProcessor(col["unsorted[add][0][uid]"], acc, _processQueue, _log, token));

                task = Task.Run(() => leadProcessor.Value.Run());
                _processQueue.AddTask(task, cts, $"unsorted_{col["unsorted[add][0][uid]"]}", acc.name, "WebHook");
                return Ok();
            }
            else return Ok();
            #endregion

            leadProcessor = new Lazy<ILeadProcessor>(() =>                                                                                     //Создаём экземпляр процессора сделки
                               new InitialLeadProcessor(leadNumber, acc, _amo, _gSheets, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"initial_{leadNumber}", acc.name, "WebHook");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ppie
        [Route("[action]")]
        [ActionName("PPIE")]
        [HttpPost]
        public IActionResult PPIE()
        {
            var col = Request.Form;
            int leadNumber = 0;
            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            #region Parsing hook
            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col["unsorted[delete][0][action]"] == "accept")                                                                            //Сделка принята из Неразобранного
            {
                if (!Int32.TryParse(col["unsorted[delete][0][accept_result][leads][0]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else return Ok();
            #endregion

            CancellationTokenSource cts = new();

            Lazy<ILeadProcessor> leadProcessor = new(() =>                                                                                     //Создаём экземпляр процессора сделки
                               new PPIELeadsProcessor(leadNumber, acc, _processQueue, _log, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"initial_{leadNumber}", acc.name, "WebHook");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ret/setcourse
        [Route("ret/[action]")]
        [ActionName("SetCourse")]
        [HttpPost]
        public IActionResult SetCourse()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<ILeadProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new RetailCourseProcessor(_amo, _processQueue, cts.Token, leadNumber, _log));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"setCourse-{leadNumber}", "mzpoeducationsale", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ret/respchange
        [Route("ret/[action]")]
        [ActionName("RespChange")]
        [HttpPost]
        public IActionResult RespChange()
        {
            var col = Request.Form;
            int entityNumber = 0;
            int oldRespId = 0;
            int modRespId = 0;
            Type type = default;

            if (col.ContainsKey("leads[responsible][0][id]") &&
                col.ContainsKey("leads[responsible][0][old_responsible_user_id]") &&
                col.ContainsKey("leads[responsible][0][modified_user_id]"))
            {
                if (!Int32.TryParse(col["leads[responsible][0][id]"], out entityNumber) ||
                    !Int32.TryParse(col["leads[responsible][0][old_responsible_user_id]"], out oldRespId) ||
                    !Int32.TryParse(col["leads[responsible][0][modified_user_id]"], out modRespId)) 
                    return BadRequest("Incorrect request.");

                type = typeof(Lead);
            }

            if (col.ContainsKey("contacts[update][0][id]") &&
                col.ContainsKey("contacts[update][0][old_responsible_user_id]") &&
                col.ContainsKey("contacts[update][0][modified_user_id]"))
            {
                if (!Int32.TryParse(col["contacts[update][0][id]"], out entityNumber) ||
                    !Int32.TryParse(col["contacts[update][0][old_responsible_user_id]"], out oldRespId) ||
                    !Int32.TryParse(col["contacts[update][0][modified_user_id]"], out modRespId))
                    return BadRequest("Incorrect request.");

                type = typeof(Contact);
            }

            if (type == default ||
                entityNumber == 0) 
                return BadRequest("Incorrect request.");

            CancellationTokenSource cts = new();

            Lazy<ILeadProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new RetailRespProcessor(_amo, _processQueue, cts.Token, entityNumber, _log, type, oldRespId, modRespId));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"setResp-{entityNumber}", "mzpoeducationsale", "WebHook");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: wh/corp/checkdouble
        [Route("corp/[action]")]
        [ActionName("CheckDouble")]
        [HttpPost]
        public IActionResult CheckDouble()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int companyNumber)) return BadRequest("Incorrect lead number.");

            if (!_filter.CheckEntityIsValid(companyNumber))
                return Ok();

            CancellationTokenSource cts = new();

            Lazy<ILeadProcessor> leadProcessor = new(() =>
                   new SmilarcompaniesCheckProcessor(companyNumber, acc, _processQueue, _log, cts.Token, _filter));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"compDouble-{companyNumber}", acc.name, "WebHook");

            return Ok();
        }

        // POST wh/ret2corp/send
        [Route("ret2corp/[action]")]
        [ActionName("Send")]
        [HttpPost]
        public IActionResult R2CSend()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Send());
            _processQueue.AddTask(task, cts, $"ret2corp-{leadNumber}", "ret2corp", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ret2corp/success
        [Route("ret2corp/[action]")]
        [ActionName("Success")]
        [HttpPost]
        public IActionResult R2CSuccess()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Success());
            _processQueue.AddTask(task, cts, $"corp2ret-{leadNumber}", "ret2corp", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ret2corp/fail
        [Route("ret2corp/[action]")]
        [ActionName("Fail")]
        [HttpPost]
        public IActionResult R2CFail()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Fail());
            _processQueue.AddTask(task, cts, $"corp2ret-{leadNumber}", "ret2corp", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/corp2ret/send
        [Route("corp2ret/[action]")]
        [ActionName("Send")]
        [HttpPost]
        public IActionResult C2RSend()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0)
                return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToRetProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToRetProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Send());
            _processQueue.AddTask(task, cts, $"corp2ret-{leadNumber}", "corp2ret", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/corp2ret/success
        [Route("corp2ret/[action]")]
        [ActionName("Success")]
        [HttpPost]
        public IActionResult C2RSuccess()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0)
                return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToRetProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToRetProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Success());
            _processQueue.AddTask(task, cts, $"ret2corp-{leadNumber}", "corp2ret", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/corp2ret/fail
        [Route("corp2ret/[action]")]
        [ActionName("Fail")]
        [HttpPost]
        public IActionResult C2RFail()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber))
                    return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0)
                return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<SendToRetProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToRetProcessor(_amo, _log, _processQueue, leadNumber, cts.Token));

            Task task = Task.Run(() => leadProcessor.Value.Fail());
            _processQueue.AddTask(task, cts, $"ret2corp-{leadNumber}", "corp2ret", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // GET wh/nightly
        [Route("[action]")]
        [ActionName("Nightly")]
        [HttpGet]
        public async Task<IActionResult> Nightly()
        {
            try
            {
                var leads = await ucheba.ru.LeadSource.GetLeads();

                List<FormRequest> formRequests = new();

                foreach (var lead in leads)
                {
                    if (lead is null ||
                        lead.person is null ||
                        lead.lastActivity is null ||
                        lead.lastActivity.type is null ||
                        lead.person.fullName == "" ||
                        (lead.person.email == "" && lead.person.phone == ""))
                        continue;

                    if (lead.lastActivity.type.value == "learning_request" &&
                        lead.lastActivity.learningRequest is not null &&
                        lead.lastActivity.learningRequest.program is not null)
                        formRequests.Add(new() {
                            name = lead.person.fullName,
                            phone = lead.person.phone,
                            email = lead.person.email,
                            form_name_site = "Заявка с формы Запись на обучение с сайта ucheba.ru",
                            site = "ucheba.ru",
                            comment = $"Заявка на обучение с сайта ucheba.ru по программе: {lead.lastActivity.learningRequest.program.name}"
                        });

                    if (lead.lastActivity.type.value == "question" &&
                        lead.lastActivity.question is not null)
                        formRequests.Add(new() {
                            name = lead.person.fullName,
                            phone = lead.person.phone,
                            email = lead.person.email,
                            form_name_site = "Заявка с формы Вопрос с сайта ucheba.ru",
                            site = "ucheba.ru",
                            comment = $"Вопрос с сайта ucheba.ru: {lead.lastActivity.question.text}"
                        });
                }

                int i = 0;

                foreach (var formRequest in formRequests)
                {
                    CancellationTokenSource cts = new();

                    string taskName = $"FormSiteRet-{i++}";

                    var leadProcessor = new Lazy<ILeadProcessor>(() =>
                           new SiteFormRetailProcessor(_amo, _log, formRequest, _processQueue, cts.Token, _gSheets, taskName, _webinars));

                    Task task = Task.Run(() => leadProcessor.Value.Run());
                    _processQueue.AddTask(task, cts, taskName, "ucheba.ru", "WebHook");
                }
            }
            catch (Exception e)
            {
                _log.Add($"ATTENTION! Unable to get ucheba.ru leads: {e.Message}");
            }

            return Ok();
        }

        // POST wh/conferencepaid
        [Route("[action]")]
        [ActionName("ConferencePaid")]
        [HttpPost]
        public IActionResult ConferencePaid()
        {
            if (Request.ContentType != "application/x-www-form-urlencoded")
                return BadRequest();

            var col = Request.Form;

            if (!col.ContainsKey("phone") ||
                !col.TryGetValue("phone", out var phone) ||
                !col.ContainsKey("email") ||
                !col.TryGetValue("email", out var email))
                return BadRequest();

            _log.Add($"Поступила информация об оплате от клиента {phone} {email}");

            CancellationTokenSource cts = new();

            string taskName = $"FormSiteRet-{DateTime.Now.ToLongTimeString()}";

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new ConferencePaidProcessor(_amo, _log, _processQueue, cts.Token, _gSheets, taskName, phone, email ));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, taskName, "conferencePaid", "WebHook");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST wh/retailpaid
        [Route("[action]")]
        [ActionName("RetailPaid")]
        [HttpPost]
        public IActionResult RetailPaid()
        {
            if (Request.ContentType != "application/x-www-form-urlencoded")
                return BadRequest();

            var col = Request.Form;

            if (!col.ContainsKey("phone") ||
                !col.TryGetValue("phone", out var phone) ||
                !col.ContainsKey("email") ||
                !col.TryGetValue("email", out var email) ||
                !col.ContainsKey("price") ||
                !col.TryGetValue("price", out var price))
                return BadRequest();

            _log.Add($"Поступила информация об оплате на сумму {price} от клиента {phone} {email}");

            CancellationTokenSource cts = new();

            string taskName = $"FormSiteRet-{DateTime.Now.ToLongTimeString()}";

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new RetailPaidProcessor(_amo, _log, _processQueue, cts.Token, _gSheets, taskName, phone, email, price));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, taskName, "retailPaid", "WebHook");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST wh/checkwebinaradmission
        [Route("[action]")]
        [ActionName("CheckWebinarAdmission")]
        [HttpPost]
        public IActionResult CheckWebinarAdmission()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<CheckAdmissionProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new CheckAdmissionProcessor(_amo, _log, _processQueue, cts.Token, _webinars, $"webinaradmission-{leadNumber}", leadNumber));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"webinaradmission-{leadNumber}", "webinars", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/processevents
        [Route("[action]")]
        [ActionName("ProcessEvents")]
        [HttpPost]
        public IActionResult ProcessEvents()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            CancellationTokenSource cts = new();

            Lazy<EventsProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new EventsProcessor(_amo, _log, _processQueue, cts.Token, _gSheets, $"eventsProcessor-{leadNumber}", leadNumber));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"eventsProcessor-{leadNumber}", "processors", "WebHook");                                            //Запускаем и добавляем в очередь
            return Ok();
        }
    }
}