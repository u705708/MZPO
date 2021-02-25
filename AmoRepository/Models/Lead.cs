using System.Collections.Generic;
using Newtonsoft.Json;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Lead is an entity in amoCRM.
    /// </summary>
    public class Lead : IEntity
    {
        [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get => "leads"; }

        /// <summary>
        /// ID сделки.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Название сделки.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Бюджет сделки.
        /// </summary>
        public int? price { get; set; }
        /// <summary>
        /// ID пользователя, ответственного за сделку.
        /// </summary>
        public int? responsible_user_id { get; set; }
        /// <summary>
        /// ID группы, в которой состоит ответственный пользователь за сделку.
        /// </summary>
        public int? group_id { get; set; }
        /// <summary>
        /// ID статуса, в который добавляется сделка, по-умолчанию – первый этап главной воронки.
        /// </summary>
        public int? status_id { get; set; }
        /// <summary>
        /// ID воронки, в которую добавляется сделка.
        /// </summary>
        public int? pipeline_id { get; set; }
        /// <summary>
        /// ID причины отказа.
        /// </summary>
        public int? loss_reason_id { get; set; }
        /// <summary>
        /// ID источника сделки.
        /// </summary>
        public int? source_id { get; set; }
        /// <summary>
        /// ID пользователя, создающий сделку.
        /// </summary>
        public int? created_by { get; set; }
        /// <summary>
        /// ID пользователя, изменяющий сделку.
        /// </summary>
        public int? updated_by { get; set; }
        /// <summary>
        /// Дата закрытия сделки, передается в Unix Timestamp.
        /// </summary>
        public int? closed_at { get; set; }
        /// <summary>
        /// Дата создания сделки, передается в Unix Timestamp.
        /// </summary>
        public int? created_at { get; set; }
        /// <summary>
        /// Дата изменения сделки, передается в Unix Timestamp.
        /// </summary>
        public int? updated_at { get; set; }
        /// <summary>
        /// Дата ближайшей задачи к выполнению, передается в Unix Timestamp.
        /// </summary>
        public int? closest_task_at { get; set; }
        /// <summary>
        /// Удалена ли сделка.
        /// </summary>
        public bool? is_deleted { get; set; }
        /// <summary>
        /// Список, содержащий информацию по значениям дополнительных полей, заданных для данной сделки.
        /// </summary>
        public List<Custom_fields_value> custom_fields_values { get; set; }
        /// <summary>
        /// Скоринг сделки.
        /// </summary>
        public int? score { get; set; }
        /// <summary>
        /// ID аккаунта, в котором находится сделка.
        /// </summary>
        public int? account_id { get; set; }
        /// <summary>
        /// Изменен ли в последний раз бюджет сделки роботом.
        /// </summary>
        public bool? is_price_modified_by_robot { get; set; }
        /// <summary>
        /// Ссылки сделки.
        /// </summary>
        public Links _links { get; set; }
        /// <summary>
        /// Данные вложенных сущностей.
        /// </summary>
        public Embedded _embedded { get; set; }

        public class Custom_fields_value
        {
            /// <summary>
            /// ID поля.
            /// </summary>
            public int field_id { get; set; }
            /// <summary>
            /// Название поля.
            /// </summary>
            public string field_name { get; set; }
            /// <summary>
            /// Код поля.
            /// </summary>
            public string field_code { get; set; }
            /// <summary>
            /// Тип поля.
            /// </summary>
            public string field_type { get; set; }
            /// <summary>
            /// Массив значений поля.
            /// </summary>
            public Values[] values { get; set; }

            public class Values
            {
                /// <summary>
                /// Значение поля.
                /// </summary>
                [JsonProperty(NullValueHandling = NullValueHandling.Include)]
                public object value { get; set; }
                public int? enum_id { get; set; }
                public string enum_code { get; set; }
            }
        }
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
            /// Причина отказа сделки.
            /// </summary>
            public LossReason loss_reason { get; set; }
            /// <summary>
            /// Данные тегов, привязанных к сделке.
            /// </summary>
            public List<Tag> tags { get; set; }
            /// <summary>
            /// Данные контактов, привязанных к сделке.
            /// </summary>
            public List<Contact> contacts { get; set; }
            /// <summary>
            /// Данные компании, привязанной к сделке, в данном массиве всегда 1 элемент, так как у сделки может быть только 1 компания.
            /// </summary>
            public List<Company> companies { get; set; }
            /// <summary>
            /// Данные элементов списков, привязанных к сделке.
            /// </summary>
            public List<CatalogElements> catalog_elements { get; set; }

            public class LossReason
            {
                /// <summary>
                /// ID причины отказа.
                /// </summary>
                public int id { get; set; }
                /// <summary>
                /// Название причины отказа.
                /// </summary>
                public string name { get; set; }
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