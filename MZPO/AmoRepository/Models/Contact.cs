using Newtonsoft.Json;
using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    public class Contact : IModel
    {
        [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get => "contacts"; }                      //Возвращается название ссылки на сущность, не сериализуется в JSON

        public int id { get; set; }                                                 //ID контакта
        public string name { get; set; }                                            //Название контакта
        public string first_name { get; set; }                                      //Имя контакта
        public string last_name { get; set; }                                       //Фамилия контакта
        public int? responsible_user_id { get; set; }                               //ID пользователя, ответственного за контакт
        public int? group_id { get; set; }                                          //ID группы, в которой состоит ответственны пользователь за контакт
        public int? created_by { get; set; }                                        //ID пользователя, создавший контакт
        public int? updated_by { get; set; }                                        //ID пользователя, изменивший контакт
        public int? created_at { get; set; }                                        //Дата создания контакта, передается в Unix Timestamp
        public int? updated_at { get; set; }                                        //Дата изменения контакта, передается в Unix Timestamp
        public int? closest_task_at { get; set; }                                   //Дата ближайшей задачи к выполнению, передается в Unix Timestamp
        public IList<Custom_fields_value> custom_fields_values { get; set; }        //Массив, содержащий информацию по значениям дополнительных полей, заданных для данного контакта
        public int? account_id { get; set; }                                        //ID аккаунта, в котором находится контакт
        public Links _links { get; set; }
        public Embedded _embedded { get; set; } 	 	                            //Данные вложенных сущностей

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
            public IList<Tag> tags { get; set; }                                    //Данные тегов, привязанных к сделке
            public IList<Companies> companies { get; set; }                         //Данные компании, привязанной к сделке, в данном массиве всегда 1 элемент, так как у сделки может быть только 1 компания
            public IList<Customers> customers { get; set; }                         //Требуется GET параметр with. Данные контактов, привязанных к сделке
            public IList<Lead> leads { get; set; }                                  //Требуется GET параметр with. Данные сделок, привязанных к контакту
            public IList<CatalogElements> catalog_elements { get; set; }            //Требуется GET параметр with. Данные элементов списков, привязанных к сделке

            public class Companies                                                  //Данные компании, привязанной к контакту. В массиве всегда 1 объект
            {
                public int id { get; set; }                                         //ID компании, привязанной к контакту
            }

            public class Customers                                                  //Требуется GET параметр with. Данные покупателей, привязанных к контакту 
            {
                public int id { get; set; }                                         //ID покупателя
            }

            public class CatalogElements                                            //Требуется GET параметр with. Данные элементов списков, привязанных к контакту
            {
                public int id { get; set; }                                         //ID элемента, привязанного к контакту
                public object metadata { get; set; }                                //Мета-данные элемента
                public int quantity { get; set; }                                   //Количество элементов у контакта
                public int catalog_id { get; set; }                                 //ID списка, в котором находится элемент 
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
