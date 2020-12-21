﻿using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    public class CustomField
    {
		public int id { get; set; }											//ID поля
		public string name { get; set; }									//Название поля
		public string code { get; set; }									//Код поля, по-которому можно обновлять значение в сущности, без передачи ID поля
		public int? sort { get; set; }										//Сортировка поля
		public string type { get; set; }									//Тип поля. Список доступных полей
		public int? account_id { get; set; }
		public string entity_type { get; set; }								//Тип сущности (leads, contacts, companies, segments, customers, catalogs)
		public bool? is_predefined { get; set; }							//Является ли поле предустановленным. Данный ключ возвращается только при получении списка полей контактов и компаний
		public bool? is_deletable { get; set; }								//Доступно ли поле для удаления. Данный ключ возвращается только при получении списка полей контактов и компаний
		public bool? is_visible { get; set; }								//Отображается ли поле в интерфейсе списка. Данный ключ возвращается только при получении списка полей списков (каталогов)
		public bool? is_required { get; set; }								//Обязательно ли поле для заполнения при создании элемента списка. Данный ключ возвращается только при получении списка полей списков (каталогов)
		public string remind { get; set; }									//Когда напоминать о дне рождения (never – никогда, day – за день, week – за неделю, month – за месяц). Значение данного поля доступно только для поля типа birthday. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний
		public List<Enum> enums { get; set; }								//Доступные значения для поля. Значение данного поля доступно только для полей с поддержкой enum
		public bool? is_api_only { get; set; }								//Доступно ли поле для редактирования только через API. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний
		public string group_id { get; set; }								//ID группы полей, в которой состоит данное поле. Данный ключ возвращается только при получении списка полей контактов, сделок, покупателей и компаний
		public List<RequiredStatus> required_statuses { get; set; }			//Обязательные поля для смены этапов. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний

		public class Enum													//Доступное значение для поля
		{
			public int id { get; set; }										//ID значения
			public string value { get; set; }								//Значение
			public int? sort { get; set; }									//Сортировка значения
		}

		public class RequiredStatus											//Модель обязательного поля для смены этапов. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний

		{
			public int? status_id { get; set; }								//ID статуса, для перехода в который данное поле обязательно к заполнению. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний
			public int? pipeline_id { get; set; }							//ID воронки, для перехода в который данное поле обязательно к заполнению. Данный ключ возвращается только при получении списка полей контактов, сделок и компаний 
		}
	}
}
