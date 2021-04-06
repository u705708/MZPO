using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Course1C Run(Guid course_id)
        {
            var course1C = _repo1C.GetCourse(course_id);

            if (course1C.amo_ids is not null &&
                course1C.amo_ids.Count == 2) return course1C;

            List<Amo_id> amo_ids = new();

            try 
            { 
                amo_ids.AddRange(new CreateOrUpdateAmoCourse(course1C, _amo, _log).Run());
                if (amo_ids.Count < 2) throw new Exception("Created less than 2 courses in amoCRM.");
            }
            catch (Exception e) { throw new Exception($"Unable to create courses in amoCRM: {e}"); }
            
            course1C.amo_ids = amo_ids;
            try {_repo1C.UpdateCourse(course1C); }
            catch (Exception e) { throw new Exception($"Unable to save amo_ids in 1C: {e}"); }

            return course1C;
        }
    }
}