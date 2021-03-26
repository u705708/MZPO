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
                client_id_1C = new Guid("29421f71-8b5c-11eb-891f-20040ffb909d"),
                amo_ids = new() {
                    new() {
                        account_id = 19453687,
                        entity_id = 46781093
                    },
                    new() {
                        account_id = 28395871,
                        entity_id = 33346793
                    }
                },
                email = "no@email.test.test",
                phone = "+79001112233",
                name = "Александров Александр Иванович",
                dob = DateTime.Now,
                pass_serie = "7202",
                pass_number = "331004",
                pass_issued_by = "АТП-3",
                pass_issued_at = "05.08.2010 0:00:00",
                //pass_dpt_code = "741-441"
            };

            return Content(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }), "application/json");
        }

        // GET example/company
        [HttpGet]
        public IActionResult Company()
        {
            var payload = new Integration1C.Company1C() { 
                company_id_1C = new Guid("5c269475-8ca2-11eb-891f-20040ffb909d"),
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
                INN = "7714829999",
                acc_no = "30301810000006000001",
                //KPP = "771401001",
                BIK = "044525225",
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
                product_id_1C = new Guid("d96704d3-5821-11eb-86f0-82172a65f31e"),
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
                price = new() { new() { 
                    Price = 10000,
                    UID = new Guid("5bba5dc3-580c-11eb-86f0-82172a65d31e"),
                    PriceType = "Основная цена"
                } },
                duration = 144,
                format = "Очный",
                //program_id = "",
                //group = "",
                //requirements = "Нет",
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
                client_id_1C = new Guid("29421f71-8b5c-11eb-891f-20040ffb909d"),
                product_id_1C = new Guid("d96704d3-5821-11eb-86f0-82172a65f31e"),
                company_id_1C = new Guid("5c269475-8ca2-11eb-891f-20040ffb909d"),
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