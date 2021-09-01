using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Integration1C;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace MZPO.Controllers
{
    [Route("integration/1c/[action]")]
    public class Integration1CController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public Integration1CController(Amo amo, ProcessQueue processQueue, Log log, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _cred1C = cred1C;
            _filter = filter;
        }

        // POST: integration/1c/saveclient
        [HttpPost]
        [ActionName("SaveClient")]
        public IActionResult SaveClient1C()
        {
            var col = Request.Form;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            int leadNumber = 0;

            if (!col.ContainsKey("leads[status][0][id]") &&
                !col.ContainsKey("leads[add][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber) &&
                !Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CClient(_amo, _log, leadNumber, accNumber, _cred1C, _filter).Run());

            return Ok();
        }

        // POST: integration/1c/updateclient
        [HttpPost]
        [ActionName("UpdateClient")]
        public IActionResult UpdateClient1C()
        {
            var col = Request.Form;

            if (!col.Any(x => x.Value.ToString() == "client_id_1C"))
                return Ok();

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int contactNumber)) return BadRequest("Incorrect contact number.");

            if (!_filter.CheckEntityIsValid(contactNumber)) 
                return Ok();

            var task = Task.Run(() => new Update1CClient(_amo, _log, contactNumber, accNumber, _cred1C, _filter).Run());

            return Ok();
        }

        // POST: integration/1c/client
        [HttpPost]
        [ActionName("Client")]
        public IActionResult CreateClientAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Client1C client1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} POST integration/1c/client ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), client1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to client1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (client1C.client_id_1C is null)
                return BadRequest("Incorrect client_id_1C");

            if (string.IsNullOrEmpty(client1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(client1C.phone) &&
                string.IsNullOrEmpty(client1C.email))
                return BadRequest("Incorrect contacts");

            if (client1C.amo_ids is not null &&
                client1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");
            #endregion

            List<Amo_id> result = new();

            if (client1C.amo_ids is null ||
                !client1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoContact(client1C, _amo, _log, _filter).Run());
            else
                result.AddRange(new UpdateAmoContact(client1C, _amo, _log, _filter).Run());

            result.ForEach(x => _filter.AddEntity(x.entity_id));

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // PATCH: integration/1c/client
        [HttpPatch]
        [ActionName("Client")]
        public IActionResult UpdateClientAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Client1C client1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} PATCH integration/1c/client ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), client1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to client1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (client1C.client_id_1C is null)
                return BadRequest("Incorrect client_id_1C");

            if (string.IsNullOrEmpty(client1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(client1C.phone) &&
                string.IsNullOrEmpty(client1C.email))
                return BadRequest("Incorrect contacts");

            if (client1C.amo_ids is not null &&
                client1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");

            if (client1C.amo_ids is null ||
                !client1C.amo_ids.Any())
                return Ok(new List<Amo_id>());
            #endregion

            List<Amo_id> result = new();

            result.AddRange(new UpdateAmoContact(client1C, _amo, _log, _filter).Run());

            result.ForEach(x => _filter.AddEntity(x.entity_id));

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/savecompany
        [HttpPost]
        [ActionName("SaveCompany")]
        public IActionResult SaveCompany1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            int leadNumber = 0;

            if (!col.ContainsKey("leads[status][0][id]") &&
                !col.ContainsKey("leads[add][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber) &&
                !Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CCompany(_amo, _log, leadNumber, _cred1C, _filter).Run());

            return Ok();
        }

        // POST: integration/1c/updatecompany
        [HttpPost]
        [ActionName("UpdateCompany")]
        public IActionResult UpdateCompany1C()
        {
            var col = Request.Form;

            if (!col.Any(x => x.Value.ToString() == "company_id_1C"))
                return Ok();

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int companyNumber)) return BadRequest("Incorrect lead number.");

            if (!_filter.CheckEntityIsValid(companyNumber))
                return Ok();

            var task = Task.Run(() => new Update1CCompany(_amo, _log, companyNumber, _cred1C).Run());

            return Ok();
        }

        // POST: integration/1c/company
        [HttpPost]
        [ActionName("Company")]
        public IActionResult CreateCompanyAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Company1C company1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} POST integration/1c/company ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), company1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to company1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (company1C.company_id_1C is null)
                return BadRequest("Incorrect company_id_1C");

            if (string.IsNullOrEmpty(company1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(company1C.phone) &&
                string.IsNullOrEmpty(company1C.email) &&
                string.IsNullOrEmpty(company1C.INN))
                return BadRequest("Incorrect contacts/INN");

            if (company1C.amo_ids is not null &&
                company1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");
            #endregion

            List<Amo_id> result = new();

            if (company1C.amo_ids is null ||
                !company1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoCompany(company1C, _amo, _log, _filter).Run());
            else
                result.AddRange(new UpdateAmoCompany(company1C, _amo, _log, _filter).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // PATCH: integration/1c/company
        [HttpPatch]
        [ActionName("Company")]
        public IActionResult UpdateCompanyAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Company1C company1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} PATCH integration/1c/company ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), company1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to company1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (company1C.company_id_1C is null)
                return BadRequest("Incorrect company_id_1C");

            if (string.IsNullOrEmpty(company1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(company1C.phone) &&
                string.IsNullOrEmpty(company1C.email))
                return BadRequest("Incorrect contacts");

            if (company1C.amo_ids is not null &&
                company1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");

            if (company1C.amo_ids is null ||
                !company1C.amo_ids.Any())
                return Ok(new List<Amo_id>());
            #endregion

            List<Amo_id> result = new();

            result.AddRange(new UpdateAmoCompany(company1C, _amo, _log, _filter).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/savelead
        [HttpPost]
        [ActionName("SaveLead")]
        public IActionResult SaveLead1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            int leadNumber = 0;

            if (!col.ContainsKey("leads[status][0][id]") &&
                !col.ContainsKey("leads[add][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber) &&
                !Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CLead(_amo, _log, leadNumber, accNumber, _cred1C, _filter).Run());

            return Ok();
        }

        // POST: integration/1c/updatelead
        [HttpPost]
        [ActionName("UpdateLead")]
        public IActionResult UpdateLead1C()
        {
            var col = Request.Form;

            if (!col.Any(x => x.Value.ToString() == "lead_id_1C"))
                return Ok();

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            int leadNumber = 0;

            if (!col.ContainsKey("leads[status][0][id]") &&
                !col.ContainsKey("leads[add][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber) &&
                !Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");

            if (!_filter.CheckEntityIsValid(leadNumber))
                return Ok();

            var task = Task.Run(() => new Update1CLead(_amo, _log, leadNumber, accNumber, _cred1C).Run());

            return Ok();
        }

        // POST: integration/1c/lead
        [HttpPost]
        [ActionName("Lead")]
        public IActionResult CreateLeadAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Lead1C lead1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} POST integration/1c/lead ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), lead1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to lead1C: {e.Message}");
                return BadRequest($"Incorrect JSON");
            }

            if (lead1C.lead_id_1C is null)
                return BadRequest("Incorrect lead_id_1C");

            if (lead1C.client_id_1C == default ||
                lead1C.client_id_1C is null)
                return BadRequest("Incorrect client_id_1C");

            if (lead1C.product_id_1C == default ||
                lead1C.product_id_1C is null)
                return BadRequest("Incorrect product_id_1C");

            if (string.IsNullOrEmpty(lead1C.organization))
                return BadRequest("Incorrect organization");

            if (lead1C.is_corporate &&
                (lead1C.company_id_1C is null))
                return BadRequest("Incorrect company_id_1C");

            if (lead1C.amo_ids is not null &&
                lead1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");

            if (lead1C.organization != "ООО «МЦПО»" &&
                lead1C.organization != "ООО «МИРК»" &&
                lead1C.organization != "МЦПО" &&
                lead1C.organization != "ООО «Первый Профессиональный Институт Эстетики»") return BadRequest($"Unknown organization {lead1C.organization}");
            #endregion

            List<Amo_id> result = new();

            //if (lead1C.amo_ids is null ||
            //    !lead1C.amo_ids.Any())
            //    result.AddRange(new CreateOrUpdateAmoLead(lead1C, _amo, _log, _cred1C, _filter).Run());
            //else
            //    result.AddRange(new UpdateAmoLead(lead1C, _amo, _log).Run());

            if(lead1C.amo_ids is not null &&
                lead1C.amo_ids.Any())
                result.AddRange(new UpdateAmoLead(lead1C, _amo, _log, _filter).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // PATCH: integration/1c/lead
        [HttpPatch]
        [ActionName("Lead")]
        public IActionResult UpdateLeadAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Lead1C lead1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} PATCH integration/1c/lead ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), lead1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to lead1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (lead1C.lead_id_1C is null)
                return BadRequest("Incorrect lead_id_1C");

            if (lead1C.client_id_1C == default)
                return BadRequest("Incorrect client_id_1C");

            if (lead1C.product_id_1C == default)
                return BadRequest("Incorrect product_id_1C");

            if (string.IsNullOrEmpty(lead1C.organization))
                return BadRequest("Incorrect organization");

            if (lead1C.is_corporate &&
                (lead1C.company_id_1C is null))
                return BadRequest("Incorrect company_id_1C");

            if (lead1C.amo_ids is not null &&
                lead1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");

            if (lead1C.amo_ids is null ||
                !lead1C.amo_ids.Any())
                return Ok(new List<Amo_id>());
            #endregion

            List<Amo_id> result = new();

            result.AddRange(new UpdateAmoLead(lead1C, _amo, _log, _filter).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/course
        [HttpPost]
        [ActionName("Course")]
        public IActionResult CreateCourseAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Course1C course1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} POST integration/1c/course ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), course1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to course1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (course1C.product_id_1C is null)
                return BadRequest("Incorrect product_id_1C");

            if (string.IsNullOrEmpty(course1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(course1C.short_name))
                return BadRequest("Incorrect short_name");

            if (course1C.amo_ids is not null &&
                course1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");
            #endregion

            List<Amo_id> result = new();

            if (course1C.amo_ids is null ||
                !course1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoCourse(course1C, _amo, _log).Run());
            else
                result.AddRange(new UpdateAmoCourse(course1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // PATCH: integration/1c/course
        [HttpPatch]
        [ActionName("Course")]
        public IActionResult UpdateCourseAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Course1C course1C = new();

            #region Adding to log
            using StreamWriter sw = new($"integration1C_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} PATCH integration/1c/course ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(request));
            sw.WriteLine();
            #endregion

            #region Parsing request
            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), course1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to course1C: {e}");
                return BadRequest($"Incorrect JSON");
            }

            if (course1C.product_id_1C is null)
                return BadRequest("Incorrect product_id_1C");

            if (string.IsNullOrEmpty(course1C.name))
                return BadRequest("Incorrect name");

            if (string.IsNullOrEmpty(course1C.short_name))
                return BadRequest("Incorrect short_name");

            if (course1C.amo_ids is not null &&
                course1C.amo_ids.Any(x => x.account_id == 0 || x.entity_id == 0))
                return BadRequest("amo_id values cannot be 0");

            if (course1C.amo_ids is null ||
                !course1C.amo_ids.Any())
                return Ok(new List<Amo_id>());
            #endregion

            List<Amo_id> result = new();

            result.AddRange(new UpdateAmoCourse(course1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/paymentreceived
        [HttpPost]
        [ActionName("PaymentReceived")]
        public IActionResult ProcessPayment()
        {
            using StreamReader sr = new(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/paymentreceived");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // POST: integration/1c/courseended
        [HttpPost]
        [ActionName("CourseEnded")]
        public IActionResult FinishCourse()
        {
            using StreamReader sr = new(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/courseended");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }
    }
}