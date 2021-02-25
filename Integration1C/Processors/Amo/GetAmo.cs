using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    internal static class GetAmo
    {
        internal static string GetFieldValue(MZPO.AmoRepo.Lead lead, int fieldId)
        {
            if (lead.custom_fields_values is not null &&
                lead.custom_fields_values.Any(x => x.field_id == fieldId))
                return (string)lead.custom_fields_values.First(x => x.field_id == fieldId).values[0].value;
            return null;
        }

        internal static string SetFieldValue(MZPO.AmoRepo.Lead lead, int fieldId, string fieldValue)
        {
            if (lead.custom_fields_values is null) lead.custom_fields_values = new();
            lead.custom_fields_values.Add(new MZPO.AmoRepo.Lead.Custom_fields_value()
            {
                field_id = fieldId,
                values = new MZPO.AmoRepo.Lead.Custom_fields_value.Values[] { 
                    new MZPO.AmoRepo.Lead.Custom_fields_value.Values() { value = fieldValue } 
                }
            });
            return fieldValue;
        }
    }
}
