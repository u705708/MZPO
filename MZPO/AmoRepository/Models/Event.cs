using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    public class Event
    {
#pragma warning disable IDE1006 // Naming Styles
        public string id { get; set; }                         //ID примечания
        public string type { get; set; }
        public int? entity_id { get; set; }                 //ID родительской сущности примечания
        public string entity_type { get; set; }
        public int? created_by { get; set; }                //ID пользователя, создавший примечание
        public int? created_at { get; set; }                //Дата создания примечания, передается в Unix Timestamp
        public List<Value> value_after { get; set; }
        public List<Value> value_before { get; set; }
        public int? account_id { get; set; }
        public Links _links { get; set; }
        public Embedded _embedded { get; set; }                                 //Данные вложенных сущностей

        public class Links
        {
            public Link self { get; set; }

            public class Link
            {
                public string href { get; set; }
            }
        }

        public class Value
        {
            Task task { get; set; }
            Helpbot helpbot { get; set; }
            Transaction transaction { get; set; }
            Note note { get; set; }
            NPS nps { get; set; }
            Message message { get; set; }
            Tag tag { get; set; }
            LeadStatus lead_status { get; set; }
            CustomerStatus customer_status { get; set; }
            EntityLink link { get; set; }
            ResponsibleUser responsible_user { get; set; }
            TaskDeadline task_deadline { get; set; }
            TaskType task_type { get; set; }
            CField custom_field_value { get; set; }

            public class Task
            {
                public string text { get; set; }
            }

            public class Helpbot
            {
                public int id { get; set; }
            }

            public class Transaction
            {
                public int id { get; set; }
            }

            public class NPS
            {
                public int rate { get; set; }
            }

            public class Message
            {
                public string id { get; set; }
            }

            public class LeadStatus
            {
                public int id { get; set; }
                public int pipeline_id { get; set; }
            }

            public class CustomerStatus
            {
                public int id { get; set; }
            }

            public class EntityLink
            {
                public string type { get; set; }
                public int id { get; set; }
            }

            public class ResponsibleUser
            {
                public int id { get; set; }
            }

            public class TaskDeadline
            {
                public int timestamp { get; set; }
            }

            public class TaskType
            {
                public int id { get; set; }
            }

            public class CField
            {
                public int field_id { get; set; }
                public int field_type { get; set; }
                public int? enum_id { get; set; }
                public string text { get; set; }
            }
        }

        public class Embedded
        {
            public Entity entity { get; set; }

            public class Entity
            {
                public int id { get; set; }
                public Links _links { get; set; }
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
