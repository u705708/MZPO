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
        private readonly Amo _amo;
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;
        private readonly Log _log;
        private readonly int _leadNumber;
        private readonly SheetsService _service;

        public GSheetsProcessor(int leadnumber, Amo amo, GSheets gSheets, TaskList processQueue, Log log, CancellationToken token)
        {
            _processQueue = processQueue;
            _token = token;
            _log = log;
            _amo = amo;
            _leadNumber = leadnumber;
            _service = gSheets.GetService();
        }

        protected static readonly List<(int, string)> managers = new()
        {
            (2576764, "Администратор"),
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (2976226, "Вера Гладкова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (6769426, "Рюмина Наталья"),
            (6929800, "Саида Исмаилова"),
            (2375152, "Карен Оганисян"),
            (3813670, "Федорова Александра"),
            (6102562, "Валерия Лукьянова"),
            (6872548, "Оксана Полукеева"),
            (2375116, "Киреева Светлана"),
            (2375131, "Алферова Лилия"),
            (6904255, "Виктория Корчагина"),
            (6909061, "Оксана Строганова"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (6630727, "Елена Зубатых"),
            (6697522, "Наталья Филатова"),
            (3770773, "Шталева Лидия"),
            (6200629, "Харшиладзе Леван"),
            (6346882, "Мусихина Юлия")
        };

        private class FormulaCell
        {
            public string formula;
        }

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
            } } } } } );

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

        private static int GetWebinarSheetId(string title, SheetsService service, string spreadsheetId)
        {
            #region Retrieving spreadsheet
            var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
            #endregion

            #region Deleting existing sheets except first
            if (spreadsheet.Sheets.Any(x => x.Properties.Title == title))
                return (int)spreadsheet.Sheets.First(x => x.Properties.Title == title).Properties.SheetId;
            #endregion

            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(title));
            int sheetId = BitConverter.ToUInt16(hashed, 0) + BitConverter.ToUInt16(hashed, 3) + BitConverter.ToUInt16(hashed, 6) + BitConverter.ToUInt16(hashed, 8);

            #region Adding sheet
            List<Request> requestContainer = new() { new()
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
            }};
            #endregion

            requestContainer.AddRange(GetWebinarHeaderRequests(sheetId, title));

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).Execute();

            return sheetId;
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

                string date = $"{DateTime.UtcNow.AddHours(3).ToShortDateString()} {DateTime.UtcNow.AddHours(3).ToShortTimeString()}";
                string leadId = lead.id.ToString();
                string leadName = lead.name;

                string companyName;
                try { companyName = compRepo.GetById(lead._embedded.companies.First().id).name; }
                catch { companyName = ""; }

                await SaveData(_service, spreadsheetId, (int)lead.responsible_user_id, date, leadId, leadName, companyName);
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
                string H = inName is null ? "" : inName;
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

        public async Task Events(string inDate, string inName, int inPrice, string inPerson, string inPhone, string inEmail)
        {
            if (_token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                string spreadsheetId = "1G0podkeCiVDku2phod_2BgphfjUgWBPlmlFnZTWaqP8";
                int sheetId = 972104398;

                string applicationDate = $"{DateTime.UtcNow.AddHours(3).ToShortDateString()} {DateTime.UtcNow.AddHours(3).ToShortTimeString()}";
                FormulaCell leadId = new() { formula = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{_leadNumber}"", ""{_leadNumber}"")" };
                //string date = inDate is null ? "" : inDate;
                string date = DateTime.TryParse(inDate, out DateTime DT)? DT.ToShortDateString() : inDate is null ? "" : inDate;
                string name = inName is null ? "" : inName;
                string price = inPrice == 0 ? "бесплатно" : inPrice.ToString();
                string person = inPerson is null ? "" : inPerson;
                string phone = inPhone is null ? "" : inPhone;
                string email = inEmail is null ? "" : inEmail;

                await SaveData(_service, spreadsheetId, sheetId, applicationDate, leadId, date, name, price, person, phone, email);
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось записать мероприятие сделки {_leadNumber}: {e.Message}");
                throw;
            }
        }
    }
}