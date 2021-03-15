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

namespace MZPO.Controllers
{
    [Route("integration/1c/{action}")]
    public class Integration1CController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;

        public Integration1CController(Amo amo, TaskList processQueue, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
        }

        
        
        // POST: integration/1c/saveclient
        [HttpPost]
        [ActionName("SaveClient")]
        public IActionResult SaveClient1C()
        {
            var col = Request.Form;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CClientFromLead(leadNumber, accNumber, _amo, _log).Run());

            return Ok();
        }

        // POST: integration/1c/updateclient
        [HttpPost]
        [ActionName("UpdateClient")]
        public IActionResult UpdateClient1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int contactNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new Update1CClient(contactNumber, acc, _log).Run());

            return Ok();
        }

        // POST: integration/1c/client
        [HttpPost]
        [ActionName("Client")]
        public IActionResult CreateClientAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/client");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // PATCH: integration/1c/client
        [HttpPatch]
        [ActionName("Client")]
        public IActionResult UpdateClientAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("PATCH: integration/1c/client");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
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

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CCompanyFromLead(leadNumber, _amo, _log).Run());

            return Ok();
        }

        // POST: integration/1c/updateclient
        [HttpPost]
        [ActionName("UpdateClient")]
        public IActionResult UpdateCompany1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int companyNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new Update1CCompany(companyNumber, acc, _log).Run());

            return Ok();
        }

        // POST: integration/1c/company
        [HttpPost]
        [ActionName("Company")]
        public IActionResult CreateCompanyAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/company");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // PATCH: integration/1c/company
        [HttpPatch]
        [ActionName("Company")]
        public IActionResult UpdateCompanyAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("PATCH: integration/1c/company");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
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

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CLeadWithContacts(leadNumber, _amo, acc, _log).Run());

            return Ok();
        }

        // POST: integration/1c/updatelead
        [HttpPost]
        [ActionName("UpdateLead")]
        public IActionResult UpdateLead1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new Update1CLead(leadNumber, acc, _log).Run());

            return Ok();
        }

        // POST: integration/1c/lead
        [HttpPost]
        [ActionName("Lead")]
        public IActionResult CreateLeadAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/lead");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // PATCH: integration/1c/lead
        [HttpPatch]
        [ActionName("Lead")]
        public IActionResult UpdateLeadAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("PATCH: integration/1c/lead");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // POST: integration/1c/course
        [HttpPost]
        [ActionName("Course")]
        public IActionResult CreateCourseAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/course");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // PATCH: integration/1c/course
        [HttpPatch]
        [ActionName("Course")]
        public IActionResult UpdateCourseAmo()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("PATCH: integration/1c/course");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // POST: integration/1c/paymentreceived
        [HttpPost]
        [ActionName("PaymentReceived")]
        public IActionResult ProcessPayment()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
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
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/courseended");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }
    }
}