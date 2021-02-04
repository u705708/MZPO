using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Event in amoCRM describes any activity within account, related to entities or not.
    /// </summary>
    public class Event
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// ID события .
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Тип события.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// ID сущности события.
        /// </summary>
        public int? entity_id { get; set; }
        /// <summary>
        /// Сущность события.
        /// </summary>
        public string entity_type { get; set; }
        /// <summary>
        /// ID пользователя, создавший событие.
        /// </summary>
        public int? created_by { get; set; }
        /// <summary>
        /// Дата создания события, передается в Unix Timestamp.
        /// </summary>
        public int? created_at { get; set; }
        /// <summary>
        /// Список с изменениями по событию.
        /// </summary>
        public List<Value> value_after { get; set; }
        /// <summary>
        /// Список с изменениями по событию.
        /// </summary>
        public List<Value> value_before { get; set; }
        /// <summary>
        /// ID аккаунта, в котором находится событие.
        /// </summary>
        public int? account_id { get; set; }
        /// <summary>
        /// Ссылки события.
        /// </summary>
        public Links _links { get; set; }
        /// <summary>
        /// Вложенные сущности.
        /// </summary>
        public Embedded _embedded { get; set; }

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
            public Task task { get; set; }
            public Helpbot helpbot { get; set; }
            public Transaction transaction { get; set; }
            public Note note { get; set; }
            public NPS nps { get; set; }
            public Message message { get; set; }
            public Tag tag { get; set; }
            public LeadStatus lead_status { get; set; }
            public CustomerStatus customer_status { get; set; }
            public EntityLink link { get; set; }
            public ResponsibleUser responsible_user { get; set; }
            public TaskDeadline task_deadline { get; set; }
            public TaskType task_type { get; set; }
            public CField custom_field_value { get; set; }

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
