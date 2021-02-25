using Newtonsoft.Json;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Note in amoCRM is a property of some entity Lead, Contact, Company etc.
    /// </summary>
    public class Note
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// ID примечания.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// ID родительской сущности примечания.
        /// </summary>
        public int? entity_id { get; set; }
        /// <summary>
        /// ID пользователя, создавший примечание.
        /// </summary>
        public int? created_by { get; set; }
        /// <summary>
        /// ID пользователя, изменивший примечание последним.
        /// </summary>
        public int? updated_by { get; set; }
        /// <summary>
        /// Дата создания примечания, передается в Unix Timestamp.
        /// </summary>
        public int? created_at { get; set; }
        /// <summary>
        /// Дата изменения примечания, передается в Unix Timestamp.
        /// </summary>
        public int? updated_at { get; set; }
        /// <summary>
        /// ID пользователя, ответственного за примечание.
        /// </summary>
        public int? responsible_user_id { get; set; }
        /// <summary>
        /// ID группы, в которой состоит ответственны пользователь за примечание.
        /// </summary>
        public int? group_id { get; set; }
        /// <summary>
        /// Тип примечания.
        /// </summary>
        public string note_type { get; set; }
        /// <summary>
        /// Свойства примечания, зависят от типа примечания.
        /// </summary>
        [JsonProperty("params")]
        public Params parameters { get; set; }
        /// <summary>
        /// ID аккаунта, в котором находится примечание.
        /// </summary>
        public int? account_id { get; set; }
        /// <summary>
        /// Ссылки примечания.
        /// </summary>
        public Links _links { get; set; }

        public class Links
        {
            /// <summary>
            /// Ссылка на примечание.
            /// </summary>
            public Link self { get; set; }

            public class Link
            {
                /// <summary>
                /// Адрес ссылки.
                /// </summary>
                public string href { get; set; }
            }
        }

        public class Params
        {
            /// <summary>
            /// Текст примечания.
            /// </summary>
            public string text { get; set; }
            /// <summary>
            /// Идентификатор звонка.
            /// </summary>
            public string uniq { get; set; }
            /// <summary>
            /// Длительность звонка.
            /// </summary>
            public int? duration { get; set; }
            /// <summary>
            /// Источник звонка (телефония).
            /// </summary>
            public string source { get; set; }
            /// <summary>
            /// Ссылка на звонок.
            /// </summary>
            public string link { get; set; }
            /// <summary>
            /// Телефон абонента.
            /// </summary>
            public string phone { get; set; }
            /// <summary>
            /// Сервис, отправиший сообщение.
            /// </summary>
            public string service { get; set; }
            /// <summary>
            /// Статус заказа. Варианты: created, shown, canceled
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// Адрес ссылки на иконку.
            /// </summary>
            public string icon_url { get; set; }
            /// <summary>
            /// Почтовый адрес.
            /// </summary>
            public string address { get; set; }
            /// <summary>
            /// Долгота.
            /// </summary>
            public string longitude { get; set; }
            /// <summary>
            /// Широта.
            /// </summary>
            public string latitude { get; set; }
            /// <summary>
            /// Адрес ссылки.
            /// </summary>
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