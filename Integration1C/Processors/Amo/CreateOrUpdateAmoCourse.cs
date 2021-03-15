using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    class CreateOrUpdateAmoCourse
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Course1C _course1C;
        private readonly CourseRepository _1CRepo;

        public CreateOrUpdateAmoCourse(Course1C course1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _course1C = course1C;
            _1CRepo = new();
        }

        List<int> amo_accounts = new()
        {
            19453687,
            28395871
        };

        private static Amo_id UpdateCourseInAmo(Course1C course, IAmoRepo<Lead> leadRepo, int ce_id, int acc_id)
        {
            CatalogElement ce = new()
            {
                id = ce_id,
                name = course.name,
                custom_fields = new()
            };

            ce.custom_fields.Add(new CatalogElement.Custom_fields()
            {
                id = FieldLists.Courses[acc_id]["product_id_1C"],
                values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = course.product_id_1C.ToString("D") } }
            });

            foreach (var p in course.GetType().GetProperties())
                if (FieldLists.Courses[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(course) is not null &&
                    (string)p.GetValue(course) != "") //В зависимости от политики передачи пустых полей
                {
                    ce.custom_fields.Add(new CatalogElement.Custom_fields()
                    {
                        id = FieldLists.Courses[acc_id][p.Name],
                        values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = (string)p.GetValue(course) } }
                    });
                }

            var result = leadRepo.UpdateCEs(ce);
            
            if (!result.Any()) throw new Exception($"Unable to update course in amo {ce_id}");

            return new() { account_id = acc_id, entity_id = result.First().id };
        }

        private static Amo_id CreateCourseInAmo(Course1C course, IAmoRepo<Lead> leadRepo, int acc_id)
        {
            CatalogElement ce = new()
            {
                name = course.name,
                custom_fields = new()
            };

            ce.custom_fields.Add(new CatalogElement.Custom_fields()
            {
                id = FieldLists.Courses[acc_id]["product_id_1C"],
                values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = course.product_id_1C.ToString("D") } }
            });

            foreach (var p in course.GetType().GetProperties())
                if (FieldLists.Courses[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(course) is not null &&
                    (string)p.GetValue(course) != "") //В зависимости от политики передачи пустых полей
                {
                    ce.custom_fields.Add(new CatalogElement.Custom_fields()
                    {
                        id = FieldLists.Courses[acc_id][p.Name],
                        values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = (string)p.GetValue(course) } }
                    });
                }

            var result = leadRepo.AddCEs(ce);

            if (!result.Any()) throw new Exception($"Unable to update course in amo {course.name}");

            return new() { account_id = acc_id, entity_id = result.First().id };
        }

        public List<Amo_id> Run()
        {
            List<Amo_id> amo_Ids = new();

            try
            {
                foreach (var a in amo_accounts)
                {
                    if (_course1C.amo_ids is not null &&
                        _course1C.amo_ids.Any(x => x.account_id == a))
                        amo_Ids.Add(UpdateCourseInAmo(_course1C, _amo.GetAccountById(a).GetRepo<Lead>(), _course1C.amo_ids.First(x => x.account_id == a).entity_id, a));
                    else
                        amo_Ids.Add(CreateCourseInAmo(_course1C, _amo.GetAccountById(a).GetRepo<Lead>(), a));
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e}");
            }

            return amo_Ids;
        }
    }
}