using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdate1CLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CreateOrUpdate1CLead(Amo amo, Log log, int leadId, int amo_acc, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = amo_acc;
            _cred1C = cred1C;
            _filter = filter;
        }

        private static void PopulateCFs(Lead lead, int amo_acc, Lead1C lead1C)
        {
            if (lead.custom_fields_values is not null)
                foreach (var p in lead1C.GetType().GetProperties())
                    if (FieldLists.Leads[amo_acc].ContainsKey(p.Name) &&
                        lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]))
                    {
                        var value = lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]).values[0].value;
                        if ((p.PropertyType == typeof(Guid?) ||
                            p.PropertyType == typeof(Guid)) &&
                            Guid.TryParse((string)value, out Guid guidValue))
                        {
                            p.SetValue(lead1C, guidValue);
                            continue;
                        }

                        p.SetValue(lead1C, value);
                    }
        }

        private static Guid GetOrCreateContragent(Amo amo, Lead1C lead1C, Cred1C cred1C, int amo_acc, RecentlyUpdatedEntityFilter filter)
        {
            if (amo_acc == 19453687) return default;
            
            var client1C = new ClientRepository(cred1C).GetClient((Guid)lead1C.client_id_1C);

            if (client1C.amo_ids is not null &&
                client1C.amo_ids.Any(x => x.account_id == amo_acc))
            {
                var amoRepo = amo.GetAccountById(amo_acc).GetRepo<Contact>();

                var clientAmo = amoRepo.GetById(client1C.amo_ids.First(x => x.account_id == amo_acc).entity_id);

                if (clientAmo.custom_fields_values is not null &&
                    clientAmo.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[amo_acc]["company_id_1C"]) &&
                    Guid.TryParse((string)clientAmo.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[amo_acc]["company_id_1C"]).values[0].value, out Guid contragent))
                    return contragent;

                Company1C company = new()
                {
                    name = client1C.name
                };

                var contragentGuid = new CompanyRepository(cred1C).AddCompany(company);

                Contact newClientAmo = new() { 
                    id = clientAmo.id,
                    custom_fields_values = new()
                    {
                        new()
                        {
                            field_id = FieldLists.Contacts[amo_acc]["company_id_1C"],
                            values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = contragentGuid.ToString() } }
                        }
                    }
                };

                filter.AddEntity((int)clientAmo.id);

                amoRepo.Save(newClientAmo);

                return contragentGuid;
            }

            return default;
        }

        private static void GetConnectedEntities(Amo amo, Log log, Lead lead, int amo_acc, Lead1C lead1C, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            if (lead._embedded is null ||
                lead._embedded.contacts is null ||
                !lead._embedded.contacts.Any() ||
                lead._embedded.catalog_elements is null ||
                !lead._embedded.catalog_elements.Any())
                throw new Exception($"No contacts or catalog elements in lead {lead.id}");

            #region Client
            var clientId = new CreateOrUpdate1CClient(amo, log, lead.id, amo_acc, cred1C, filter).Run();

            if (clientId == default) throw new Exception($"Unable to get clientId for contact from the lead {lead.id}");

            lead1C.client_id_1C = clientId;
            #endregion

            #region Course
            var course = amo.GetAccountById(amo_acc).GetRepo<Lead>().GetCEById(lead._embedded.catalog_elements.First().id);

            if (course is not null &&
                course.custom_fields is not null &&
                course.custom_fields.Any(x => x.id == FieldLists.Courses[amo_acc]["product_id_1C"]) &&
                Guid.TryParse(course.custom_fields.First(x => x.id == FieldLists.Courses[amo_acc]["product_id_1C"]).values[0].value, out Guid product_id_1C))
                lead1C.product_id_1C = product_id_1C;
            else
                throw new Exception($"Unable to add course {course.id} from lead {lead.id}");
            #endregion

            #region Company
            if (lead1C.is_corporate &&
                lead._embedded.companies is not null &&
                lead._embedded.companies.Any())
            {
                var companyId = new CreateOrUpdate1CCompany(amo, log, lead.id, cred1C, filter).Run();

                if (companyId == default) throw new Exception($"Unable to get companyId for company from the lead {lead.id}");

                lead1C.company_id_1C = companyId;
            }

            if (lead1C.is_corporate &&
                lead1C.company_id_1C is null)
                throw new Exception($"Unable to get company in lead {lead.id}");

            if (!lead1C.is_corporate)
            {
                var companyId = GetOrCreateContragent(amo, lead1C, cred1C, amo_acc, filter);

                lead1C.company_id_1C = companyId;
            }
            #endregion
        }

        private static void UpdateLeadIn1C(Amo amo, Log log, Lead lead, Guid lead_id_1C, int amo_acc, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            var repo1C = new LeadRepository(cred1C);

            Lead1C lead1C = repo1C.GetLead(lead_id_1C);

            if (lead1C == default) throw new Exception($"Unable to update lead in 1C. 1C returned no lead {lead_id_1C}.");

            PopulateCFs(lead, amo_acc, lead1C);

            lead1C.responsible_user = UserList.Get1CUser(lead.responsible_user_id);

            if (string.IsNullOrEmpty(lead1C.lead_status))
                lead1C.lead_status = "ВРаботе";

            if (amo_acc == 19453687)
                lead1C.is_corporate = true;

            GetConnectedEntities(amo, log, lead, amo_acc, lead1C, cred1C, filter);

            System.Threading.Thread.Sleep(2000);

            repo1C.UpdateLead(lead1C);
        }

        private static Guid CreateLeadIn1C(Amo amo, Log log, Lead lead, int amo_acc, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            Lead1C lead1C = new() {
                price = (int)lead.price,
                author = UserList.Get1CUser(lead.responsible_user_id),
                responsible_user = UserList.Get1CUser(lead.responsible_user_id),
                amo_ids = new() { new() {
                        account_id = amo_acc,
                        entity_id = lead.id
            } } };

            PopulateCFs(lead, amo_acc, lead1C);

            lead1C.lead_status = "ВРаботе";

            if (amo_acc == 19453687)
                lead1C.is_corporate = true;

            GetConnectedEntities(amo, log, lead, amo_acc, lead1C, cred1C, filter);

            System.Threading.Thread.Sleep(2000);

            return new LeadRepository(cred1C).AddLead(lead1C);
        }

        private static void UpdateLeadInAmoWithUID(IAmoRepo<Lead> leadRepo, int amo_acc, int leadId, Guid uid, RecentlyUpdatedEntityFilter filter)
        {
            Lead lead = new() {
                id = leadId,
                custom_fields_values = new() { new Lead.Custom_fields_value() {
                        field_id = FieldLists.Leads[amo_acc]["lead_id_1C"],
                        values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = uid.ToString("D") } }
            } } };

            try
            {
                filter.AddEntity(leadId);
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {leadId} in amo {amo_acc}: {e}");
            }
        }

        public Guid Run()
        {
            try
            {
                var leadRepo = _amo.GetAccountById(_amo_acc).GetRepo<Lead>();

                var lead = leadRepo.GetById(_leadId);

                if (lead is null ||
                    lead == default)
                    throw new Exception("No lead returned from amo.");

                if (lead.custom_fields_values is not null &&
                    lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]) &&
                    Guid.TryParse((string)lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]).values[0].value, out Guid lead_id_1C))
                {
                    UpdateLeadIn1C(_amo, _log, lead, lead_id_1C, _amo_acc, _cred1C, _filter);

                    _log.Add($"Updated lead {lead.id} in 1C {lead_id_1C}");

                    return lead_id_1C;
                }

                var uid = CreateLeadIn1C(_amo, _log, lead, _amo_acc, _cred1C, _filter);

                _log.Add($"Created lead {lead.id} in 1C {uid}");

                UpdateLeadInAmoWithUID(leadRepo, _amo_acc, _leadId, uid, _filter);

                return uid;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or update lead {_leadId} in 1C: {e.Message}");
                return default;
            }
        }
    }
}