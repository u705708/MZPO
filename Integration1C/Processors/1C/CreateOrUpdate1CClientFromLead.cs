using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdate1CClientFromLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly IAmoRepo<Lead> leadRepo;
        private readonly IAmoRepo<Contact> mainContactRepo;
        private readonly IAmoRepo<Contact> otherContactRepo;
        private readonly Dictionary<string, int> mainFields;
        private readonly Dictionary<string, int> otherFields;
        private readonly int _lead_id;
        private readonly ClientRepository _clientRepo1C;
        
        public CreateOrUpdate1CClientFromLead(int lead_id, int acc, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            leadRepo = amo.GetAccountById(acc).GetRepo<Lead>();
            mainContactRepo = amo.GetAccountById(acc).GetRepo<Contact>();

            if (acc == 19453687)
            {
                otherContactRepo = amo.GetAccountById(28395871).GetRepo<Contact>();
                mainFields = FieldLists.ContactCorp;
                otherFields = FieldLists.ContactRet;
            }
            else 
            { 
                otherContactRepo = amo.GetAccountById(19453687).GetRepo<Contact>();
                mainFields = FieldLists.ContactRet;
                otherFields = FieldLists.ContactCorp;
            }

            _lead_id = lead_id;
            _clientRepo1C = new();
        }



        public void Run()
        {
            var lead = leadRepo.GetById(_lead_id);

            if (lead is null ||
                lead._embedded is null ||
                lead._embedded.contacts is null) return;

            foreach (var c in lead._embedded.contacts)
            {
                var contact = mainContactRepo.GetById((int)c.id);
                if (contact is null) continue;

                if (contact.custom_fields_values is not null &&
                    contact.custom_fields_values.Any(x => x.field_id == mainFields["client_id_1C"]))
                    try 
                    { 
                        _clientRepo1C.UpdateClient(Get1C.ClientFromContact(c, mainFields)); 
                    }
                    catch (Exception e)
                    { 
                        _log.Add($"Unable to update client in 1C: {e}"); 
                    }

                else
                {
                    Guid client_id = default;

                    try 
                    { 
                        client_id = _clientRepo1C.AddClient(Get1C.ClientFromContact(c, mainFields)).Client_id_1C; 
                    }

                    catch (Exception e) 
                    { 
                        _log.Add($"Unable to update client in 1C: {e}"); 
                    }

                    if (client_id == default) return;

                    if (contact.custom_fields_values is null) contact.custom_fields_values = new();

                    contact.custom_fields_values.Add(new()
                    {
                        field_id = mainFields["client_id_1C"],
                        values = new Contact.Custom_fields_value.Values[] {
                            new Contact.Custom_fields_value.Values() { value = $"{client_id}" }
                        }
                    });

                    try 
                    { 
                        mainContactRepo.Save(contact); 
                    }
                    catch (Exception e) 
                    { 
                        _log.Add($"Unable to update contact {contact.id} with 1C id {client_id}: {e}"); 
                    }
                }
            }
        }
    }
}