using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class PopulateCourses
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly CourseRepository _repo1C;

        public PopulateCourses(Amo amo, Log log, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _repo1C = new(cred1C);
        }

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

        public Course1C Run(Guid course_id)
        {
            var course1C = _repo1C.GetCourse(course_id);

            if (course1C.amo_ids is not null &&
                course1C.amo_ids.Count == 3) return course1C;

            try 
            {
                //amo_ids.AddRange(new CreateOrUpdateAmoCourse(course1C, _amo, _log).Run());
                course1C.amo_ids.Add(CreateCourseInAmo(course1C, _amo.GetAccountById(29490250).GetRepo<Lead>(), 29490250));

                if (course1C.amo_ids.Count < 3) throw new Exception("Created less than 3 courses in amoCRM.");
            }
            catch (Exception e) { throw new Exception($"Unable to create courses in amoCRM: {e}"); }
            
            try {_repo1C.UpdateCourse(course1C); }
            catch (Exception e) { throw new Exception($"Unable to save amo_ids in 1C: {e}"); }

            return course1C;
        }
    }
}