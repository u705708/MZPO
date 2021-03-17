using Microsoft.AspNetCore.Mvc;
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
            return Ok(new Integration1C.Client1C() { 
                client_id_1C = default,
                amo_ids = new() { new() { 
                    account_id = 123465,
                    entity_id = 44556688
                } },
                email = "test@email.com",
                phone = "+79001112233",
                name = "Иван Иванов",
                dob = DateTime.Now,
                pass_serie = "1234",
                pass_number = "556677",
                pass_issued_by = "some text",
                pass_issued_at = "Date as string",
                pass_dpt_code = "123132"
            });
        }

        // GET example/company
        [HttpGet]
        public IActionResult Company()
        {
            return Ok(new Integration1C.Company1C() { 
                company_id_1C = default,
                amo_ids = new() { new() { 
                    account_id = 123465,
                    entity_id = 44556688
                } },
                name = "ООО Ромашка",
                email = "test@email.com",
                phone = "+79001112233",
                signee = "Подписант",
                LPR_name = "Иван Иванов",
                OGRN = "13223131326",
                INN = "5465454654",
                acc_no = "4654654654654654",
                KPP = "546546465",
                BIK = "45654654",
                address = "ул. Пушкина, 10",
                post_address = "ул. Колотушкина, 11"
            });
        }

        // GET example/course
        [HttpGet]
        public IActionResult Course()
        {
            return Ok(new Integration1C.Course1C() {
                product_id_1C = default,
                amo_ids = new() { new() {
                    account_id = 123465,
                    entity_id = 44556688
                } },
                name = "Массаж медицинский",
                short_name = "МММ",
                price = 10000,
                duration = 44,
                format = "",
                program_id = "",
                group = "",
                requirements = "",
                supplementary_info = ""
            });
        }

        // GET example/lead
        [HttpGet]
        public IActionResult Lead()
        {
            return Ok(new Integration1C.Lead1C() {
                lead_id_1C = default,
                amo_ids = new() { new() { 
                    account_id = 123465,
                    entity_id = 44556688
                } },
                client_id_1C = default,
                product_id_1C = default,
                company_id_1C = default,
                organization = "МЦПО или МИРК",
                price = 10000,
                is_corporate = true,
                lead_status = "",
                marketing_channel = "",
                marketing_source = "",
                author = "",
                responsible_user = "",
                payments = new() { new() { 
                    payment_date = DateTime.Now,
                    payment_amount = 10000,
                    client_id_1C = default
                } }
            });
        }

        // GET example/diploma
        [HttpGet]
        public IActionResult Diploma()
        {
            return Ok(new Integration1C.Diploma()
            {
                discipline = "Массаж медицинский",
                qualification = "",
                hours = 288,
                educationForm = "Очная",
                educationType = "Вид обучения",
                diplomaNumber = "1231№54654",
                dateOfIssue = DateTime.Now,
                client_Id_1C = default
            });
        }
    }
}