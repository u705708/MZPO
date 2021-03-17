using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    internal static class FieldLists
    {
        internal static readonly Dictionary<int, Dictionary<string, int>> Contacts = new()
        {
            { 28395871, new()
                            {
                                { "client_id_1C", 710429 },
                                { "email", 264913 },
                                { "phone", 264911 },
                                { "dob", 644285 },
                                { "pass_serie", 715535 },
                                { "pass_number", 715537 },
                                { "pass_issued_by", 650839 },
                                { "pass_issued_at", 715539 },
                                { "pass_dpt_code", 710419 },
                            }
            },
            { 19453687, new()
                            {
                                { "client_id_1C", 748405 },
                                { "email", 33577 },
                                { "phone", 33575 },
                                { "dob", 68819 },
                                { "pass_serie", 748393 },
                                { "pass_number", 748395 },
                                { "pass_issued_by", 748397 },
                                { "pass_issued_at", 751223 },
                                { "pass_dpt_code", 748401 },
                            }
            },
        };

        internal static readonly Dictionary<int, Dictionary<string, int>> Companies = new()
        {
            { 28395871, new()
                            {
                                { "company_id_1C", 710403 },
                                { "email", 264913 },
                                { "phone", 264911 },
                                { "signee", 710423 },
                                { "OGRN", 644315 },
                                { "INN", 644317 },
                                { "acc_no", 710421 },
                                { "KPP", 644319 },
                                { "BIK", 644323 },
                                { "address", 644313 },
                                { "LPR_name", 710425 },
                                { "post_address", 644313 },
                            }
            },
            { 19453687, new()
                            {
                                { "company_id_1C", 748387 },
                                { "email", 33577 },
                                { "phone", 33575 },
                                { "signee", 68805 },
                                { "OGRN", 69121 },
                                { "INN", 69123 },
                                { "acc_no", 69127 },
                                { "KPP", 69125 },
                                { "BIK", 69129 },
                                { "address", 33583 },
                                { "LPR_name", 640657 },
                                { "post_address", 748389 }
                            }
            },
        };

        internal static readonly Dictionary<int, Dictionary<string, int>> Leads = new()
        {
            { 28395871, new()
                            {
                                { "lead_id_1C", 710399 },
                                { "organization", 65965 },
                                { "marketing_channel", 639075 },
                                { "marketing_source", 639085 },
                            }
            },
            { 19453687, new()
                            {
                                { "lead_id_1C", 748381 },
                                { "organization", 162301 },
                                { "marketing_channel", 748383 },
                                { "marketing_source", 748385 },
                            }
            },
        };

        internal static readonly Dictionary<int, Dictionary<string, int>> Courses = new()
        {
            { 28395871, new()
                            {
                                { "product_id_1C", 710407 },
                                { "short_name", 647993 },
                                { "price", 647997 },
                                { "duration", 715507 },
                                { "format", 715509 },
                                { "group", 647999 },
                                { "requirements", 650253 },
                                { "supplementary_info", 647995 },
                            }
            },
            { 19453687, new()
                            {
                                { "product_id_1C", 751191 },
                                { "short_name", 751165 },
                                { "price", 751169 },
                                { "duration", 751185 },
                                { "format", 751187 },
                                { "group", 751171 },
                                { "requirements", 751189 },
                                { "supplementary_info", 751167 },
                            }
            },
        };
    }
}