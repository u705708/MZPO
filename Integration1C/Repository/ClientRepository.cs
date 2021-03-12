using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class ClientRepository
    {
        public class StudentDTO
        {
            public string name;
            public Guid uid;
            public string Telephone;
            public string Mail;
            public Pass Pasport;

            public class Pass
            {
                public string Series;
                public string Number;
                public string Issued;
                public string DivisionCode;
                public DateTime DateOfIssued;
            }
        }
        
        internal Client1C GetClient(Client1C client) => GetClient(client.Client_id_1C);

        internal Client1C GetClient(Guid client_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/getStudentInfo?id={client_id.ToString("D")}";
            Request1C request = new("GET", uri);

            StudentDTO student = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), student);

            return new() { 
                Name = student.name,
                Client_id_1C = student.uid,
                Phone = student.Telephone,
                Email = student.Mail,
                Pass_number = student.Pasport.Number,
                Pass_serie = student.Pasport.Series,
                Pass_issued_by = student.Pasport.Issued,
                Pass_dpt_code = student.Pasport.DivisionCode,
                Pass_issued_at = student.Pasport.DateOfIssued.ToShortDateString()
            };
        }

        internal Client1C UpdateClient(Client1C client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Client1C AddClient(Client1C client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}
