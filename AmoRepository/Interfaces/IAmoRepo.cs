using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an amoCRM repository.
    /// </summary>
    /// <typeparam name="T">Entity in amoCRM, must implement <see cref="IEntity"/></typeparam>
    public interface IAmoRepo<T> where T: IEntity, new()
    {
        /// <summary>
        /// Добавляет в amoCRM новые сущности. Принимает список сущностей. Возвращает список добавленных сущностей.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список добавленных сущностей.</returns>
        public IEnumerable<T> AddNew(IEnumerable<T> payload);

        /// <summary>
        /// Добавляет в amoCRM новую сущность. Принимает объект сущности. Возвращает список, содержащий добавленную сущность.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список добавленных сущностей.</returns>
        public IEnumerable<T> AddNew(T payload);        
        
        /// <summary>
        /// Добавляет в amoCRM новые сущности с проверкой на дубли. Принимает список сущностей. Возвращает список id добавленных сущностей.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список id добавленных сущностей.</returns>
        public IEnumerable<int> AddNewComplex(IEnumerable<T> payload);

        /// <summary>
        /// Добавляет в amoCRM новую сущность с проверкой на дубли. Принимает объект сущности. Возвращает список, содержащий id добавленной сущности.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список id добавленных сущностей.</returns>
        public IEnumerable<int> AddNewComplex(T payload);

        /// <summary>
        /// Возвращает из amoCRM список сущностей, запрошенных по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных сущностей.</returns>
        public IEnumerable<T> GetByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM список сущностей. Принимает список id сущностей, запрашивает пакетно по 10 штук. 
        /// </summary>
        /// <param name="ids">Список id сущностей.</param>
        /// <returns>Список найденных сущностей.</returns>
        public IEnumerable<T> BulkGetById(IEnumerable<int> ids);

        /// <summary>
        /// Возвращает из amoCRM сущность по id, если сущность не найдена, возвращает null.
        /// </summary>
        /// <param name="ids">Id сущностей.</param>
        /// <returns>Объект найденной сущности или null.</returns>
        public T GetById(int id);

        /// <summary>
        /// Сохраняет в amoCRM измененные сущности. Принимает список сущностей. Возвращает список измененных сущностей.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список измененных сущностей.</returns>
        public IEnumerable<T> Save(IEnumerable<T> payload);

        /// <summary>
        /// Сохраняет в amoCRM измененную сущность. Принимает объект сущности. Возвращает список, содержащий измененную сущность.
        /// </summary>
        /// <param name="payload">Объект сущности.</param>
        /// <returns>Список содержащий измененную сущность.</returns>
        public IEnumerable<T> Save(T payload);

        /// <summary>
        /// Возвращает из amoCRM список событий для сущности.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>Список событий сущности.</returns>
        public IEnumerable<Event> GetEntityEvents(int id);

        /// <summary>
        /// Возвращает из amoCRM список событий по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных событий.</returns>
        public IEnumerable<Event> GetEventsByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM список задач сущности.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>Список задач сущности.</returns>
        public IEnumerable<AmoTask> GetEntityTasks(int id);

        /// <summary>
        /// Возвращает из amoCRM список задач по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных задач.</returns>
        public IEnumerable<AmoTask> GetTasksByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM конкретную задачу. Возвращает null если задача не найдена.
        /// </summary>
        /// <param name="id">Id задачи.</param>
        /// <returns>Объект найденной задачи или null.</returns>
        public AmoTask GetTaskById(int id);

        /// <summary>
        /// Возвращает из amoCRM список задач. Принимает список id задач. Запрашивает пакетно по 10 штук.
        /// </summary>
        /// <param name="ids">Список id задач.</param>
        /// <returns>Список найденных задач.</returns>
        public IEnumerable<AmoTask> BulkGetTasksById(IEnumerable<int> ids);

        /// <summary>
        /// Добавляет в amoCRM задачи. Принимает список задач. Возвращает список добавленных задач.
        /// </summary>
        /// <param name="payload">Список задач.</param>
        /// <returns>Список содержащий добавленные задачи.</returns>
        public IEnumerable<AmoTask> AddTasks(IEnumerable<AmoTask> payload);

        /// <summary>
        /// Добавляет в amoCRM задачу. Принимает объект задачи. Возвращает список, содержащий добавленную задачу.
        /// </summary>
        /// <param name="note">Объект задачи.</param>
        /// <returns>Список содержащий добавленную задачу.</returns>
        public IEnumerable<AmoTask> AddTasks(AmoTask task);

        /// <summary>
        /// Добавляет к сущности amoCRM задачу. Принимает id сущности, текст задачи и срок выполнения. Возвращает список, содержащий добавленную задачу.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <param name="text">Текст задачи.</param>
        /// <param name="complete_till">Срок выполнения (в unix timestamp).</param>
        /// <returns>Список содержащий добавленную задачу.</returns>
        public IEnumerable<AmoTask> AddTasks(int entity_id, string text, long complete_till);

        /// <summary>
        /// Изменяет в amoCRM задачи. Принимает список задач. Возвращает список измененных задач.
        /// </summary>
        /// <param name="payload">Список задач.</param>
        /// <returns>Список содержащий измененные задачи.</returns>
        public IEnumerable<AmoTask> EditTasks(IEnumerable<AmoTask> payload);

        /// <summary>
        /// Изменяет в amoCRM задачу. Принимает объект задачи. Возвращает список, содержащий измененную задачу.
        /// </summary>
        /// <param name="note">Объект задачи.</param>
        /// <returns>Список содержащий изменнённую задачу.</returns>
        public IEnumerable<AmoTask> EditTasks(AmoTask task);

        /// <summary>
        /// Выполняет в amoCRM задачу. Принимает id задачи и результат выполнения. Возвращает список, содержащий выполненную задачу.
        /// </summary>
        /// <param name="id">Id задачи.</param>
        /// <param name="result_text">Результат выполнения задачи.</param>
        /// <returns>Список содержащий выполненную задачу.</returns>
        public IEnumerable<AmoTask> CompleteTask(int id, string result_text);

        /// <summary>
        /// Возвращает из amoCRM список примечаний к сущности.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>Список примечаний к сущности.</returns>
        public IEnumerable<Note> GetEntityNotes(int id);

        /// <summary>
        /// Возвращает из amoCRM список примечаний по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных примечаний.</returns>
        public IEnumerable<Note> GetNotesByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM конкретное примечание. Возвращает null если примечание не найдено.
        /// </summary>
        /// <param name="id">Id причания.</param>
        /// <returns>Объект найденного примечания или null.</returns>
        public Note GetNoteById(int id);

        /// <summary>
        /// Возвращает из amoCRM список примечаний. Принимает список id примечаний. Запрашивает пакетно по 10 штук.
        /// </summary>
        /// <param name="ids">Список id примечаний.</param>
        /// <returns>Список найденных примечаний.</returns>
        public IEnumerable<Note> BulkGetNotesById(IEnumerable<int> ids);

        /// <summary>
        /// Добавляет к сущности amoCRM примечание. Принимает id сущности и текст примечания. Возвращает список, содержащий добавленное примечание.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <param name="comment">Текст примечания.</param>
        /// <returns>Список содержащий добавленное примечание.</returns>
        public IEnumerable<Note> AddNotes(int id, string comment);

        /// <summary>
        /// Добавляет в amoCRM примечание. Принимает объект примечания. Возвращает список, содержащий добавленное примечание.
        /// </summary>
        /// <param name="note">Объект примечания.</param>
        /// <returns>Список содержащий добавленное примечание.</returns>
        public IEnumerable<Note> AddNotes(Note note);

        /// <summary>
        /// Добавляет в amoCRM примечания. Принимает список примечаний. Возвращает список добавленных примечаний.
        /// </summary>
        /// <param name="payload">Список примечаний.</param>
        /// <returns>Список содержащий добавленные примечания.</returns>
        public IEnumerable<Note> AddNotes(IEnumerable<Note> payload);

        /// <summary>
        /// Возвращает из amoCRM список тегов для сущности.
        /// </summary>
        /// <returns>Список тегов сущности.</returns>
        public IEnumerable<Tag> GetTags();

        /// <summary>
        /// Добавляет список тегов для сущности в amoCRM. Принимает список тегов. Возвращает список добавленных тегов.
        /// </summary>
        /// <param name="payload">Список тегов.</param>
        /// <returns>Список содержащий добавленные теги.</returns>
        public IEnumerable<Tag> AddTag(IEnumerable<Tag> payload);

        /// <summary>
        /// Добавляет к сущности amoCRM тег. Принимает объект тега. Возвращает список, содержащий добавленный тег.
        /// </summary>
        /// <param name="newTag">Объект тега.</param>
        /// <returns>Список содержащий добавленный тег.</returns>
        public IEnumerable<Tag> AddTag(Tag newTag);

        /// <summary>
        /// Добавляет к сущности amoCRM тег. Принимает название тега. Возвращает список, содержащий добавленный тег.
        /// </summary>
        /// <param name="tagName">Название тега.</param>
        /// <returns>Список содержащий добавленный тег.</returns>
        public IEnumerable<Tag> AddTag(string tagName);

        /// <summary>
        /// Возвращает из amoCRM список дополнительных полей сущности.
        /// </summary>
        /// <returns>Список дополнительных полей сущности.</returns>
        public IEnumerable<CustomField> GetFields();

        /// <summary>
        /// Добавляет к сущности amoCRM дополнительные поля. Принимает список дополнительных полей. Возвращает список добавленных полей.
        /// </summary>
        /// <param name="payload">Список дополнительных полей.</param>
        /// <returns>Список содержащий добавленные поля.</returns>
        public IEnumerable<CustomField> AddField(IEnumerable<CustomField> payload);

        /// <summary>
        /// Добавляет к сушности amoCRM дополнительное поле. Принимает объект дополнительного поля. Возвращает список, содержащий дополнительное поле.
        /// </summary>
        /// <param name="customField">Объект дополнительного поля.</param>
        /// <returns>Список содержащий добавленное поле.</returns>
        public IEnumerable<CustomField> AddField(CustomField customField);

        /// <summary>
        /// Добавляет к сущности amoCRM дополнительное текстовое поле. Принимает название поля. Возвращает список, содержащий добавленное поле.
        /// </summary>
        /// <param name="fieldName">Название дополнительного поля.</param>
        /// <returns>Список содержащий добавленное поле.</returns>
        public IEnumerable<CustomField> AddField(string fieldName);

        /// <summary>
        /// Возвращает из amoCRM элемент каталога. Принимает id поля.
        /// </summary>
        /// <param name="id">ID элемента каталога.</param>
        /// <returns>Объект элемента каталога.</returns>
        public CatalogElement GetCEById(int id);

        /// <summary>
        /// Возвращает из amoCRM список элементов каталога товаров.
        /// </summary>
        /// <returns>Список элементов каталога.</returns>
        public IEnumerable<CatalogElement> GetCEs();

        /// <summary>
        /// Добавляет в amoCRM элементы каталога. Принимает список элементов каталога. Возвращает список добавленных элементов каталога.
        /// </summary>
        /// <param name="elements">Список элементов каталога.</param>
        /// <returns>Список содержащий добавленные элементы каталога.</returns>
        public IEnumerable<CatalogElement> AddCEs(IEnumerable<CatalogElement> elements);

        /// <summary>
        /// Добавляет в amoCRM элемент каталога. Принимает элемент каталога. Возвращает список, содержащий добавленный элемент каталога.
        /// </summary>
        /// <param name="element">Элемент каталога.</param>
        /// <returns>Список содержащий добавленный элемент каталога.</returns>
        public IEnumerable<CatalogElement> AddCEs(CatalogElement element);

        /// <summary>
        /// Обновляет в amoCRM элементы каталога. Принимает список элементов каталога. Возвращает список обновленных элементов каталога.
        /// </summary>
        /// <param name="elements">Список элементов каталога.</param>
        /// <returns>Список содержащий обновленные элементы каталога.</returns>
        public IEnumerable<CatalogElement> UpdateCEs(IEnumerable<CatalogElement> elements);

        /// <summary>
        /// Обновляет в amoCRM элемент каталога. Принимает элемент каталога. Возвращает список, содержащий обновленный элемент каталога.
        /// </summary>
        /// <param name="element">Элемент каталога.</param>
        /// <returns>Список содержащий обновленный элемент каталога.</returns>
        public IEnumerable<CatalogElement> UpdateCEs(CatalogElement element);

        /// <summary>
        /// Удаляет из amoCRM элементы каталога. Принимает список элементов каталога. Возвращает список удаленных элементов каталога.
        /// </summary>
        /// <param name="elements">Список элементов каталога.</param>
        /// <returns>Список содержащий удаленные элементы каталога.</returns>
        public IEnumerable<CatalogElement> DeleteCEs(IEnumerable<CatalogElement> elements);

        /// <summary>
        /// Удаляет из amoCRM элемент каталога. Принимает элемент каталога. Возвращает список, содержащий удаленный элемент каталога.
        /// </summary>
        /// <param name="element">Элемент каталога.</param>
        /// <returns>Список содержащий удаленный элемент каталога.</returns>
        public IEnumerable<CatalogElement> DeleteCEs(CatalogElement element);

        /// <summary>
        /// Связывает в amoCRM различные сущности. Принимает список запросов на связь. Возвращает список связанных элементов.
        /// </summary>
        /// <param name="entity_id">Id сущности, к которой осуществляется привязка.</param>
        /// <param name="payload">Список запросов на связывание сущностей.</param>
        /// <returns>Список содержащий связанные элементы.</returns>
        public IEnumerable<EntityLink> LinkEntity(int entity_id, IEnumerable<EntityLink> payload);

        /// <summary>
        /// Связывает в amoCRM различные сущности. Принимает запрос на связь. Возвращает список связанных элементов.
        /// </summary>
        /// <param name="entity_id">Id сущности, к которой осуществляется привязка.</param>
        /// <param name="payload">Запрос на связывание сущностей.</param>
        /// <returns>Список содержащий связанные элементы.</returns>
        public IEnumerable<EntityLink> LinkEntity(int entity_id, EntityLink entityLink);

        /// <summary>
        /// Связывает в amoCRM различные сущности. Принимает параметры связываемых сущностей. Возвращает список связанных элементов.
        /// </summary>
        /// <param name="entity_id">Id сущности, к которой осуществляется привязка.</param>
        /// <param name="to_entity_id">Id привязываемой сущности.</param>
        /// <param name="to_entity_type">Тип привязываемой сущности.</param>
        /// <returns>Список содержащий связанные элементы.</returns>
        public IEnumerable<EntityLink> LinkEntity(int entity_id, int to_entity_id, string to_entity_type);

        /// <summary>
        /// Принимает в amoCRM Неразобранное по идентификатору.
        /// </summary>
        /// <param name="uid">Идентификатор Неразобранного в amoCRM.</param>
        public void AcceptUnsorted(string uid);
    }
}