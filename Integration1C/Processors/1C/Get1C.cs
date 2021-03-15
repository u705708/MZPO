using MZPO.AmoRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    internal static class Get1C
    {
        internal static Client1C ClientFromContact(Contact contact, Dictionary<string, int> fieldIds)
        {
            if (contact.custom_fields_values is null) throw new Exception($"Empty contact: {contact.id}");

            Client1C result = new() { name = $"{contact.name}", client_id_1C = new() };

            if (result.name == "") throw new Exception($"No contact name: {contact.id}");

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["phone"]))
                result.phone = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["phone"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["email"]))
                result.email = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["email"]).values[0].value;

            if (result.phone == "" && result.email == "") throw new Exception($"No phone or email: {contact.id}");

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["client_id_1C"]))
            {
                Guid.TryParse((string)contact.custom_fields_values.First(x => x.field_id == fieldIds["client_id_1C"]).values[0].value, out Guid value);
                result.client_id_1C = value;
            }

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["dob"]))
                result.dob = (DateTime)contact.custom_fields_values.First(x => x.field_id == fieldIds["dob"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["pass_serie"]))
                result.pass_serie = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["pass_serie"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["pass_number"]))
                result.pass_number = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["pass_number"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["pass_issued_by"]))
                result.pass_issued_by = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["pass_issued_by"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["pass_issued_at"]))
                result.pass_issued_at = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["pass_issued_at"]).values[0].value;

            if (contact.custom_fields_values.Any(x => x.field_id == fieldIds["pass_dpt_code"]))
                result.pass_dpt_code = (string)contact.custom_fields_values.First(x => x.field_id == fieldIds["pass_dpt_code"]).values[0].value;

            return result;
        }

        internal static Company1C CompanyFromCompany(Company company, Dictionary<string, int> fieldIds)
        {
            if (company.custom_fields_values is null) throw new Exception($"Empty company: {company.id}");

            Company1C result = new() { name = $"{company.name}", company_id_1C = new() };

            if (result.name == "") throw new Exception($"No company name: {company.id}");

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["phone"]))
                result.phone = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["phone"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["email"]))
                result.email = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["email"]).values[0].value;

            if (result.phone == "" && result.email == "") throw new Exception($"No phone or email: {company.id}");

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["company_id_1C"]))
            {
                Guid.TryParse((string)company.custom_fields_values.First(x => x.field_id == fieldIds["company_id_1C"]).values[0].value, out Guid value);
                result.company_id_1C = value;
            }

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["signee"]))
                result.signee = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["signee"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["OGRN"]))
                result.OGRN = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["OGRN"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["INN"]))
                result.INN = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["INN"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["acc_no"]))
                result.acc_no = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["acc_no"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["KPP"]))
                result.KPP = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["KPP"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["BIK"]))
                result.BIK = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["BIK"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["address"]))
                result.address = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["address"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["LPR_name"]))
                result.LPR_name = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["LPR_name"]).values[0].value;

            if (company.custom_fields_values.Any(x => x.field_id == fieldIds["post_address"]))
                result.post_address = (string)company.custom_fields_values.First(x => x.field_id == fieldIds["post_address"]).values[0].value;

            return result;
        }
    }
}