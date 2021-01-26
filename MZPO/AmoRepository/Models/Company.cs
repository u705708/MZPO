using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    public class Company : IModel
    {
        [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get => "companies"; }                 //Возвращается название ссылки на сущность, не сериализуется в JSON

        public int id { get; set; }                                             //ID компании
        public string name { get; set; }                                        //Название компании
        public int? responsible_user_id { get; set; }                           //ID пользователя, ответственного за компанию
        public int? group_id { get; set; }                                      //ID группы, в которой состоит ответственны пользователь за компанию
        public int? created_by { get; set; }                                    //ID пользователя, создавший компанию
        public int? updated_by { get; set; }                                    //ID пользователя, изменивший компанию
        public int? created_at { get; set; }                                    //Дата создания компании, передается в Unix Timestamp
        public int? updated_at { get; set; }                                    //Дата изменения компании, передается в Unix Timestamp
        public int? closest_task_at { get; set; }                               //Дата ближайшей задачи к выполнению, передается в Unix Timestamp
        public IList<Custom_fields_value> custom_fields_values { get; set; }    //Массив, содержащий информацию по значениям дополнительных полей, заданных для данной компании
        public int account_id { get; set; }                                     //ID аккаунта, в котором находится компания
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
            public IList<Contact> contacts { get; set; }                        //Требуется GET параметр with. Данные контактов, привязанных к сделке
            public IList<Lead> leads { get; set; }                     //Данные компании, привязанной к сделке, в данном массиве всегда 1 элемент, так как у сделки может быть только 1 компания
            public IList<CatalogElements> catalog_elements { get; set; }        //Требуется GET параметр with. Данные элементов списков, привязанных к сделке

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