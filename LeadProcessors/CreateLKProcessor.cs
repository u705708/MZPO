using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace MZPO.LeadProcessors
{
    public class CreateLKProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly string _taskName;
        private readonly int _leadNumber;
        private readonly Result _result;

        public CreateLKProcessor(Amo amo, Log log, ProcessQueue processQueue, CancellationToken token, GSheets gSheets, string taskName, int leadNumber)
        {
            _amo = amo;
            _gSheets = gSheets;
            _log = log;
            _processQueue = processQueue;
            _token = token;
            _taskName = taskName;
            _leadNumber = leadNumber;
            _result = new()
            {
                leadNumber = leadNumber,
                name = "",
                email = "",
                phone = "",
                course = "",
                created = false,
                message = ""
            };

            var acc = amo.GetAccountById(28395871);
            _leadRepo = acc.GetRepo<Lead>();
            _contRepo = acc.GetRepo<Contact>();
        }

        private class Result
        {
            public int leadNumber;
            public string name;
            public string email;
            public string phone;
            public string course;
            public bool created;
            public string message;
        }

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskName);
                return Task.FromCanceled(_token);
            }
            try
            {
                #region Getting lead
                Lead lead = _leadRepo.GetById(_leadNumber);
                #endregion

                #region Getting contact
                if (lead._embedded?.contacts is null ||
                    !lead._embedded.contacts.Any())
                {
                    _leadRepo.AddNotes(_leadNumber, "Не удалось создать личный кабинет. В сделке отсутствует контакт");
                    throw new InvalidOperationException("В сделке отсутствует контакт");
                }

                List<Contact> contacts = _contRepo.BulkGetById(lead._embedded.contacts.Select(x => (int)x.id)).ToList();

                Contact contact = contacts.First();

                if (contacts.Any(x => x.HasCF(726297)))
                    contact = contacts.First(x => x.HasCF(726297));

                _result.name = contact.name;
                _result.email = contact.GetCFStringValue(264913);
                _result.phone = contact.GetCFStringValue(264911);
                #endregion

                #region Getting list of available courses
                List<string> courses = new();

                try
                {
                    using FileStream stream = new("CoursesForLK.json", FileMode.Open, FileAccess.Read);
                    using StreamReader sr = new(stream);
                    JsonConvert.PopulateObject(sr.ReadToEnd(), courses);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Не удалось обработать CoursesForLK.json: {e.Message}");
                }
                #endregion

                #region Checking course
                string leadCourse = lead.GetCFStringValue(357005);
                _result.course = leadCourse;

                if (string.IsNullOrEmpty(leadCourse) ||
                    !courses.Contains(leadCourse))
                    throw new InvalidOperationException("В сделке отсутствует подходящий курс");
                #endregion

                #region Checking for 1c id
                if (!contact.HasCF(710429) ||
                    string.IsNullOrEmpty(contact.GetCFStringValue(710429)))
                {
                    _leadRepo.AddNotes(_leadNumber, "Не удалось создать личный кабинет. Контакт не привязан к 1С");
                    throw new InvalidOperationException("Контакт не привязан к 1С");
                }
                #endregion

                #region Creating request and getting response
                DateTime birthdayDate = DateTimeOffset.FromUnixTimeSeconds(contact.GetCFIntValue(644285)).UtcDateTime.AddHours(3);
                DateTime passDoiDate = DateTimeOffset.FromUnixTimeSeconds(contact.GetCFIntValue(718557)).UtcDateTime.AddHours(3);

                CreateLKRequest request = new()
                {
                    name = contact.name,
                    login = contact.GetCFStringValue(264913),
                    email = contact.GetCFStringValue(264913),
                    phone = contact.GetCFStringValue(264911),
                    birthday = $"{birthdayDate.Year:d4}-{birthdayDate.Month:d2}-{birthdayDate.Day:d2}",
                    sex = contact.GetCFStringValue(710417) == "М" ? "male" : "female",
                    snils = contact.GetCFStringValue(724399),
                    pass_series = contact.GetCFStringValue(715535),
                    pass_number = contact.GetCFStringValue(715537),
                    pass_doi = $"{passDoiDate.Year:d4}-{passDoiDate.Month:d2}-{passDoiDate.Day:d2}",
                    pass_poi = contact.GetCFStringValue(650841),
                    pass_dpt = contact.GetCFStringValue(710419),
                    pass_registration = contact.GetCFStringValue(650843),
                    id_1c = contact.GetCFStringValue(710429),
                    amo_id = contact.id.ToString(),
                    id = contact.GetCFStringValue(726299)
                };

                CreateLKResponse response = new MZPOLK(request).CreateLKAsync().GetAwaiter().GetResult(); 
                #endregion

                #region Saving result to amo as note
                bool created = true;
                string message = "Создан ЛК для клиента.";

                if (response.user is null)
                {
                    created = false;
                    message = $"Не удалось создать ЛК для клиента: {(response.email is not null ? response.email[0] : string.Empty)} {(response.phone is not null ? response.phone[0] : string.Empty)}";
                    _result.message = message;
                }

                _result.created = created;

                _leadRepo.AddNotes(_leadNumber, message); 
                #endregion

                #region Saving LK ID to amo
                if (created)
                {
                    Contact createdLKContact = new()
                    {
                        id = contact.id
                    };

                    createdLKContact.AddNewCF(726299, response.user.ToString());

                    _contRepo.Save(createdLKContact);
                }
                #endregion

                #region Saving result to lead
                Lead saveLead = new()
                {
                    id = _leadNumber
                };
                saveLead.AddNewCF(728583, created ? "Да" : "Нет");
                _leadRepo.Save(saveLead); 
                #endregion

                #region Adding to google sheets
                GSheetsProcessor gProcessor = new(_leadNumber, _amo, _gSheets, _processQueue, _log, _token);
                gProcessor.LK(_leadNumber, leadCourse, request.name, request.email, request.phone, created, created ? response.user.ToString() : "", _result.message) .Wait();
                _log.Add($"Добавлены данные о создании ЛК из сделки {_leadNumber} в таблицу.");
                #endregion

                _processQueue.Remove(_taskName);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                try
                {
                    #region Saving result to lead
                    Lead saveLead = new()
                    {
                        id = _leadNumber
                    };
                    saveLead.AddNewCF(728583, "Нет");
                    _leadRepo.Save(saveLead);
                    #endregion

                    #region Adding to google sheets
                    GSheetsProcessor gProcessor = new(_leadNumber, _amo, _gSheets, _processQueue, _log, _token);
                    gProcessor.LK(_result.leadNumber, _result.course, _result.name, _result.email, _result.phone, false, "", e.Message).Wait();
                    #endregion
                }
                catch { }
                _log.Add($"Не удалось создать ЛК для сделки {_leadNumber}: {e}.");
                _processQueue.Remove(_taskName);
                return Task.FromException(e);
            }
        }
    }
}