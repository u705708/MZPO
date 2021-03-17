using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        
        internal Client1C GetClient(Client1C client) => GetClient((Guid)client.client_id_1C);

        internal Client1C GetClient(Guid client_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/getStudentInfo?id={client_id:D}";
            Request1C request = new("GET", uri);

            StudentDTO student = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), student);

            return new() { 
                name = student.name,
                client_id_1C = student.uid,
                phone = student.Telephone,
                email = student.Mail,
                pass_number = student.Pasport.Number,
                pass_serie = student.Pasport.Series,
                pass_issued_by = student.Pasport.Issued,
                pass_dpt_code = student.Pasport.DivisionCode,
                pass_issued_at = student.Pasport.DateOfIssued.ToShortDateString()
            };
        }

        internal Guid UpdateClient(Client1C client)
        {
            if (client.client_id_1C is null ||
                client.client_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");
            
            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid AddClient(Client1C client)
        {
            if (string.IsNullOrEmpty(client.email) &&
                string.IsNullOrEmpty(client.phone))
                throw new Exception("Unable to add client to 1C: no phone or email.");

            client.client_id_1C = null;

            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}