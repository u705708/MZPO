using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;

namespace Integration1C
{
    public class UpdateAmoCourse
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Course1C _course1C;

        public UpdateAmoCourse(Course1C course, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _course1C = course;
        }

        private static void AddUIDToEntities(Course1C course, int acc_id, CatalogElement ce)
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
                    ce.custom_fields.Add(new CatalogElement.Custom_fields()
                    {
                        id = FieldLists.Courses[acc_id][p.Name],
                        values = new CatalogElement.Custom_fields.Values[] { new CatalogElement.Custom_fields.Values() { value = p.GetValue(course).ToString() } }
                    });
                }
        }

        private static void UpdateCourseInAmo(Course1C course, IAmoRepo<Lead> leadRepo, int ce_id, int acc_id)
        {
            CatalogElement ce = new()
            {
                id = ce_id,
                name = course.name,
                custom_fields = new()
            };

            AddUIDToEntities(course, acc_id, ce);

            PopulateCFs(course, acc_id, ce);

            leadRepo.UpdateCEs(ce);
        }

        public List<Amo_id> Run()
        {
            try
            {
                if (_course1C.amo_ids is not null)
                {
                    foreach (var a in _course1C.amo_ids)
                        UpdateCourseInAmo(_course1C, _amo.GetAccountById(a.account_id).GetRepo<Lead>(), a.entity_id, a.account_id);
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update course in amo: {e}");
            }

            return _course1C.amo_ids;
        }
    }
}