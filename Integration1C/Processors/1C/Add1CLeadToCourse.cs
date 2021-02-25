using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Add1CLeadToCourse
    {
        private readonly AmoAccount _acc;
        private readonly int _lead_id;
        private readonly LeadRepository _leadRepo1C;
        private readonly Log _log;

        public Add1CLeadToCourse(int lead_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _lead_id = lead_id;
            _leadRepo1C = new();
            _log = log;
        }

        public void Run()
        {
            var leadRepo = _acc.GetRepo<Lead>();

            var lead = leadRepo.GetById(_lead_id);

            if (lead is null) return;

            Dictionary<string, int> fieldIds;
            if (_acc.id == 19453687) fieldIds = FieldLists.LeadCorp;
            else fieldIds = FieldLists.LeadRet;

            if (lead.custom_fields_values is not null &&
                lead.custom_fields_values.Any(x => x.field_id == fieldIds["lead_id_1C"]))
                try { _leadRepo1C.AddToCourse((int)lead.custom_fields_values.First(x => x.field_id == fieldIds["lead_id_1C"]).values[0].value); }
                catch (Exception e) { _log.Add($"Unable to add lead to course in 1C: {e}"); }
        }
    }
}