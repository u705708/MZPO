using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdateAmoCourse
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Course1C _course1C;

        public CreateOrUpdateAmoCourse(Course1C course1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _course1C = course1C;
        }

        private readonly List<int> amo_accounts = new()
        {
            19453687,
            28395871
        };

        private static void AddUIDToEntity(Course1C course, int acc_id, CatalogElement ce)
        {
            ce.custom_fields.Add(new CatalogElement.Custom_fields()
            {
                id = FieldLists.Courses[acc_id]["product_id_1C"],
                values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = course.product_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Course1C course, int acc_id, CatalogElement ce)
        {
            foreach (var p in course.GetType().GetProperties())
                if (FieldLists.Courses[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(course) is not null)
                {
                    if (p.Name == "price")
                        continue;

                    try { if ((string)p.GetValue(course) == "") continue; }
                    catch { }

                    ce.custom_fields.Add(new CatalogElement.Custom_fields()
                    {
                        id = FieldLists.Courses[acc_id][p.Name],
                        values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = p.GetValue(course).ToString() } }
                    });
                }

            if (course.ItemPrices is not null &&
                course.ItemPrices.Any())
                ce.custom_fields.Add(new CatalogElement.Custom_fields()
                {
                    id = FieldLists.Courses[acc_id]["price"],
                    values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = course.ItemPrices.First().Price.ToString() } }
                });

        }

        private static void UpdateCourseInAmo(Course1C course, IAmoRepo<Lead> leadRepo, int ce_id, int acc_id)
        {
            CatalogElement ce = new()
            {
                id = ce_id,
                name = course.name,
                custom_fields = new()
            };

            AddUIDToEntity(course, acc_id, ce);

            PopulateCFs(course, acc_id, ce);

            leadRepo.UpdateCEs(ce);
        }

        private static Amo_id CreateCourseInAmo(Course1C course, IAmoRepo<Lead> leadRepo, int acc_id)
        {
            CatalogElement ce = new()
            {
                name = course.name,
                custom_fields = new()
            };

            AddUIDToEntity(course, acc_id, ce);

            PopulateCFs(course, acc_id, ce);

            var result = leadRepo.AddCEs(ce);

            if (!result.Any()) throw new Exception($"Unable to update course in amo {course.name}");

            return new() { account_id = acc_id, entity_id = result.First().id };
        }

        public List<Amo_id> Run()
        {
            if (_course1C.amo_ids is null) _course1C.amo_ids = new();

            try
            {
                foreach (var a in amo_accounts)
                {
                    if (_course1C.amo_ids.Any(x => x.account_id == a))
                        try 
                        { 
                            UpdateCourseInAmo(_course1C, _amo.GetAccountById(a).GetRepo<Lead>(), _course1C.amo_ids.First(x => x.account_id == a).entity_id, a);

                            _log.Add($"Updated course {_course1C.short_name} in amo {a}.");

                            continue;
                        }
                        catch (Exception e) 
                        { 
                            _log.Add($"Unable to update course {_course1C.amo_ids.First(x => x.account_id == a).entity_id} in amo. Creating new. {e}"); 
                        }
                    _course1C.amo_ids.Add(CreateCourseInAmo(_course1C, _amo.GetAccountById(a).GetRepo<Lead>(), a));

                    _log.Add($"Created course {_course1C.short_name} in amo {a}.");
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update course in amo from 1C: {e}");
            }

            return _course1C.amo_ids;
        }
    }
}