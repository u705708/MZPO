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

        public CreateOrUpdateAmoLead(Lead1C lead1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;
        }
        private static void AddUIDToEntity(Lead1C lead1C, int acc_id, Lead lead)
        {
            lead.custom_fields_values.Add(new Lead.Custom_fields_value()
            {
                field_id = FieldLists.Leads[acc_id]["lead_id_1C"],
                values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = lead1C.lead_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Lead1C lead1C, int acc_id, Lead lead)
        {
            foreach (var p in lead1C.GetType().GetProperties())
                if (FieldLists.Leads[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(lead1C) is not null)
                {
                    lead.custom_fields_values.Add(new Lead.Custom_fields_value()
                    {
                        field_id = FieldLists.Leads[acc_id][p.Name],
                        values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = p.GetValue(lead1C) } }
                    });
                }
        }

        private static void PopulateConnectedEntities(int contact_id, int course_id, int company_id, Lead lead)
        {
            if (contact_id > 0)
                lead._embedded.contacts = new() { new() { id = contact_id } };
            if (course_id > 0)
                lead._embedded.catalog_elements = new() { new() { id = course_id } }; //Возможно надо добавлять после создания сущности через связывание
            if (company_id > 0)
                lead._embedded.companies = new() { new() { id = company_id } };

            if (lead._embedded.contacts is null &&
                lead._embedded.catalog_elements is null &&
                lead._embedded.companies is null) throw new Exception("Unable to add entities to lead.");
        }

        private static void UpdateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int lead_id, int acc_id)
        {
            Lead lead = new()
            {
                id = lead_id,
                price = lead1C.price,
                custom_fields_values = new(),
            };

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

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

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

            PopulateConnectedEntities(contact_id, course_id, company_id, lead);

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

        public List<Amo_id> Run()
        {
            if (_lead1C.amo_ids is null) _lead1C.amo_ids = new();

            try
            {
                int amo_acc = 28395871;
                if (_lead1C.is_corporate) amo_acc = 19453687;

                var leadRepo = _amo.GetAccountById(amo_acc).GetRepo<Lead>();

                #region Checking if lead already linked to entity an updating if possible
                if (_lead1C.amo_ids.Any(x => x.account_id == amo_acc))
                {
                    UpdateLeadInAmo(_lead1C, leadRepo, _lead1C.amo_ids.First().entity_id, amo_acc);
                    return _lead1C.amo_ids;
                } 
                #endregion

                #region Getting connected entitites ids
                Client1C client1C = new ClientRepository().GetClient(_lead1C.client_id_1C);
                Course1C course1C = new CourseRepository().GetCourse(_lead1C.product_id_1C);
                Company1C company1C = null;
                if (_lead1C.is_corporate) company1C = new CompanyRepository().GetCompany((Guid)_lead1C.company_id_1C);

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

                #region Creating new lead
                var lead_id = CreateLeadInAmo(_lead1C, leadRepo, amo_acc, contact_id, course_id, company_id);
                _lead1C.amo_ids.Add(new()
                {
                    account_id = amo_acc,
                    entity_id = lead_id
                });
                #endregion
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or update lead {_lead1C.lead_id_1C} in amo: {e}");
            }

            return _lead1C.amo_ids;
        }
    }
}