using System;
using System.Collections.Generic;
using System.Linq;

namespace MZPO.AmoRepo
{
    public static class EntityExtension
    {
        /// <summary>
        /// Возвращает первое значение поля сущности. Принимает id поля. Возвращает объект, содержащий значение поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Объект содержащий значение поля.</returns>
        public static object GetCFValue<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId))
                return entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value;
            return null;
        }

        /// <summary>
        /// Возвращает все значения поля сущности. Принимает id поля. Возвращает список, содержащий значения поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Список содержащий значения поля.</returns>
        public static IEnumerable<object> GetCFValues<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId))
                foreach (var v in entity.custom_fields_values.First(x => x.field_id == fieldId).values)
                    yield return v.value;
            yield break;
        }

        /// <summary>
        /// Возвращает первое значение поля сущности. Принимает id поля. Возвращает строку, содержащую значение поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Значение поля в виде строки.</returns>
        public static string GetCFStringValue<T>(this T entity, int fieldId) where T : IEntity
        {
            string result = "";
            if (entity.HasCF(fieldId))
                result = entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value.ToString();
            return result;
        }

        /// <summary>
        /// Возвращает значения поля сущности. Принимает id поля. Возвращает список строк, содержащий значение поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Список значений поля в виде строк.</returns>
        public static IEnumerable<string> GetCFStringValues<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId))
                foreach (var v in entity.custom_fields_values.First(x => x.field_id == fieldId).values)
                yield return v.value.ToString();
            yield break;
        }

        /// <summary>
        /// Возвращает первое значение поля сущности. Принимает id поля. Возвращает значение поля в виде числа или 0, если поле не содержит чисел.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Значение поля в виде числа или 0.</returns>
        public static int GetCFIntValue<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId) &&
                entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value.GetType() == typeof(Int32))
                return (int)entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value;

            if (entity.HasCF(fieldId) &&
                int.TryParse(entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value.ToString(), out int result))
                return result;

                return 0;
        }

        /// <summary>
        /// Возвращает значения поля сущности. Принимает id поля. Возвращает список значений поля в виде числа или 0, если значение не содержит чисел.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Список значений поля в виде чисел или 0.</returns>
        public static IEnumerable<int> GetCFIntValues<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId))
                foreach (var v in entity.custom_fields_values.First(x => x.field_id == fieldId).values)
                {
                    if (v.value.GetType() == typeof(Int32))
                        yield return (int)v.value;
                    else if (int.TryParse(v.value.ToString(), out int result))
                        yield return result;
                    else yield return 0;
                }
            
            yield break;
        }

        /// <summary>
        /// Добавляет новое поле в сущность. Принимает id поля и объект, содержащий значение поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <param name="value">Значение поля.</param>
        public static void AddNewCF<T>(this T entity, int fieldId, object value) where T : IEntity
        {
            if (entity.custom_fields_values is null) entity.custom_fields_values = new();
            entity.custom_fields_values.Add( new Custom_fields_value() {
                field_id = fieldId,
                values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = value } }
            });
        }

        /// <summary>
        /// Добавляет новое поле в сущность. Принимает id поля и список значений поля.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <param name="values">Список значений поля.</param>
        public static void AddNewCF<T>(this T entity, int fieldId, List<object> values) where T : IEntity
        {
            if (entity.custom_fields_values is null) entity.custom_fields_values = new();
            entity.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = fieldId,
                values = values.Select(x => new Custom_fields_value.Value() { value = x }).ToArray()
            });
        }

        /// <summary>
        /// Устанавливает значения поля в сущность. Если поле отсутствует, создаёт его. Принимает id поля и список значений поля.
        /// Возвращает true, если было создано новое поле в сущности.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <param name="value">Список значений поля.</param>
        /// <returns>Возвращает true, если было создано новое поле в сущности.</returns>
        public static bool SetCF<T>(this T entity, int fieldId, List<object> values) where T : IEntity
        {
            if (entity.HasCF(fieldId))
            {
                entity.custom_fields_values.First(x => x.field_id == fieldId).values = values.Select(x => new Custom_fields_value.Value() { value = x }).ToArray();
                return false;
            }

            entity.AddNewCF(fieldId, values);
            return true;
        }

        /// <summary>
        /// Показывает, есть ли поле с указанным id в сущности. Принимает id поля. Возвращает true, если такое поле в сущности есть.
        /// </summary>
        /// <param name="fieldId">Id поля.</param>
        /// <returns>Возвращает true, если такое поле в сущности есть.</returns>
        public static bool HasCF<T>(this T entity, int fieldId) where T : IEntity
        {
            return entity.custom_fields_values is not null &&
                   entity.custom_fields_values.Any(x => x.field_id == fieldId);
        }
    }
}