using Newtonsoft.Json;

namespace MZPO.AmoRepo
{
    public class Note
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }                         //ID примечания
        public int? entity_id { get; set; }                 //ID родительской сущности примечания
        public int? created_by { get; set; }                //ID пользователя, создавший примечание
        public int? updated_by { get; set; }                //ID пользователя, изменивший примечание последним
        public int? created_at { get; set; }                //Дата создания примечания, передается в Unix Timestamp
        public int? updated_at { get; set; }                //Дата изменения примечания, передается в Unix Timestamp
        public int? responsible_user_id { get; set; }       //ID пользователя, ответственного за примечание
        public int? group_id { get; set; }                  //ID группы, в которой состоит ответственны пользователь за примечание
        public string note_type { get; set; }               //Тип примечания
        [JsonProperty("params")]
        public Params parameters { get; set; }                 //Свойства примечания, зависят от типа примечания. Подробней о свойствах читайте тут
        public int? account_id { get; set; }                //ID аккаунта, в котором находится примечание 
        public Links _links { get; set; }

        public class Links
        {
            public Link self { get; set; }

            public class Link
            {
                public string href { get; set; }
            }
        }

        public class Params
        {
            public string text { get; set; }
            public string uniq { get; set; }
            public int? duration { get; set; }
            public string source { get; set; }
            public string link { get; set; }
            public string phone { get; set; }
            public string service { get; set; }
            public string status { get; set; }
            public string icon_url { get; set; }
            public string address { get; set; }
            public string longitude { get; set; }
            public string latitude { get; set; }
            public string thread_id { get; set; }
            public string message_id { get; set; }
            [JsonProperty("private")]
            public bool? prvt { get; set; }
            public bool? income { get; set; }
            public Address from { get; set; }
            public Address to { get; set; }
            public int? version { get; set; }
            public string subject { get; set; }
            public int? access_granted { get; set; }
            public string content_summary { get; set; }
            public int? attach_cnt { get; set; }
            public Delivery delivery { get; set; }

            public class Address
            {
                public string email { get; set; }
                public string name { get; set; }
            }

            public class Delivery
            {
                public string status { get; set; }
                public int? time { get; set; }
                public string reason { get; set; }
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
