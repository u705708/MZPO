using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdateAmoLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Lead1C _lead1C;
        private readonly ClientRepository _1CClientRepo;
        private readonly CompanyRepository _1CCompanyRepo;
        private readonly CourseRepository _1CCourseRepo;
        private readonly LeadRepository _1CLeadRepo;

        public CreateOrUpdateAmoLead(Lead1C lead1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;
            _1CClientRepo = new();
            _1CCompanyRepo = new();
            _1CLeadRepo = new();
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

        private static int CreateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int acc_id, int contact_id, int course_id, int company_id)
        {
            Lead lead = new()
            {
                name = "Новая сделка",
                price = lead1C.price,
                custom_fields_values = new(),
                _embedded = new()
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

            #region Populating connected entities
            if (contact_id > 0)
                lead._embedded.contacts = new() { new() { id = contact_id } };
            if (course_id > 0)
                lead._embedded.catalog_elements = new() { new() { id = course_id } };
            if (company_id > 0)
                lead._embedded.companies = new() { new() { id = company_id } };

            if (lead._embedded.contacts is null &&
                lead._embedded.catalog_elements is null &&
                lead._embedded.companies is null) throw new Exception("Unable to add entities to lead."); 
            #endregion

            try
            {
                var result = leadRepo.AddNewComplex(lead);
                if (result.Any())
                    return result.First();
                else throw new Exception("Amo returned no lead Ids.");
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead1C.lead_id_1C} in amo: {e}");
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

            #region Getting connected entitites ids
            Client1C client1C = _1CClientRepo.GetClient(_lead1C.client_id_1C);
            Course1C course1C = _1CCourseRepo.GetCourse(_lead1C.product_id_1C);
            Company1C company1C = null;
            if (_lead1C.is_corporate) company1C = _1CCompanyRepo.GetCompany((Guid)_lead1C.company_id_1C);

            var contact_id = 0;
            var course_id = 0;
            var company_id = 0;

            if (client1C.amo_ids is not null &&
                client1C.amo_ids.Any(x => x.account_id == amo_acc))
                contact_id = client1C.amo_ids.First(x => x.account_id == amo_acc).entity_id;

            if (course1C.amo_ids is not null &&
                course1C.amo_ids.Any(x => x.account_id == amo_acc))
                contact_id = course1C.amo_ids.First(x => x.account_id == amo_acc).entity_id;

            if (company1C is not null &&
                company1C.amo_ids is not null &&
                company1C.amo_ids.Any(x => x.account_id == amo_acc))
                contact_id = company1C.amo_ids.First(x => x.account_id == amo_acc).entity_id; 
            #endregion

            var lead_id = CreateLeadInAmo(_lead1C, leadRepo, amo_acc, contact_id, course_id, company_id);
            _lead1C.amo_ids.Add(new() { 
                account_id = amo_acc,
                entity_id = lead_id
            });

            _1CLeadRepo.UpdateLead(_lead1C);
        }
    }
}