using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdateAmoLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Lead1C _lead1C;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CreateOrUpdateAmoLead(Lead1C lead1C, Amo amo, Log log, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;
            _cred1C = cred1C;
            _filter = filter;
        }

        private static int GetCatalogId(int acc_id)
        {
            if (acc_id == 19453687) return 5111;
            if (acc_id == 28395871) return 12463;
            if (acc_id == 29490250) return 5835;
            throw new Exception($"No catalog_id for account {acc_id}");
        }

        private static void AddUIDToEntity(Lead1C lead1C, int acc_id, Lead lead)
        {
            lead.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = FieldLists.Leads[acc_id]["lead_id_1C"],
                values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = lead1C.lead_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Lead1C lead1C, int acc_id, Lead lead)
        {
            foreach (var p in lead1C.GetType().GetProperties())
                if (FieldLists.Leads[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(lead1C) is not null)
                {
                    try { if ((string)p.GetValue(lead1C) == "") continue; }
                    catch { }

                    lead.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Leads[acc_id][p.Name],
                        values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = p.GetValue(lead1C) } }
                    });
                }
        }

        private static void PopulateConnectedEntities(int contact_id, int company_id, Lead lead)
        {
            if (contact_id > 0)
                lead._embedded.contacts = new() { new() { id = contact_id } };
            if (company_id > 0)
                lead._embedded.companies = new() { new() { id = company_id } };

            if (lead._embedded.contacts is null &&
                lead._embedded.companies is null) throw new Exception("Unable to add entities to lead.");
        }

        private static void UpdateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int lead_id, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Lead lead = new()
            {
                id = lead_id,
                price = lead1C.price,
                responsible_user_id = UserList.GetAmoUser(lead1C.responsible_user),
                custom_fields_values = new(),
            };

            if (lead.responsible_user_id is null)
                lead.responsible_user_id = UserList.GetAmoUser(lead1C.author);

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

            try
            {
                filter.AddEntity(lead_id);
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead_id} in amo: {e.Message}");
            }
        }

        private static int CreateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int acc_id, int contact_id, int course_id, int company_id, RecentlyUpdatedEntityFilter filter)
        {
            Lead lead = new()
            {
                name = "Новая сделка",
                price = lead1C.price,
                responsible_user_id = UserList.GetAmoUser(lead1C.responsible_user),
                custom_fields_values = new(),
                _embedded = new() { tags = new() { new() { name = "1C"} } }
            };

            if (lead.responsible_user_id is null)
                lead.responsible_user_id = UserList.GetAmoUser(lead1C.author);

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

            PopulateConnectedEntities(contact_id, company_id, lead);

            try
            {
                var result = leadRepo.AddNewComplex(lead).ToList();
                result.ForEach(x => filter.AddEntity(x));
                if (result.Any())
                {
                    EntityLink link = new()
                    {
                        to_entity_id = course_id,
                        to_entity_type = "catalog_elements",
                        metadata = new() {
                            quantity = 1,
                            catalog_id = GetCatalogId(acc_id)
                        } };

                    leadRepo.LinkEntity(result.First(), link);
                    return result.First();
                }
                else throw new Exception("Amo returned no lead Ids.");
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead1C.lead_id_1C} in amo: {e.Message}");
            }
        }

        public List<Amo_id> Run()
        {
            if (_lead1C.amo_ids is null) _lead1C.amo_ids = new();

            try
            {
                int amo_acc = 28395871;
                if (_lead1C.is_corporate) amo_acc = 19453687;
                if (_lead1C.organization == "ООО «Первый Профессиональный Институт Эстетики»") amo_acc = 29490250;

                var leadRepo = _amo.GetAccountById(amo_acc).GetRepo<Lead>();

                #region Checking if lead already linked to entity an updating if possible
                if (_lead1C.amo_ids.Any(x => x.account_id == amo_acc))
                {
                    try
                    {
                        UpdateLeadInAmo(_lead1C, leadRepo, _lead1C.amo_ids.First().entity_id, amo_acc, _filter);

                        _log.Add($"Updated lead {_lead1C.amo_ids.First().entity_id} in amo {amo_acc}.");

                        return _lead1C.amo_ids;
                    }
                    catch (Exception e)
                    {
                        _log.Add($"Unable to update existing lead {_lead1C.amo_ids.First().entity_id} in amo. Creating new.");
                    }
                } 
                #endregion

                #region Getting connected entitites ids
                Client1C client1C = new ClientRepository(_cred1C).GetClient((Guid)_lead1C.client_id_1C);
                Course1C course1C = new CourseRepository(_cred1C).GetCourse((Guid)_lead1C.product_id_1C);
                Company1C company1C = null;
                if (_lead1C.is_corporate) company1C = new CompanyRepository(_cred1C).GetCompany((Guid)_lead1C.company_id_1C);

                var contact_id = 0;
                var course_id = 0;
                var company_id = 0;

                if (client1C.amo_ids is not null &&
                    client1C.amo_ids.Any(x => x.account_id == amo_acc))
                    contact_id = client1C.amo_ids.First(x => x.account_id == amo_acc).entity_id;

                if (course1C.amo_ids is not null &&
                    course1C.amo_ids.Any(x => x.account_id == amo_acc))
                    course_id = course1C.amo_ids.First(x => x.account_id == amo_acc).entity_id;

                if (company1C is not null &&
                    company1C.amo_ids is not null &&
                    company1C.amo_ids.Any(x => x.account_id == amo_acc))
                    company_id = company1C.amo_ids.First(x => x.account_id == amo_acc).entity_id;
                #endregion

                #region Creating new lead
                var lead_id = CreateLeadInAmo(_lead1C, leadRepo, amo_acc, contact_id, course_id, company_id, _filter);
                _lead1C.amo_ids.Add(new()
                {
                    account_id = amo_acc,
                    entity_id = lead_id
                });

                _log.Add($"Created lead {lead_id} in amo {amo_acc}.");
                #endregion
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or update lead {_lead1C.lead_id_1C} in amo: {e.Message}");
            }

            return _lead1C.amo_ids;
        }
    }
}