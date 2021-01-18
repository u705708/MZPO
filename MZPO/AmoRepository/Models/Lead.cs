using System.Collections.Generic;
using Newtonsoft.Json;

namespace MZPO.AmoRepo
{
    public class Lead : IModel
    {
        [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get => "leads"; }                     //Возвращается название ссылки на сущность, не сериализуется в JSON

        public int id { get; set; }                                             //ID сделки
        public string name { get; set; }                                        //Название сделки
        public int? price { get; set; }                                         //Бюджет сделки
        public int? responsible_user_id { get; set; }                           //ID пользователя, ответственного за сделку 
        public int? group_id { get; set; }                                      //ID группы, в которой состоит ответственный пользователь за сделку 
        public int? status_id { get; set; }                                     //ID статуса, в который добавляется сделка, по-умолчанию – первый этап главной воронки
        public int? pipeline_id { get; set; }                                   //ID воронки, в которую добавляется сделка
        public int? loss_reason_id { get; set; }                                //ID причины отказа
        public int? source_id { get; set; }                                     //Требуется GET параметр with. ID источника сделки
        public int? created_by { get; set; }                                    //ID пользователя, создающий сделку
        public int? updated_by { get; set; }                                    //ID пользователя, изменяющий сделку
        public int? closed_at { get; set; }                                     //Дата закрытия сделки, передается в Unix Timestamp 
        public int? created_at { get; set; }                                    //Дата создания сделки, передается в Unix Timestamp 
        public int? updated_at { get; set; }                                    //Дата изменения сделки, передается в Unix Timestamp 
        public int? closest_task_at { get; set; }                               //Дата ближайшей задачи к выполнению, передается в Unix Timestamp 
        public bool? is_deleted { get; set; }                                   //Удалена ли сделка
        public IList<Custom_fields_value> custom_fields_values { get; set; }    //Массив, содержащий информацию по значениям дополнительных полей, заданных для данной сделки
        public int? score { get; set; }                                         //Скоринг сделки
        public int? account_id { get; set; }                                    //ID аккаунта, в котором находится сделка
        public bool? is_price_modified_by_robot { get; set; }                   //Требуется GET параметр with. Изменен ли в последний раз бюджет сделки роботом 
        public Links _links { get; set; }
        public Embedded _embedded { get; set; }                                 //Данные вложенных сущностей

        public class Custom_fields_value
        {
            public int field_id { get; set; }
            public string field_name { get; set; }
            public string field_code { get; set; }
            public string field_type { get; set; }
            public Values[] values { get; set; }

            public class Values
            {
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
            public LossReason loss_reason { get; set; }                         //Требуется GET параметр with. Причина отказа сделки
            public IList<Tag> tags { get; set; }                                //Данные тегов, привязанных к сделке
            public IList<Contact> contacts { get; set; }                        //Требуется GET параметр with. Данные контактов, привязанных к сделке
            public IList<Company> companies { get; set; }                     //Данные компании, привязанной к сделке, в данном массиве всегда 1 элемент, так как у сделки может быть только 1 компания
            public IList<CatalogElements> catalog_elements { get; set; }        //Требуется GET параметр with. Данные элементов списков, привязанных к сделке

            public class LossReason
            {
                public int id { get; set; }                                     //ID причины отказа
                public string name { get; set; }                                //Название причины отказа
            }

            public class Companies
            {
                public int id { get; set; }                                     //ID контакта, привязанного к сделке
            }

            public class CatalogElements
            {
                public int id { get; set; }                                     //ID элемента, привязанного к сделке
                public object metadata { get; set; }                            //Мета-данные элемента
                public int quantity { get; set; }                               //Количество элементов у сделки
                public int catalog_id { get; set; }                             //ID списка, в котором находится элемент 
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}