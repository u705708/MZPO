using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class UpdateAmoLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Lead1C _lead1C;
        private readonly ClientRepository _1CClientRepo;
        private readonly CompanyRepository _1CCompanyRepo;
        private readonly CourseRepository _1CCourseRepo;

        public UpdateAmoLead(Lead1C lead1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;
            _1CClientRepo = new();
            _1CCompanyRepo = new();
        }

        private static void UpdateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int lead_id, int acc_id)
        {
            Lead lead = new()
            {
                id = lead_id,
                price = lead1C.price,
                custom_fields_values = new(),
            };

            lead.custom_fields_values.Add(new Lead.Custom_fields_value()
            {
                field_id = FieldLists.Leads[acc_id]["lead_id_1C"],
                values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = lead1C.lead_id_1C.ToString("D") } }
            });

            #region Populating custom fields
            foreach (var p in lead1C.GetType().GetProperties())
                if (FieldLists.Leads[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(lead1C) is not null &&
                    (string)p.GetValue(lead1C) != "") //В зависимости от политики передачи пустых полей
                {
                    lead.custom_fields_values.Add(new Lead.Custom_fields_value()
                    {
                        field_id = FieldLists.Leads[acc_id][p.Name],
                        values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = (string)p.GetValue(lead1C) } }
                    });
                }
            #endregion
            try
            {
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead_id} in amo: {e}");
            }
        }

        public void Run()
        {
            #region Updating connected entities
            if (_lead1C.client_id_1C != default)
            {
                var client = _1CClientRepo.GetClient(_lead1C.client_id_1C);
                new CreateOrUpdateAmoContact(client, _amo, _log).Run();
            }

            if (_lead1C.product_id_1C != default)
            {
                var course = _1CCourseRepo.GetCourse(_lead1C.product_id_1C);
                new CreateOrUpdateAmoCourse(course, _amo, _log).Run();
            }

            if (_lead1C.company_id_1C is not null &&
                _lead1C.company_id_1C != default)
            {
                var company = _1CCompanyRepo.GetCompany((Guid)_lead1C.company_id_1C);
                new CreateOrUpdateAmoCompany(company, _amo, _log).Run();
            }
            #endregion

            int amo_acc = 28395871;
            if (_lead1C.is_corporate) amo_acc = 19453687;

            var leadRepo = _amo.GetAccountById(amo_acc).GetRepo<Lead>();

            if (_lead1C.amo_ids.Any(x => x.account_id == amo_acc))
            {
                UpdateLeadInAmo(_lead1C, leadRepo, _lead1C.amo_ids.First().entity_id, amo_acc);
                return;
            }
        }
    }
}