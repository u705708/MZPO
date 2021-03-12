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
                Client_id_1C = default,
                Amo_ids = new() { new() { 
                    Account_id = 123465,
                    Entity_id = 44556688
                } },
                Email = "test@email.com",
                Phone = "+79001112233",
                Name = "Иван Иванов",
                Dob = DateTime.Now,
                Pass_serie = "1234",
                Pass_number = "556677",
                Pass_issued_by = "some text",
                Pass_issued_at = "Date as string",
                Pass_dpt_code = "123132"
            });
        }

        // GET example/company
        [HttpGet]
        public IActionResult Company()
        {
            return Ok(new Integration1C.Company1C() { 
                Company_id_1C = default,
                Amo_ids = new() { new() { 
                    Account_id = 123465,
                    Entity_id = 44556688
                } },
                Name = "ООО Ромашка",
                Email = "test@email.com",
                Phone = "+79001112233",
                Signee = "Подписант",
                LPR_name = "Иван Иванов",
                OGRN = "13223131326",
                INN = "5465454654",
                Acc_no = "4654654654654654",
                KPP = "546546465",
                BIK = "45654654",
                Address = "ул. Пушкина, 10",
                Post_address = "ул. Колотушкина, 11"
            });
        }

        // GET example/course
        [HttpGet]
        public IActionResult Course()
        {
            return Ok(new Integration1C.Course1C() {
                Product_id_1C = default,
                Amo_ids = new() { new() {
                    Account_id = 123465,
                    Entity_id = 44556688
                } },
                Name = "Массаж медицинский",
                Short_name = "МММ",
                Price = 10000,
                Duration = 44,
                Format = "",
                Program_id = "",
                Group = "",
                Requirements = "",
                Supplementary_info = ""
            });
        }

        // GET example/lead
        [HttpGet]
        public IActionResult Lead()
        {
            return Ok(new Integration1C.Lead1C() {
                Lead_id_1C = default,
                Amo_ids = new() { new() { 
                    Account_id = 123465,
                    Entity_id = 44556688
                } },
                Client_id_1C = default,
                Product_id_1C = default,
                Company_id_1C = default,
                Organization = "МЦПО или МИРК",
                Price = 10000,
                Is_corporate = true,
                Lead_status = "",
                Marketing_channel = "",
                Marketing_source = "",
                Author = "",
                Responsible_user = "",
                Payments = new() { new() { 
                    Payment_date = DateTime.Now,
                    Payment_amount = 10000,
                    Client_id_1C = default
                } }
            });
        }

        // GET example/diploms
        [HttpGet]
        public IActionResult Diploma()
        {
            return Ok(new Integration1C.Diploma()
            {
                Discipline = "Массаж медицинский",
                Qualification = "",
                Hours = 288,
                EducationForm = "Очная",
                EducationType = "Вид обучения",
                DiplomaNumber = "1231№54654",
                DateOfIssue = DateTime.Now,
                Client_Id_1C = default
            });
        }
    }
}