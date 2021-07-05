using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Event in amoCRM describes any activity within account, related to entities or not.
    /// </summary>
    public class AmoTask
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// ID задачи.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// ID пользователя, создавшкго задачу.
        /// </summary>
        public int? created_by { get; set; }
        /// <summary>
        /// Дата создания задачи, передается в Unix Timestamp.
        /// </summary>
        public long? created_at { get; set; }
        /// <summary>
        /// ID пользователя, обновившего задачу.
        /// </summary>
        public int? updated_by { get; set; }
        /// <summary>
        /// Дата обновления задачи, передается в Unix Timestamp.
        /// </summary>
        public long? updated_at { get; set; }
        /// <summary>
        /// ID пользователя, ответственного за задачу.
        /// </summary>
        public int? responsible_user_id { get; set; }
        /// <summary>
        /// ID группы, в которой состоит пользователь, ответственного за задачу.
        /// </summary>
        public int? group_id { get; set; }
        /// <summary>
        /// ID сущности, к которой привязана задача.
        /// </summary>
        public int? entity_id { get; set; }
        /// <summary>
        /// Тип сущности, к которой привязана задача.
        /// </summary>
        public string entity_type { get; set; }
        /// <summary>
        /// Флаг, показывающий выполнена ли задача.
        /// </summary>
        public bool is_completed { get; set; }
        /// <summary>
        /// Тип задачи.
        /// </summary>
        public int task_type_id { get; set; }

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Длительность задачи в секундах.
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Дата, когда задача должна быть завершена, передается в Unix Timestamp
        /// </summary>
        public long complete_till { get; set; }
        /// <summary>
        /// Результат выполнения задачи.
        /// </summary>
        public object result { get; set; }
        /// <summary>
        /// ID аккаунта, в котором находится событие.
        /// </summary>
        public int? account_id { get; set; }

        public class Result
        {        
            /// <summary>
            /// Текст результата выполнения задачи.
            /// </summary>
            public string text { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}