using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class GSheetsProcessor
    {
        #region Definition
        private readonly Amo _amo;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly Log _log;
        private readonly int _leadNumber;
        private readonly SheetsService _service;

        public GSheetsProcessor(int leadnumber, Amo amo, GSheets gSheets, ProcessQueue processQueue, Log log, CancellationToken token)
        {
            _processQueue = processQueue;
            _token = token;
            _log = log;
            _amo = amo;
            _leadNumber = leadnumber;
            _service = gSheets.GetService();
        }

        protected static readonly (int, string)[] managers = {
            (2576764, "Администратор"),
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (2976226, "Вера Гладкова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (2375152, "Карен Оганисян"),
            (3813670, "Александра Федорова"),
            (6102562, "Валерия Лукьянова"),
            (6929800, "Исмайлова Саида"),
            (8628637, "Кубрина Людмила"),
            (8688502, "Ревина Галина"),
            (8505166, "Симкина Екатерина"),
            (8366494, "Федько Мария"),
            (8923558, "Литвина Светлана"),
            (8670964, "Афанасьева Ксения"),
            (9403926, "Белякова Влада"),
            (9193650, "Горшенина Нина"),
            (7358368, "Лидия Ковш"),
            (2375116, "Светлана Киреева"),
            (2375122, "Елена Васина"),
            (7358626, "Эллада Саланович"),
            (2375131, "Лилия Алферова"),
            (6630727, "Елена Зубатых"),
            (6028753, "Алена Федосова"),
            (6697522, "Наталья Филатова"),
            (2884132, "Ирина Сорокина"),
            (3770773, "Лидия Шталева"),
            (6200629, "Леван Харшиладзе"),
            (6346882, "Юлия Мусихина"),
            (7448173, "Инна Апостол"),
            (2375146, "Системный Администратор"),
            (7523557, "Бекташева Ленара"),
            (7532620, "Лоскутова Анастасия"),
            (7744360, "Володина Мария"),
            (7771945, "Сиренко Оксана"),
            (7744360, "Володина Мария"),
            (7824505, "Климчукова Жанна"),
        };

        private class FormulaCell
        {
            public string formula;
        }
        #endregion

        #region Supplementary methods
        private static string GetManager(int? id)
        {
            if (id is null) return "";
            if (!managers.Any(x => x.Item1 == id)) return id.ToString();
            return managers.First(x => x.Item1 == id).Item2;
        }

        private static List<Request> GetWebinarHeaderRequests(int sheetId, string title)
        {
            List<Request> requestContainer = new();

            #region Creating CellFormat for header
            var centerAlignment = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 11
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };

            var header = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 18
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            #region Adding header
            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = sheetId },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = header, UserEnteredValue = new ExtendedValue() { StringValue = title} },
            } } }
                }
            });

            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 1, SheetId = sheetId },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "№"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Менеджер"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Юр. лицо"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Организация"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "ФИО"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Электронная почта"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Телефон"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Курс"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Оплата"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Цена"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Ссылка (регистр/присутствие)"} },
                            } }
                        }
                }
            });
            #endregion

            #region Merge cells in first row
            requestContainer.Add(new Request()
            {
                MergeCells = new MergeCellsRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = 0,
                        EndRowIndex = 1,
                        StartColumnIndex = 0,
                        EndColumnIndex = 12
                    }
                }

            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 36, 96, 96, 120, 240, 180, 120, 240, 96, 96, 360 };
            int i = 0;

            foreach (var c in width)
            {
                requestContainer.Add(new Request()
                {
                    UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                    {
                        Fields = "PixelSize",
                        Range = new DimensionRange() { SheetId = sheetId, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                        Properties = new DimensionProperties() { PixelSize = c }
                    }
                });
                i++;
            }
            #endregion

            return requestContainer;
        }

        private static List<Request> GetEventsHeaderRequests(int sheetId, string title)
        {
            List<Request> requestContainer = new();

            #region Creating CellFormat for header
            var centerAlignment = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 10
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            #region Adding header
            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = sheetId },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Дата обращения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Номер сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Дата мероприятия"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Название мероприятия"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Стоимость"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "ФИО слушателя"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Телефон слушателя"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Email слушателя"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Комментарий"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "ВА/-"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Да/Нет"} },
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 120, 108, 132, 360, 84, 144, 144, 144, 120, 64, 64 };
            int i = 0;

            foreach (var c in width)
            {
                requestContainer.Add(new Request()
                {
                    UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                    {
                        Fields = "PixelSize",
                        Range = new DimensionRange() { SheetId = sheetId, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                        Properties = new DimensionProperties() { PixelSize = c }
                    }
                });
                i++;
            }
            #endregion

            return requestContainer;
        }

        private static int GetWebinarSheetId(string title, SheetsService service, string spreadsheetId)
        {
            #region Retrieving spreadsheet
            var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
            #endregion

            #region Checking for existing sheets with same title
            if (spreadsheet.Sheets.Any(x => x.Properties.Title.Contains(title)))
                return (int)spreadsheet.Sheets.First(x => x.Properties.Title.Contains(title)).Properties.SheetId;
            #endregion

            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(title));
            int sheetId = BitConverter.ToUInt16(hashed, 0) + BitConverter.ToUInt16(hashed, 3) + BitConverter.ToUInt16(hashed, 6) + BitConverter.ToUInt16(hashed, 8);

            #region Adding sheet
            List<Request> requestContainer = new()
            {
                new()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = 11,
                                FrozenRowCount = 2,
                                RowCount = 152
                            },
                            Title = title,
                            SheetId = sheetId
                        }
                    }
                }
            };
            #endregion

            requestContainer.AddRange(GetWebinarHeaderRequests(sheetId, title));

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).Execute();

            return sheetId;
        }

        private static int GetManagerSheetId(int managerId, SheetsService service, string spreadsheetId)
        {
            #region Retrieving spreadsheet
            var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
            #endregion

            #region Checking for existing sheets for this manager
            if (spreadsheet.Sheets.Any(x => x.Properties.SheetId == managerId))
                return managerId;

            if (managers.Any(x => x.Item1 == managerId) &&
                spreadsheet.Sheets.Any(x => x.Properties.Title.Contains(managers.First(x => x.Item1 == managerId).Item2)))
                return (int)spreadsheet.Sheets.First(x => x.Properties.Title.Contains(managers.First(x => x.Item1 == managerId).Item2)).Properties.SheetId;
            #endregion

            int sheetId = managerId;

            #region Adding sheet
            List<Request> requestContainer = new()
            {
                new()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = 5,
                                RowCount = 10000
                            },
                            Title = (managers.Any(x => x.Item1 == managerId)) ? managers.First(x => x.Item1 == managerId).Item2 : managerId.ToString(),
                            SheetId = sheetId
                        }
                    }
                }
            };
            #endregion

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).Execute();

            return sheetId;
        }

        private static int GetEventsSheetId(string title, SheetsService service, string spreadsheetId)
        {
            #region Retrieving spreadsheet
            var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
            #endregion

            #region Checking for existing sheets with same title
            if (spreadsheet.Sheets.Any(x => x.Properties.Title.Contains(title)))
                return (int)spreadsheet.Sheets.First(x => x.Properties.Title.Contains(title)).Properties.SheetId;
            #endregion

            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(title));
            int sheetId = BitConverter.ToUInt16(hashed, 0) + BitConverter.ToUInt16(hashed, 3) + BitConverter.ToUInt16(hashed, 6) + BitConverter.ToUInt16(hashed, 8);

            #region Adding sheet
            List<Request> requestContainer = new()
            {
                new()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = 11,
                                FrozenRowCount = 1,
                                RowCount = 1000
                            },
                            Title = title,
                            SheetId = sheetId
                        }
                    }
                }
            };
            #endregion

            requestContainer.AddRange(GetEventsHeaderRequests(sheetId, title));

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).Execute();

            return sheetId;
        }

        private static string ProcessAnswer(string input)
        {
            switch (input)
            {
                case "1": return "Да";
                case "+": return "Да";
                case "Да": return "Да";
                case "да": return "Да";
                case "y": return "Да";
                case "2": return "Нет";
                case "-": return "Нет";
                case "Нет": return "Нет";
                case "нет": return "Нет";
                case "n": return "Нет";
                default: return input;
            }

        }

        private static int GetMarathoneCode(int leadId)
        {
            int id = leadId % 1000000;
            int[] digits = new int[6];

            for (int index = 0; index < 6; index++)
            {
                digits[index] = id % 10;
                id /= 10;
            }

            return digits[0] * 10000 + digits[1] * 10 + digits[2] * 100000 + digits[3] * 1 + digits[4] * 1000 + digits[5] * 100;
        }

        private static async Task SaveData(SheetsService service, string spreadsheetId, int sheetId, params object[] cellData)
        {
            List<Request> requestContainer = new();

            var rows = new List<RowData>
                {
                    new RowData()
                    {
                        Values = new List<CellData>()
                    }
                };

            foreach (var c in cellData)
            {
                if (c.GetType() == typeof(String))
                    rows.First().Values.Add(
                        new CellData()
                        {
                            UserEnteredValue = new ExtendedValue() { StringValue = (string)c },
                            UserEnteredFormat = new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } }
                        });
                else if (c.GetType() == typeof(Int32))
                    rows.First().Values.Add(
                        new CellData()
                        {
                            UserEnteredValue = new ExtendedValue() { NumberValue = (int)c },
                            UserEnteredFormat = new CellFormat() { NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } }
                        });
                else if (c.GetType() == typeof(FormulaCell))
                    rows.First().Values.Add(
                        new CellData()
                        {
                            UserEnteredValue = new ExtendedValue() { FormulaValue = ((FormulaCell)c).formula },
                            UserEnteredFormat = new CellFormat() { NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } }
                        });
                else
                    rows.First().Values.Add(
                        new CellData()
                        {
                            UserEnteredValue = new ExtendedValue() { StringValue = c.ToString() },
                            UserEnteredFormat = new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } }
                        });
            }

            requestContainer.Add(new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            });

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            await service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).ExecuteAsync();
        } 
        #endregion

        public async Task NPS()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"NPS-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "10fXUlsSMYUk-DNo3najyRF9prllh6Oz8ycjhZAeG51Y";
                int sheetId = 0;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string course = lead.GetCFStringValue(357005);
                string educationForm = lead.GetCFStringValue(643207);
                string manager = GetManager(contact.responsible_user_id);
                int NPS = contact.GetCFIntValue(647667);
                string date = DateTime.UtcNow.AddHours(3).ToShortDateString();
                string problems = lead.GetCFStringValue(647799);
                int critic = lead.GetCFIntValue(647807);
                int neutral = lead.GetCFIntValue(647811);
                int promoter = lead.GetCFIntValue(647813);
                string comments = contact.GetCFStringValue(647289);

                await SaveData(_service, spreadsheetId, sheetId, name, phone, course, educationForm, manager, NPS, date, problems, critic, neutral, promoter, comments);
                _processQueue.Remove($"NPS-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"NPS-{_leadNumber}");
                _log.Add($"Не получилось учесть NPS для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Poll()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"Poll-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "1vIQeBXxw2iRexkfYJouzGZtq-xTOiy-pv_DVe-J1N3Q";
                int sheetId = 0;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;
                
                string answer1 = ProcessAnswer(lead.GetCFStringValue(724245));
                string answer2 = ProcessAnswer(lead.GetCFStringValue(724247));

                if (answer1 == "" &&
                    answer2 == "")
                {
                    _processQueue.Remove($"Poll-{_leadNumber}");
                    return;
                }

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string course = lead.GetCFStringValue(357005);
                string educationForm = lead.GetCFStringValue(643207);
                string dateTime = $"{DateTime.UtcNow.AddHours(3).ToShortDateString()} {DateTime.UtcNow.AddHours(3).ToShortTimeString()}";
                var examDate = lead.GetCFIntValue(644915);
                string examDateString = examDate > 0 ? DateTimeOffset.FromUnixTimeSeconds(examDate).UtcDateTime.AddHours(3).ToShortDateString() : "";

                await SaveData(_service, spreadsheetId, sheetId, name, phone, course, educationForm, answer1, answer2, dateTime, examDateString);
                _processQueue.Remove($"Poll-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"Poll-{_leadNumber}");
                _log.Add($"Не получилось учесть результат опроса для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Reprimands()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"Reprimands-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "10fXUlsSMYUk-DNo3najyRF9prllh6Oz8ycjhZAeG51Y";
                int sheetId = 1843407134;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string course = lead.GetCFStringValue(357005);
                string educationForm = lead.GetCFStringValue(643207);
                string manager = GetManager(contact.responsible_user_id);
                string reprimands = contact.GetCFStringValue(647703);
                string date = DateTime.UtcNow.AddHours(3).ToShortDateString();
                string segment = contact.GetCFStringValue(647701);
                string result = contact.GetCFStringValue(647753);

                await SaveData(_service, spreadsheetId, sheetId, name, phone, course, educationForm, manager, reprimands, date, segment, result);
                _processQueue.Remove($"Reprimands-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"Reprimands-{_leadNumber}");
                _log.Add($"Не получилось учесть замечания для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task DOD()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"DOD-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "1yOIbfo_8SqwkLkeNiQ_nb3UsitNsESupPCfyjzFwwnU";
                int sheetId = 0;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                string date = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
                string leadId = lead.id.ToString();
                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string email = contact.GetCFStringValue(264913);

                await SaveData(_service, spreadsheetId, sheetId, date, leadId, name, phone, email);
                _processQueue.Remove($"DOD-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"DOD-{_leadNumber}");
                _log.Add($"Не получилось обработать ДОД для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task CorpKP()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"SentKP-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "1xuxd7RfHTTCtalfbXfyJbx2BuNb5RflxwqgtlBamQFM";
                int accountId = 19453687;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Company> compRepo = _amo.GetAccountById(accountId).GetRepo<Company>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.companies is null)
                    return;

                int sheetId = GetManagerSheetId((int)lead.responsible_user_id, _service, spreadsheetId);

                string date = $"{DateTime.UtcNow.AddHours(3).ToShortDateString()} {DateTime.UtcNow.AddHours(3).ToShortTimeString()}";
                string leadId = lead.id.ToString();
                string leadName = lead.name;

                string companyName;
                try { companyName = compRepo.GetById(lead._embedded.companies.First().id).name; }
                catch { companyName = ""; }

                await SaveData(_service, spreadsheetId, sheetId, date, leadId, leadName, companyName);
                _processQueue.Remove($"SentKP-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"SentKP-{_leadNumber}");
                _log.Add($"Не получилось учесть отправку КП для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Webinar(string inDate, string inName, int inPrice, string inPerson, string inPhone, string inEmail)
        {
            if (_token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                string spreadsheetId = "1S20zpV80Y_IODbC0thJckuqY-3wUQLVAG6Z7H9I8Dps";
                int sheetId = DateTime.TryParse(inDate, out DateTime DT) ? GetWebinarSheetId(DT.ToShortDateString(), _service, spreadsheetId) : 0;

                FormulaCell A = new() { formula = @"=INDIRECT(""R[-1]C[0]""; FALSE)+1" };
                string B = "";
                string C = "";
                string D = "";
                string E = inPerson is null ? "" : inPerson;
                string F = inEmail is null ? "" : inEmail;
                string G = inPhone is null ? "" : inPhone;
                string H = inName is null ? "" : $"{inName} {DT.ToShortTimeString()}";
                string I = inPrice == 0 ? "бесплатно" : "";
                string J = inPrice == 0 ? "" : inPrice.ToString();

                await SaveData(_service, spreadsheetId, sheetId, A, B, C, D, E, F, G, H, I, J);
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось записать вебинар для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Events(string inDate, string inName, int inPrice, string inPerson, string inPhone, string inEmail, int leadId)
        {
            if (_token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                string spreadsheetId = "1G0podkeCiVDku2phod_2BgphfjUgWBPlmlFnZTWaqP8";
                int sheetId = DateTime.TryParse(inDate, out DateTime DT) ? GetEventsSheetId(DT.ToShortDateString(), _service, spreadsheetId) : 0;

                string A = DateTime.UtcNow.AddHours(3).ToShortDateString();
                FormulaCell B = new() { formula = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{leadId}"", ""{leadId}"")" };
                string C = inDate;
                string D = inName is null ? "" : inName; 
                string E = inPrice == 0 ? "бесплатно" : inPrice.ToString();
                string F = inPerson is null ? "" : inPerson;
                string G = inPhone is null ? "" : inPhone;
                string H = inEmail is null ? "" : inEmail;

                await SaveData(_service, spreadsheetId, sheetId, A, B, C, D, E, F, G, H);
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось записать мероприятие сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Conference(int manager, string company, string client, string email, string phone, bool paid, int price, string comments)
        {
            if (_token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                string spreadsheetId = "1rriGJJjE9Q_BylQnGoMmdV-nDFZXuI1iJgyuCtTOnMw";
                int sheetId = 0;

                FormulaCell A = new() { formula = $@"=HYPERLINK(""https://mzpoeducation.amocrm.ru/leads/detail/{_leadNumber}"", ""{_leadNumber}"")" };
                string B = GetManager(manager);
                string C = company ?? "";
                string D = client ?? "";
                string E = email ?? "";
                string F = phone ?? "";
                string G = "";
                string H = paid ? "Да" : "Нет";
                string I = $"{price}";
                string J = "";
                string K = comments ?? "";

                await SaveData(_service, spreadsheetId, sheetId, A, B, C, D, E, F, G, H, I, J, K);
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось записать конференцию для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Retail()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"Retail-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "12SjOuDW3kCABnYgxUSN3vXaB1S9dyRfdlA7YZFbuAw0";
                int sheetId = 855929522;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                string date = DateTime.Now.ToShortDateString();
                
                string company = lead.GetCFStringValue(645965);
                string companyShortName = "";
                if (company.Contains("МЦПО")) companyShortName = "МЦПО";
                if (company.Contains("МИРК")) companyShortName = "МИРК";
                
                string manager = GetManager(lead.responsible_user_id).Split(" ")[1];

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                
                string course = "";
                if (lead._embedded is not null &&
                    lead._embedded.catalog_elements is not null &&
                    lead._embedded.catalog_elements.Any())
                {
                    CatalogElement catalogElement = leadRepo.GetCEById(lead._embedded.catalog_elements.First().id);
                    if (catalogElement is not null)
                        course = catalogElement.name;
                }

                string email = contact.GetCFStringValue(264913);
                
                string phone = contact.GetCFStringValue(264911);
                
                string length = lead.GetCFStringValue(644757).Replace("ак.ч.","");
                
                string examDate = DateTimeOffset.FromUnixTimeSeconds(lead.GetCFIntValue(644915)).UtcDateTime.AddHours(3).ToShortDateString();

                await SaveData(_service, spreadsheetId, sheetId, date, "", companyShortName, manager, name, course, email, phone, length, examDate);
                _processQueue.Remove($"Retail-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"Retail-{_leadNumber}");
                _log.Add($"Не получилось передать успешную продажу в таблицу для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task CorpMeetings()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"CorpMeeting-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "19jH7uZY5swnly0KF-BGBO9iGo3wIvBh8DnknfRh8gc0";
                int accountId = 19453687;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Company> compRepo = _amo.GetAccountById(accountId).GetRepo<Company>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.companies is null)
                    return;

                int sheetId = GetManagerSheetId((int)lead.responsible_user_id, _service, spreadsheetId);

                string date = $"{DateTime.UtcNow.AddHours(3).ToShortDateString()} {DateTime.UtcNow.AddHours(3).ToShortTimeString()}";
                FormulaCell leadId = new() { formula = $@"=HYPERLINK(""https://mzpoeducation.amocrm.ru/leads/detail/{lead.id}"", ""{lead.id}"")" };
                string leadName = lead.name;

                string companyName;
                try { companyName = compRepo.GetById(lead._embedded.companies.First().id).name; }
                catch { companyName = ""; }

                string meetingResult = lead.GetCFStringValue(757953);

                await SaveData(_service, spreadsheetId, sheetId, date, leadId, leadName, companyName, meetingResult);
                _processQueue.Remove($"CorpMeeting-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"CorpMeeting-{_leadNumber}");
                _log.Add($"Не получилось учесть встречу для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task OpenLesson()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"OpenLesson-{_leadNumber}");
                return;
            }
            try
            {
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                if (contact is null ||
                    contact.custom_fields_values is null ||
                    !contact.custom_fields_values.Any())
                    return;

                string inDate = DateTimeOffset.FromUnixTimeSeconds(lead.GetCFIntValue(724347)).UtcDateTime.AddHours(3).ToString();
                string inName = $"Пробный урок по массажу";
                string inPerson = contact.name;
                string inPhone = contact.GetCFStringValue(264911);
                string inEmail = contact.GetCFStringValue(264913);

                await Webinar(inDate, inName, 0, inPerson, inPhone, inEmail);

                _processQueue.Remove($"OpenLesson-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"OpenLesson-{_leadNumber}");
                _log.Add($"Не получилось добавить информацию об открытом уроке из сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task Marathon()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"Marathon-{_leadNumber}");
                return;
            }
            try
            {
                string spreadsheetId = "1j7HGsZ_OkdZoblhHxb8xT8q3kCA9iE8aMZWiwxxIwHE";
                int sheetId = 0;
                int accountId = 28395871;

                IAmoRepo<Lead> leadRepo = _amo.GetAccountById(accountId).GetRepo<Lead>();
                IAmoRepo<Contact> contRepo = _amo.GetAccountById(accountId).GetRepo<Contact>();

                Lead lead = leadRepo.GetById(_leadNumber);

                if (lead is null
                    || lead._embedded is null
                    || lead._embedded.contacts is null
                    || DateTime.Now > new DateTime(2022, 01, 12)
                    )
                {
                    _processQueue.Remove($"Marathon-{_leadNumber}");
                    return;
                }

                Contact contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                FormulaCell leadId = new() { formula = $@"=HYPERLINK(""https://mzpoeducation.amocrm.ru/leads/detail/{_leadNumber}"", ""{_leadNumber}"")" };
                int code = GetMarathoneCode(_leadNumber);
                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string email = contact.GetCFStringValue(264913);
                string course = lead.GetCFStringValue(357005);
                int price = (int)lead.price;

                await SaveData(_service, spreadsheetId, sheetId, leadId, code, name, phone, email, price, course);

                if (lead.GetCFIntValue(725529) == 0)
                {
                    lead = new()
                    {
                        id = _leadNumber
                    };
                    lead.AddNewCF(725529, code);
                    leadRepo.Save(lead);
                }

                _processQueue.Remove($"Marathon-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"Marathon-{_leadNumber}");
                _log.Add($"Не получилось записать Марафон для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

        public async Task LK(int leadnumber, string course, string name, string email, string phone, bool status, string id, string message = "")
        {
            if (_token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                string spreadsheetId = "1BfgXuyB1kJ9l8eLsAxOUwG966WNBq203cvF-uBvFa_Y";
                int sheetId = 0;
                FormulaCell leadId = new() { formula = $@"=HYPERLINK(""https://mzpoeducation.amocrm.ru/leads/detail/{leadnumber}"", ""{leadnumber}"")" };

                await SaveData(_service, spreadsheetId, sheetId, DateTime.Now.ToShortDateString(), leadId, course, name, email, phone, status ? "создан" : "не создан", id, message);
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось записать результат создания ЛК для сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }

    }
}