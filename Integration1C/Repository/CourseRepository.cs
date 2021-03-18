using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Integration1C
{
    internal class CourseRepository
    {
        private readonly Cred1C _cred1C;

        public CourseRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        internal Course1C GetCourse(Course1C course) => GetCourse((Guid)course.product_id_1C);

        internal Course1C GetCourse(Guid course_id)
        {
            string method = $"http://94.230.11.182:50080/uuc/hs/courses/EditApplication?id={course_id:D}";
            Request1C request = new("GET", method, _cred1C);
            Course1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal IEnumerable<Course1C> GetAllCourses()
        {
            string method = "";
            Request1C request = new("GET", method, _cred1C);
            List<Course1C> result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}