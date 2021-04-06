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
            ItemPrices = new() { new() { 
                Price = 10000,
                UID = new Guid("5bba5dc3-580c-11eb-86f0-82172a65d31e"),
                PriceType = "Основная цена"
            } },
            duration = 144,
            format = "Очный",
            //program_id = "",
            //group = "",
            //requirements = "Нет",
            supplementary_info = "Проверка"
        };

        public class Result
        {
            public Guid product_id_1C { get; set; }
        }

        internal Course1C GetCourse(Course1C course) => GetCourse((Guid)course.product_id_1C);

        internal Course1C GetCourse(Guid course_id)
        {
            string method = $"EditCourse?uid={course_id:D}";
            Request1C request = new("GET", method, _cred1C);
            Course1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateCourse(Course1C course)
        {
            if (course.product_id_1C is null ||
                course.product_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "EditCourse";
            string content = JsonConvert.SerializeObject(course, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result); }
            catch (Exception e) { return default; }
            return result.product_id_1C;
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