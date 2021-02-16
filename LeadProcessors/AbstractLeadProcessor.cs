using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public abstract class AbstractLeadProcessor : ILeadProcessor
    {
        #region Definition
        protected readonly IAmoRepo<Lead> _leadRepo;
        protected readonly AmoAccount _acc;
        protected readonly TaskList _processQueue;
        protected readonly CancellationToken _token;
        protected readonly int _leadNumber;
        protected readonly Log _log;
        protected Lead lead;
        protected List<Tag> tags;
        protected List<Lead.Custom_fields_value> custom_fields_values;

        public AbstractLeadProcessor(int leadNumber, AmoAccount acc, TaskList processQueue, Log log, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _acc = acc;
            _leadNumber = leadNumber;
            _log = log;
            custom_fields_values = new();
            tags = new();

            try
            {
                Thread.Sleep((int)TimeSpan.FromSeconds(3).TotalMilliseconds);
                lead = _leadRepo.GetById(leadNumber);
                if (lead is not null && lead._embedded is not null && lead._embedded.tags is not null)
                    tags = lead._embedded.tags;
            }
            catch (Exception e)
            {
                _processQueue.Stop(leadNumber.ToString());
                _processQueue.Remove(leadNumber.ToString());
                _log.Add($"Error: Unable to create leadProcessor {leadNumber}: {e.Message}");
            }
        }

        public AbstractLeadProcessor(AmoAccount acc, TaskList processQueue, Log log, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _acc = acc;
            _log = log;
            custom_fields_values = new();
            tags = new();
        }
        #endregion

        #region Supplementary methods
        protected string GetFieldValue(int fieldId)
        {
            if (custom_fields_values.Any(x => x.field_id == fieldId))
                return (string)custom_fields_values.First(x => x.field_id == fieldId).values[0].value;

            else if (lead.custom_fields_values is not null &&
                    lead.custom_fields_values.Any(x => x.field_id == fieldId))
                return (string)lead.custom_fields_values.First(x => x.field_id == fieldId).values[0].value;

            else return null;
        }

        protected string SetFieldValue(int fieldId, string fieldValue)
        {
            if (custom_fields_values.Any(x => x.field_id == fieldId))
                custom_fields_values.First(x => x.field_id == fieldId).values[0].value = fieldValue;

            else
            {
                custom_fields_values.Add(new Lead.Custom_fields_value()
                {
                    field_id = fieldId,
                    values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = fieldValue } }
                });
            }
            return fieldValue;
        }

        protected string SetTag(string tagValue)
        {
            if (!tags.Any(x => x.name == tagValue))
                tags.Add(new Tag() { name = tagValue });

            return tagValue;
        }

        protected void UpdateLeadFromAmo()
        {
            if (_leadNumber == 0) return;

            try { lead = _leadRepo.GetById(_leadNumber); }
            catch (Exception e) { throw new Exception($"Error updating lead: {e}"); }

            List<Tag> allTags = new(tags);

            if (lead._embedded is not null && lead._embedded.tags is not null)
                allTags.AddRange(lead._embedded.tags);

            tags = allTags.Distinct(new TagComparer()).ToList();
        }

        protected void SaveLead(Lead result)
        {
            result.id = _leadNumber;
            result._embedded = new();
            UpdateLeadFromAmo();

            result.custom_fields_values = custom_fields_values.Distinct(new CFComparer()).ToList();
            result._embedded.tags = tags.Distinct(new TagComparer()).ToList();

            try { _leadRepo.Save(result); }
            catch (Exception e) { throw new Exception($"Error saving lead: {e}"); }

            custom_fields_values = new();
            tags = new();
        }

        protected void SaveLead() => SaveLead(new Lead());

        protected void SaveLead(string leadName) => SaveLead(new Lead() { name = leadName });

        protected void AddNote(string note)
        {
            try { _leadRepo.AddNotes(_leadNumber, note); }
            catch (Exception e) { throw new Exception($"Error adding note: {e}"); }
        }

        protected void AddNote(Note note)
        {
            try { _leadRepo.AddNotes(note); }
            catch (Exception e) { throw new Exception($"Error adding note: {e}"); }
        }
        #endregion

        public abstract Task Run();
    }
}