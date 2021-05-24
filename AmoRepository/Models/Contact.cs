using Newtonsoft.Json;
using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Contact is an entity in amoCRM.
    /// </summary>
    public class Contact : IEntity
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonIgnore]
        public static string entityLink { get => "contacts"; }

        /// <summary>
        /// ID контакта.
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// Название контакта.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Имя контакта.
        /// </summary>
        public string first_name { get; set; }
        /// <summary>
        /// Фамилия контакта.
        /// </summary>
        public string last_name { get; set; }
        /// <summary>
        /// ID пользователя, ответственного за контакт.
        /// </summary>
        public int? responsible_user_id { get; set; }
        /// <summary>
        /// ID группы, в которой состоит ответственны пользователь за контакт.
        /// </summary>
        public int? group_id { get; set; }
        /// <summary>
        /// ID пользователя, создавший контакт.
        /// </summary>
        public int? created_by { get; set; }
        /// <summary>
        /// ID пользователя, изменивший контакт.
        /// </summary>
        public int? updated_by { get; set; }
        /// <summary>
        /// Дата создания контакта, передается в Unix Timestamp.
        /// </summary>
        public int? created_at { get; set; }
        /// <summary>
        /// Дата изменения контакта, передается в Unix Timestamp.
        /// </summary>
        public int? updated_at { get; set; }
        /// <summary>
        /// Дата ближайшей задачи к выполнению, передается в Unix Timestamp.
        /// </summary>
        public int? closest_task_at { get; set; }
        /// <summary>
        /// Список, содержащий информацию по значениям дополнительных полей, заданных для данного контакта.
        /// </summary>
        public List<Custom_fields_value> custom_fields_values { get; set; }
        /// <summary>
        /// ID аккаунта, в котором находится контакт.
        /// </summary>
        public int? account_id { get; set; }
        /// <summary>
        /// Ссылки контакта.
        /// </summary>
        public Links _links { get; set; }
        /// <summary>
        /// Данные вложенных сущностей.
        /// </summary>
        public Embedded _embedded { get; set; }

        //public class Custom_fields_value
        //{
        //    /// <summary>
        //    /// ID поля.
        //    /// </summary>
        //    public int field_id { get; set; }
        //    /// <summary>
        //    /// Название поля.
        //    /// </summary>
        //    public string field_name { get; set; }
        //    /// <summary>
        //    /// Код поля.
        //    /// </summary>
        //    public string field_code { get; set; }
        //    /// <summary>
        //    /// Тип поля.
        //    /// </summary>
        //    public string field_type { get; set; }
        //    /// <summary>
        //    /// Массив значений поля
        //    /// </summary>
        //    public Values[] values { get; set; }

        //    public class Values
        //    {
        //        /// <summary>
        //        /// Значения поля.
        //        /// </summary>
        //        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        //        public object value { get; set; }
        //        public int? enum_id { get; set; }
        //        public string enum_code { get; set; }
        //    }
        //}
        
        public class Links
        {
            public Link self { get; set; }

            public class Link
            {
                public string href { get; set; }
            }
        }

        public class Embedded
        {
            /// <summary>
            /// Данные тегов, привязанных к сделке.
            /// </summary>
            public List<Tag> tags { get; set; }
            /// <summary>
            /// Данные компании, привязанной к сделке, в данном массиве всегда 1 элемент, так как у сделки может быть только 1 компания.
            /// </summary>
            public List<Company> companies { get; set; }
            /// <summary>
            /// Данные контактов, привязанных к сделке.
            /// </summary>
            public List<Customers> customers { get; set; }
            /// <summary>
            /// Данные сделок, привязанных к контакту.
            /// </summary>
            public List<Lead> leads { get; set; }
            /// <summary>
            /// Данные элементов списков, привязанных к сделке.
            /// </summary>
            public List<CatalogElements> catalog_elements { get; set; }

            public class Customers
            {
                /// <summary>
                /// ID покупателя.
                /// </summary>
                public int id { get; set; }
            }

            public class CatalogElements
            {
                /// <summary>
                /// ID элемента, привязанного к сделке.
                /// </summary>
                public int id { get; set; }
                /// <summary>
                /// Мета-данные элемента.
                /// </summary>
                public Meta metadata { get; set; }
                public class Meta
                {
                    /// <summary>
                    /// Количество элементов у сделки.
                    /// </summary>
                    public int quantity { get; set; }
                    /// <summary>
                    /// ID списка, в котором находится элемент.
                    /// </summary>
                    public int catalog_id { get; set; }
                }
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
