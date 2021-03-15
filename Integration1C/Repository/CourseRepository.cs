using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Integration1C
{
    internal class CourseRepository
    {
        internal Course1C GetCourse(Course1C course) => GetCourse(course.product_id_1C);

        internal Course1C GetCourse(Guid course_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/EditApplication?id={course_id.ToString("D")}";
            Request1C request = new("GET", uri);
            Course1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal IEnumerable<Course1C> GetAllCourses()
        {
            string uri = "";
            Request1C request = new("GET", uri);
            List<Course1C> result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}