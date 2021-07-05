using Newtonsoft.Json;
using System.Collections.Generic;

namespace MZPO.AmoRepo
{
	public class CatalogElement
	{
#pragma warning disable IDE1006 // Naming Styles
		/// <summary>
		/// ID элемента списка.
		/// </summary>
		public int id { get; set; }
		/// <summary>
		/// Название элемента списка.
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Дата создания элемента списка, передается в Unix Timestamp.
		/// </summary>
		public long created_at { get; set; }
		/// <summary>
		/// Дата изменения элемента списка, передается в Unix Timestamp.
		/// </summary>
		public long updated_at { get; set; }
		/// <summary>
		/// ID каталога, которому принадлежит элемент.
		/// </summary>
		public int catalog_id { get; set; }
		/// <summary>
		/// ID пользователя, создавшего элемент списка.
		/// </summary>
		public int created_by { get; set; }
		/// <summary>
		/// Является ли элемент списка удаленным.
		/// </summary>
		public bool is_deleted { get; set; }
		/// <summary>
		/// Список, содержащий информацию по значениям дополнительных полей, заданных для данного элемента списка.
		/// </summary>
		public List<Custom_fields> custom_fields { get; set; }
		/// <summary>
		/// Ссылки элемента списка.
		/// </summary>
		public Links _links { get; set; }
		/// <summary>
		/// Данные вложенных сущностей.
		/// </summary>
		public List<object> _embedded { get; set; }


		public class Custom_fields
		{
			/// <summary>
			/// ID поля.
			/// </summary>
			public int id { get; set; }
			/// <summary>
			/// Название поля.
			/// </summary>
			public string name { get; set; }
			/// <summary>
			/// Тип поля.
			/// </summary>
			public bool is_system { get; set; }
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
				public string value { get; set; }
			}
		}

		public class Links
		{
			public Link self { get; set; }

			public class Link
			{
				public string href { get; set; }
				public string method { get; set; }
			}
		}
#pragma warning restore IDE1006 // Naming Styles
	}
}