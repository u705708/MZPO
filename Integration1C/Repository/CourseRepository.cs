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

        private readonly Course1C _mockCourse = new Course1C()
        {
            product_id_1C = new Guid("1205f8a9-0a5a-47d1-99e2-30a2d2823948"),
            amo_ids = new()
            {
                new()
                {
                    account_id = 19453687,
                    entity_id = 1795667
                },
                new()
                {
                    account_id = 28395871,
                    entity_id = 1463133
                }
            },
            name = "Тестовый курс",
            short_name = "Тест",
            price = 10000,
            duration = 144,
            format = "Очный",
            program_id = "",
            group = "",
            requirements = "Нет",
            supplementary_info = "Проверка"
        };

        internal Course1C GetCourse(Course1C course) => GetCourse((Guid)course.product_id_1C);

        internal Course1C GetCourse(Guid course_id)
        {
            string method = $"EditApplication?uid={course_id:D}";
            Request1C request = new("GET", method, _cred1C);
            Course1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal IEnumerable<Course1C> GetAllCourses()
        {
            throw new NotImplementedException();

            string method = "";
            Request1C request = new("GET", method, _cred1C);
            List<Course1C> result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}