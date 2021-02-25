using MZPO.Services;
using MZPO.AmoRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    class CreateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client;
        private readonly string _request;
        private readonly IAmoRepo<Contact> _retRepo;
        private readonly IAmoRepo<Contact> _corpRepo;

        public CreateAmoContact(string request, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _client = new();
            _request = request;
            _retRepo = _amo.GetAccountById(28395871).GetRepo<Contact>();
            _corpRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
        }

        public void Run()
        {
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(_request), _client); }
            catch(Exception e) { _log.Add($"Unable to process request: {e} --- Request: {_request}"); return; }



        }
    }
}
