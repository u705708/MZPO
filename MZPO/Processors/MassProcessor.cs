using MZPO.AmoRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MZPO.Processors
{
    public class MassProcessor
    {
        protected readonly IBaseRepo<Lead> _leadRepo;
        public List<Entry> list;
        
        public class Entry
        {
            public int id { get; set; }
            public string name { get; set; }
        }


        public MassProcessor(IBaseRepo<Lead> leadRepo)
        {
            _leadRepo = leadRepo;
            list = new List<Entry>();
        }

        public void Run()
        {
            JsonConvert.PopulateObject(File.ReadAllText(@"todo.json"), list);
            var leads = new List<Lead>();
            int i = 0;

            foreach (var lead in list)
            {
                i++;
                leads.Add(new Lead()
                {
                    id = lead.id,
                    custom_fields_values = new List<Lead.Custom_fields_value>(){
                        new Lead.Custom_fields_value(){
                            field_id = 639081,
                            values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values(){ value = lead.name } }
                        }
                    }
                });

                if (i==50)
                {
                    i = 0;

                    var result = _leadRepo.Save(leads);
                    if (!result.Any()) throw new Exception();

                    leads = new List<Lead>();
                }
            }
            if (leads.Any())
                _leadRepo.Save(leads);
        }
    }
}
