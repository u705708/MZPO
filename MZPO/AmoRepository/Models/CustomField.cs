using System.Collections.Generic;

namespace MZPO.AmoRepo
{
	/// <summary>
	/// Custom field in amoCRM is a property of entites such as leads, contacts, companies, segments, customers, catalogs.
	/// </summary>
	public class CustomField
    {
#pragma warning disable IDE1006 // Naming Styles
		/// <summary>
		/// ID поля.
		/// </summary>
		public int id { get; set; }
		/// <summary>
		/// Название поля.
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Код поля, по-которому можно обновлять значение в сущности, без передачи ID поля.
		/// </summary>
		public string code { get; set; }
		/// <summary>
		/// Сортировка поля.
		/// </summary>
		public int? sort { get; set; }
		/// <summary>
		/// Тип поля.
		/// </summary>
		public string type { get; set; }
		/// <summary>
		/// ID аккаунта, в котором расположено поле.
		/// </summary>
		public int? account_id { get; set; }
		/// <summary>
		/// Тип сущности (leads, contacts, companies, segments, customers, catalogs).
		/// </summary>
		public string entity_type { get; set; }
		/// <summary>
		/// Является ли поле предустановленным. Данный ключ возвращается только при получении списка полей контактов и компаний.
		/// </summary>
		public bool? is_predefined { get; set; }
		/// <summary>
		/// Доступно ли поле для удаления. Данный ключ возвращается только при получении списка полей контактов и компаний.
		/// </summary>
		public bool? is_deletable { get; set; }
		/// <summary>
		/// Отображается ли поле в интерфейсе списка. Данный ключ возвращается только при получении списка полей списков (каталогов).
		/// </summary>
		public bool? is_visible { get; set; }
		/// <summary>
		/// Обязательно ли поле для заполнения при создании элемента списка. Данный ключ возвращается только при получении списка полей списков (каталогов).
		/// </summary>
		public bool? is_required { get; set; }
		/// <summary>
		/// Когда напоминать о дне рождения (never – никогда, day – за день, week – за неделю, month – за месяц). Значение данного поля доступно только для поля типа birthday. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний.
		/// </summary>
		public string remind { get; set; }
		/// <summary>
		/// Доступные значения для поля. Значение данного поля доступно только для полей с поддержкой enum.
		/// </summary>
		public List<Enum> enums { get; set; }
		/// <summary>
		/// Доступно ли поле для редактирования только через API. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний.
		/// </summary>
		public bool? is_api_only { get; set; }
		/// <summary>
		/// ID группы полей, в которой состоит данное поле. Данный ключ возвращается только при получении списка полей контактов, сделок, покупателей и компаний.
		/// </summary>
		public string group_id { get; set; }
		/// <summary>
		/// Обязательные поля для смены этапов. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний.
		/// </summary>
		public List<RequiredStatus> required_statuses { get; set; }

		public class Enum
		{
			/// <summary>
			/// ID значения.
			/// </summary>
			public int id { get; set; }
			/// <summary>
			/// Значение.
			/// </summary>
			public string value { get; set; }
			/// <summary>
			/// Сортировка значения.
			/// </summary>
			public int? sort { get; set; }
		}

		public class RequiredStatus

		{
			/// <summary>
			/// ID статуса, для перехода в который данное поле обязательно к заполнению. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний.
			/// </summary>
			public int? status_id { get; set; }
			/// <summary>
			/// ID поляID воронки, для перехода в который данное поле обязательно к заполнению. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний.
			/// </summary>
			public int? pipeline_id { get; set; }
		}
#pragma warning restore IDE1006 // Naming Styles
	}
}
