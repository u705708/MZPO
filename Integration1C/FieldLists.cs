using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    internal static class FieldLists
    {
        internal static readonly Dictionary<string, int> ContactRet = new() {
            { "client_id_1C", 710401 },
            { "email", 264913 },
            { "phone", 264911 },
            { "sex", 0 },
            { "dob", 644285 },
            { "pass_serie", 650835 },
            { "pass_number", 650837 },
            { "pass_issued_by", 650839 },
            { "pass_issued_at", 650841 },
            { "pass_dpt_code", 0 },
            { "address", 650841 },
        };

        internal static readonly Dictionary<string, int> ContactCorp = new()
        {
            { "client_id_1C", 0 },
            { "email", 0 },
            { "phone", 0 },
            { "sex", 0 },
            { "dob", 0 },
            { "pass_serie", 0 },
            { "pass_number", 0 },
            { "pass_issued_by", 0 },
            { "pass_issued_at", 0 },
            { "pass_dpt_code", 0 },
            { "address", 0 },
        };

        internal static readonly Dictionary<string, int> CompanyRet = new()
        {
            { "company_id_1C", 710403 },
            { "email", 264913 },
            { "phone", 264911 },
            { "signee", 644311 },
            { "OGRN", 644315 },
            { "INN", 644317 },
            { "acc_no", 0 },
            { "KPP", 644319 },
            { "BIK", 644323 },
            { "address", 644313 },
            { "LPR_name", 0 },
            { "post_address", 644313 },
        };

        internal static readonly Dictionary<string, int> CompanyCorp = new()
        {
            { "company_id_1C", 0 },
            { "email", 0 },
            { "phone", 0 },
            { "signee", 0 },
            { "OGRN", 0 },
            { "INN", 0 },
            { "acc_no", 0 },
            { "KPP", 0 },
            { "BIK", 0 },
            { "address", 0 },
            { "LPR_name", 0 },
            { "post_address", 0 },
        };

        internal static readonly Dictionary<string, int> LeadRet = new()
        {
            { "lead_id_1C", 0 },
            { "client_id_1C", 0 },
            { "product_id_1C", 0 },
            { "company_id_1C", 0 },
            { "organization", 0 },
            { "price", 0 },
            { "is_corporate", 0 },
            { "lead_status", 0 },
            { "marketing_channel", 0 },
            { "marketing_source", 0 },
            { "author", 0 },
            { "responsible_user", 0 },
        };

        internal static readonly Dictionary<string, int> LeadCorp = new()
        {
            { "lead_id_1C", 0 },
            { "client_id_1C", 0 },
            { "product_id_1C", 0 },
            { "company_id_1C", 0 },
            { "organization", 0 },
            { "price", 0 },
            { "is_corporate", 0 },
            { "lead_status", 0 },
            { "marketing_channel", 0 },
            { "marketing_source", 0 },
            { "author", 0 },
            { "responsible_user", 0 },
        };
    }
}