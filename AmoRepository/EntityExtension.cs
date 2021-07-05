using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    public static class EntityExtension
    {
        public static object GetCFValue<T>(this T entity, int fieldId) where T : IEntity
        {
            if (entity.HasCF(fieldId))
                return entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value;
            return null;
        }

        public static string GetCFStringValue<T>(this T entity, int fieldId) where T : IEntity
        {
            string result = "";
            if (entity.HasCF(fieldId))
                result = entity.custom_fields_values.First(x => x.field_id == fieldId).values[0].value.ToString();
            return result;
        }

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

        public static void AddNewCF<T>(this T entity, int fieldId, object value) where T : IEntity
        {
            if (entity.custom_fields_values is null) entity.custom_fields_values = new();
            entity.custom_fields_values.Add( new Custom_fields_value() {
                field_id = fieldId,
                values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = value } }
            });
        }

        public static bool HasCF<T>(this T entity, int fieldId) where T : IEntity
        {
            return entity.custom_fields_values is not null &&
                   entity.custom_fields_values.Any(x => x.field_id == fieldId);
        }
    }
}