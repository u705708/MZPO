using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class ClientRepository
    {
        private readonly Cred1C _cred1C;

        public ClientRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        private readonly Guid _mockGuid = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40");
        private readonly Client1C _mockClient = new Client1C()
        {
            client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40"),
            amo_ids = new()
            {
                new()
                {
                    account_id = 19453687,
                    entity_id = 46776565
                },
                new()
                {
                    account_id = 28395871,
                    entity_id = 33336001
                }
            },
            email = "no@email.test.test",
            phone = "+79001112233",
            name = "Тестовый контакт",
            dob = DateTime.Now,
            pass_serie = "1234",
            pass_number = "556677",
            pass_issued_by = "some text",
            pass_issued_at = "Date as string",
            pass_dpt_code = "123132"
        };

        public class Result
        {
            public Guid client_id_1C { get; set; }
        }

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
            //string method = $"getStudentInfo?id={client_id:D}";
            //Request1C request = new("GET", method, _cred1C);

            //StudentDTO student = new();
            //JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), student);

            //return new() { 
            //    name = student.name,
            //    client_id_1C = student.uid,
            //    phone = student.Telephone,
            //    email = student.Mail,
            //    pass_number = student.Pasport.Number,
            //    pass_serie = student.Pasport.Series,
            //    pass_issued_by = student.Pasport.Issued,
            //    pass_dpt_code = student.Pasport.DivisionCode,
            //    pass_issued_at = student.Pasport.DateOfIssued.ToShortDateString()
            //};

            string method = $"EditStudent?uid={client_id:D}";
            Request1C request = new("GET", method, _cred1C);

            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateClient(Client1C client)
        {
            if (client.client_id_1C is null ||
                client.client_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result); }
            catch (Exception e) { return default; }
            return result.client_id_1C;
        }

        internal Guid AddClient(Client1C client)
        {
            if (string.IsNullOrEmpty(client.email) &&
                string.IsNullOrEmpty(client.phone))
                throw new Exception("Unable to add client to 1C: no phone or email.");

            client.client_id_1C = null;

            string method = "EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result); }
            catch (Exception e) { return default; }
            return result.client_id_1C;
        }
    }
}