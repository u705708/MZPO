using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers.Example
{
    [Route("example/{action}")]
    public class ExampleController : Controller
    {
        // GET example/client
        [HttpGet]
        public IActionResult Client()
        {
            var payload = new Integration1C.Client1C() { 
                client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40"),
                amo_ids = new() {
                    new() {
                        account_id = 19453687,
                        entity_id = 46776565
                    },
                    new() {
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
                //pass_dpt_code = "123132"
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }

        // GET example/company
        [HttpGet]
        public IActionResult Company()
        {
            var payload = new Integration1C.Company1C() { 
                company_id_1C = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c"),
                amo_ids = new() { new() { 
                    account_id = 19453687,
                    entity_id = 46776835
                } },
                name = "Тестовая компания",
                email = "test@email.com",
                phone = "+79001112233",
                signee = "Подписант",
                LPR_name = "Иван Иванов",
                OGRN = "13223131326",
                INN = "5465454654",
                acc_no = "4654654654654654",
                //KPP = "546546465",
                BIK = "45654654",
                address = "ул. Пушкина, 10",
                post_address = "ул. Колотушкина, 11"
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }

        // GET example/course
        [HttpGet]
        public IActionResult Course()
        {
            var payload = new Integration1C.Course1C() {
                product_id_1C = new Guid("1205f8a9-0a5a-47d1-99e2-30a2d2823948"),
                amo_ids = new() {
                    new() {
                        account_id = 19453687,
                        entity_id = 1795667
                    },
                    new() {
                        account_id = 28395871,
                        entity_id = 1463133
                    }
                },
                name = "Тестовый курс",
                short_name = "Тест",
                price = 10000,
                duration = 144,
                format = "Очный",
                //program_id = "",
                //group = "",
                requirements = "Нет",
                supplementary_info = "Проверка"
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }

        // GET example/lead
        [HttpGet]
        public IActionResult Lead()
        {
            var payload = new Integration1C.Lead1C() {
                lead_id_1C = new Guid("628c57d5-3338-4366-9691-942774e8323f"),
                amo_ids = new() { new() { 
                    account_id = 19453687,
                    entity_id = 1795667
                } },
                client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40"),
                product_id_1C = new Guid("1205f8a9-0a5a-47d1-99e2-30a2d2823948"),
                company_id_1C = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c"),
                organization = "МЦПО",
                price = 10000,
                is_corporate = true,
                lead_status = "",
                //marketing_channel = "",
                //marketing_source = "",
                author = "",
                responsible_user = "",
                payments = new() { new() { 
                    payment_date = DateTime.Now,
                    payment_amount = 5000,
                    client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40")
                } }
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }

        // GET example/diploma
        [HttpGet]
        public IActionResult Diploma()
        {
            var payload = new Integration1C.Diploma()
            {
                discipline = "Массаж медицинский",
                qualification = "",
                hours = 288,
                educationForm = "Очная",
                educationType = "Вид обучения",
                diplomaNumber = "1231№54654",
                dateOfIssue = DateTime.Now,
                client_Id_1C = default
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }
    }
}