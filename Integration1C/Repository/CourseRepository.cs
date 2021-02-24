using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace Integration1C
{
    internal class CourseRepository
    {
        internal Course GetCourse(Course course) => GetCourse(course.product_id_1C);

        internal Course GetCourse(int course_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Course result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal IEnumerable<Course> GetAllCourses()
        {
            string uri = "";
            Request1C request = new("GET", uri);
            List<Course> result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}