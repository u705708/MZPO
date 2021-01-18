using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MZPO.Processors
{
    public class UnfinishedContactsProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly BaseRepository<Lead> leadRepo;
        private readonly BaseRepository<Company> compRepo;
        private readonly BaseRepository<Contact> contRepo;
        protected readonly CancellationToken _token;

        public UnfinishedContactsProcessor(AmoAccount acc, TaskList processQueue, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _token = token;
            leadRepo = _acc.GetRepo<Lead>();
            compRepo = _acc.GetRepo<Company>();
            contRepo = _acc.GetRepo<Contact>();
        }

        List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана"),
            (2375122, "Васина Елена"),
            (2375131, "Алферова Лилия"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова")
        };
        #endregion

        #region Realization
        public void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("report_data");
                return;
            }

            foreach (var m in managers)
            {
                if (_token.IsCancellationRequested) break;

                var allLeads = leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=1121263&filter[statuses][0][status_id]=19529785&filter[responsible_user_id]={m.Item1}&with=companies,contacts");

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using StreamWriter sw = new StreamWriter($"{m.Item2}.csv", true, Encoding.GetEncoding("windows-1251"));
                //sw.WriteLine("Номер сделки;Название контакта;Название компании;Есть телефоны?;Есть email?;ЛПР");

                if (allLeads == null) continue;

                foreach (var l in allLeads)
                {
                    if (_token.IsCancellationRequested) break;

                    int leadNumber = l.id;
                    string contactName;
                    string companyName;
                    bool phoneAdded = false;
                    bool emailAdded = false;
                    string LPR = "";

                    Company company;
                    List<Contact> contacts = new List<Contact>();

                    if (leadNumber == 19849577) continue;

                    if (l._embedded.companies.Any())
                        company = compRepo.GetById(l._embedded.companies.FirstOrDefault().id);
                    else
                        company = new Company();

                    companyName = company.name;

                    if ((company.custom_fields_values != null) &&
                        company.custom_fields_values.Any(x => x.field_id == 640657))
                        LPR = (string)company.custom_fields_values.FirstOrDefault(x => x.field_id == 640657).values[0].value;

                    if ((l._embedded.contacts != null) &&
                        (l._embedded.contacts.Any()))
                        foreach(var c in l._embedded.contacts)
                            contacts.Add(contRepo.GetById(c.id));

                    if (contacts.Any())
                        contactName = contacts.First().name;
                    else contactName = "";

                    if ((company.custom_fields_values != null) &&
                        company.custom_fields_values.Any(x => x.field_id == 33575))
                        phoneAdded = true;
                    if ((company.custom_fields_values != null) &&
                        company.custom_fields_values.Any(x => x.field_id == 33577))
                        emailAdded = true;

                    if (contacts.Any())
                        foreach (var c in contacts)
                        {
                            if (c.custom_fields_values == null) continue;
                            if (c.custom_fields_values.Any(x => x.field_id == 33575))
                                phoneAdded = true;
                            if (c.custom_fields_values.Any(x => x.field_id == 33577))
                                emailAdded = true;
                        }

                    if (!phoneAdded || !emailAdded)
                        sw.WriteLine($"{leadNumber};{contactName};{companyName};{phoneAdded};{emailAdded};{LPR}");
                }
                GC.Collect();
            }
            _processQueue.Remove("report_data");
        }
        #endregion
    }
}