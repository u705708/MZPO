using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class InitialLeadProcessor : AbstractLeadProcessor, ILeadProcessor                                                               //Процессор осуществляет первоначальную обработку сделок
    {
        #region Definition
        private readonly GSheets _gSheets;
        private readonly Amo _amo;

        public InitialLeadProcessor(int leadNumber, AmoAccount acc, Amo amo, GSheets gSheets, ProcessQueue processQueue, Log log, CancellationToken token)
            : base(leadNumber, acc, processQueue, log, token)
        {
            _gSheets = gSheets;
            _amo = amo;
        }

        private readonly string[] sites = {
            "mirk.msk.ru",
            "mzpo-s.ru",
            "mzpo.education",
            "mzpokurs.com",
            "cruche-academy.ru",
            "курсы-сестринского-дела.рф",
            "obr-mast.ru",
            "inst-s.ru",
            "skillbank.su",
            "zoon",
            "obr-byx.ru",
            "2gis",
            "obr-komp.ru",
            "obr-sek.ru",
            "obr-diz.ru",
            "obr-men.ru",
            "курсы-косметологии.рф",
            "mirk.vrach.kosmetolog",
            "ucheba.ru"
        };                                                      //Список сайтов компании
        private readonly (string, string)[] wz = {
            ( "WZ (mirk.msk_WA)", "mirk.msk.ru" ),
            ( "WZ (mzpo-s.ru_WA)", "mzpo-s.ru" ),
            ( "WZ (mzpokurs.com_WA)", "mzpokurs.com" ),
            ( "WZ (mzpo.education_WA)", "mzpo.education" ),
            ( "WZ (skillbank_WA)", "skillbank.su" ),
            ( "WZ (cruche-academy_WA)", "cruche-academy.ru" ),
            ( "WZ (inst-s_WA)", "inst-s.ru" )
        };                                     //Список каналов whatsapp
        private readonly int[] pipelinesToProcess = {
            3198184, //основная воронка
            2231320, //корп. отдел
            3338257, //обучение очное
            4234969, //обучение дист
            3569908, //отдел сопровождения
            3245959, //модели
            3199819, //вебинары
            4586602, //пробный урок
            2263711, //подписка на рассылку
            4666393  //трудоустройство
        };
        #endregion

        private void AddToChampionate()
        {
            if (lead is not null &&
                lead._embedded is not null &&
                lead._embedded.contacts is not null &&
                lead._embedded.contacts.Any())
            {
                var repo = _acc.GetRepo<Contact>();

                var contact = repo.GetById((int)lead._embedded.contacts.First().id);

                string name = contact.name;
                string phone = contact.GetCFStringValue(264911);
                string email = contact.GetCFStringValue(264913);

                GSheetsProcessor leadProcessor = new(_leadNumber, _amo, _gSheets, _processQueue, _log, _token);
                leadProcessor.Webinar("21.08.2021", "Всероссийский чемпионат по массажу и реабилитации 21-22 августа", 0, name, phone, email).Wait();
                _log.Add($"Добавлены данные о сделке {_leadNumber} в таблицу.");
            }
        }

        #region Realization
        #region Название формы
        private void FormName()                                                                                         //Проверяем значение поля id_form и добавляем комментарий к сделке
        {
            try
            {
                string fieldValue = GetFieldValue(644511);
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    switch (fieldValue)
                    {
                        case "contactForm_tel":
                            {
                                AddNote("Форма: \"Заказать обратный звонок\"");
                                return;
                            }
                        case "contactForm":
                            {
                                AddNote("Форма: \"Записаться\"");
                                return;
                            }
                    }
                }
            }
            catch (Exception e) { throw new Exception($"FormName: {e.Message}"); }
        }
        #endregion

        #region Phase1
        private void PhaseOne()
        {
            #region Врач-косметолог
            var pageURL = GetFieldValue(639083);

            if (!string.IsNullOrEmpty(pageURL) &&
                pageURL.Contains("vrach-kosmetolog"))                                                               //Если посадочная страница содержит врач-косметолог
            {
                SetFieldValue(639081, "mirk.vrach.kosmetolog");                                                     //Устанавливаем сайт
                //SetFieldValue(639075, "");                                                                        //И тип обращения
            }
            #endregion

            #region Чемпионат
            //if (pageURL is not null &&
            //   (pageURL.Contains("https://www.mzpo-s.ru/activities/vserossiyskiy-chempionat-po-massazhu-i-reabilitacii-21-22-avgusta") ||
            //    pageURL.Contains("https://mirk.msk.ru/21-22-avgusta-chempionat-po-massazhu-i-reabilitacii")))
            //    AddToChampionate();
            //else if (tags.Any(x => x.name == "Fb_insta-chempionat"))
            //    AddToChampionate(); 
            #endregion

            #region Фитнес-инструктор
            if (!string.IsNullOrEmpty(pageURL) &&
                pageURL.Contains("rassylkapartnery_instruktor.fitness.promokod"))
            {
                AddNote("Фитнес Инструктор: Промокод 5000.");
                AddNote("Скидка клиенту 5000руб.");
                SetTag("WeGym");
            }
            #endregion

            #region Пробный урок
            if (!string.IsNullOrEmpty(pageURL) &&
                pageURL.Contains("probnyy-urok-po-massazhu"))
            {
                AddNote(@"ЗАЯВКА СО СТРАНИЦЫ ""ПРОБНЫЙ УРОК""");
                //SetTag("WeGym");
            }
            #endregion

            #region Тег по сайту
            var site = GetFieldValue(639081);
            var applicationType = GetFieldValue(639075);

            if (!string.IsNullOrEmpty(site))                                                                                           //Если поле сайт не пустое
            {
                foreach (var l in sites)                                                                                //Для каждого значения из списка сайтов
                {
                    if (site.Contains(l))                                                                               //Проверяем, содержит ли поле это значение
                    {
                        SetTag(l);                                                                                      //Устанавливаем соответствующий тег
                        break;
                    }
                }
            }
            #endregion

            #region Сайт и тег по типу обращения
            else if (!string.IsNullOrEmpty(applicationType))                                                                           //Если тип обращения не пустой
            {
                foreach (var l in sites)                                                                                //Для каждого значения из списка сайтов
                {
                    if (applicationType.Contains(l))                                                                    //Проверяем, содержит ли поле это значение
                    {
                        SetFieldValue(639081, l);                                                                       //Устанавливаем сайт
                        SetTag(l);                                                                                      //Устанавливаем тег
                        break;
                    }
                }
            }
            #endregion

            #region JivoSite
            if (tags.Any(x => x.name == "JivoSite"))                                                                    //Если тег содержит живосайт
            {
                foreach (var l in sites)                                                                                //Для каждого сайта из списка
                {
                    if (tags.Any(x => x.name == l))                                                                     //Если есть тег этого сайта
                    {
                        SetFieldValue(639081, l);                                                                       //Устанавливаем сайт
                        SetFieldValue(639075, $"Jivosite {l}");                                                         //и тип обращения
                        break;
                    }
                }
                try
                {
                    var city = GetFieldValue(639087);                                                                   //Пытаемся получить поле город
                    if (!string.IsNullOrEmpty(city))                                                                               //Если оно не пустое
                        SetFieldValue(639087, _acc.GetCityAsync(city).Result);                                          //Подставляем значение из базы городов
                }
                catch (Exception e)
                {
                    _log.Add($"Warning message: {e.Message}; Сделка: {lead.id}");
                }
            }
            #endregion

            #region Акции
            if (!string.IsNullOrEmpty(applicationType) &&
                applicationType.Contains("Акция"))
            {
                string[] words = applicationType.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder sb = new(applicationType.Length - 5);

                foreach (var word in words)
                {
                    if (word == "Акция") continue;
                    if (word == "Записаться") sb.Append(word.ToLower());
                    else sb.Append(word);
                    sb.Append(' ');
                }

                sb.Remove(sb.Length - 1, 1);

                SetFieldValue(639075, sb.ToString());
                SetTag("Акция");
            }
            #endregion

            #region Длина поля
            if (lead.custom_fields_values is not null &&
                lead.custom_fields_values.Any(x => x.values[0].value.ToString().Length > 256))
                foreach (var cf in lead.custom_fields_values.Where(x => x.values[0].value.ToString().Length > 256))
                    SetFieldValue(cf.field_id,
                        cf.values[0].value.ToString().Substring(0, 256));
            #endregion

            #region Курсы НМО
            if (lead.pipeline_id == 2231320)
                SetTag("Семинар РОМС");
            #endregion

            try { SaveLead("Новая сделка"); }
            catch (Exception e) { throw new Exception($"Phase 1: {e.Message}"); }
        }
        #endregion

        #region Ожидание телефонии
        private async Task CallResultWaiter()                                                                                 //Метод проверяет, была ли сделка из телефонии, если да, то ждёт полчаса
        {
            var source = GetFieldValue(639085);
            var application = GetFieldValue(639075);
            if (string.IsNullOrEmpty(source)) return;
            if (source.Contains("Прямой") ||
                source.Contains("Коллтрекинг") ||
                (source.Contains("instagram") && (string.IsNullOrEmpty(application) || !application.Contains("instagram"))))
            {
                for (int i = 1; i <= 60; i++)
                {
                    if (_token.IsCancellationRequested) return;                                                         //Если получили токен, то завершаем раньше, проверяем раз в минуту
                    if (!string.IsNullOrEmpty(GetFieldValue(644675))) return;                                           //Если Результат звонка заполнен, идём дальше
                    //if (GetFieldValue(644675) != "Пропущенный звонок")
                    //    return;
                    UpdateLeadFromAmo();                                                                                //Загружаем сделку
                    try { await Task.Delay((int)TimeSpan.FromSeconds(30).TotalMilliseconds, _token); }
                    catch { _log.Add($"LeadProcessor {_leadNumber}: Task cancelled."); }
                }
            }
            return;
        }
        #endregion

        #region Phase2
        private void PhaseTwo()
        {
            #region Сделки из Wazzup
            if (tags.Any())                                                                                             //Если у сделки есть теги
                foreach (var l in wz)                                                                                   //Для каждого канала WZ
                {
                    if (tags.Any(x => x.name == l.Item1))                                                               //Если у сделки есть соответствующий тег
                    {
                        SetFieldValue(639075, "whatsapp");                                                              //Тип обращения
                        SetFieldValue(639081, l.Item2);                                                                 //Сайт
                        SetFieldValue(639085, l.Item2 + "-whatsapp");                                                   //Источник(Маркер)
                        SetTag(l.Item2);                                                                                //Тег сайта
                        break;
                    }
                }
            #endregion

            #region Коллтрекинг
            var source = GetFieldValue(639085);                                                                         //Источник(Маркер)
            if (!string.IsNullOrEmpty(source))
            {
                if (tags.Any(x => x.name == "Коллтрекинг") || source.Contains("Коллтрекинг"))                           //Если сделка из коллтрекинга
                {
                    SetFieldValue(639075, "Коллтрекинг");                                                               //Тип обращения
                    SetFieldValue(639085, GetFieldValue(645583));                                                       //Источник(Маркер), roistat_marker
                    SetTag("Коллтрекинг");
                }
                #endregion
            #region Instagram
                else if (source.Contains("instagram"))
                {
                    SetFieldValue(639075, "instagram");
                    SetTag("insta");
                }
                #endregion
            #region Прямой звонок
                else if (tags.Any(x => x.name == "Прямой звонок") || source.Contains("Прямой звонок"))                  //Если прямой звонок
                {
                    SetFieldValue(639075, "Прямой звонок");                                                             //Тип обращения
                    SetTag("Прямой звонок");
                }
            }
            #endregion

            #region Ручная сделка
            if (tags.Count == 0)                                                                                        //Если тегов нет
            {
                SetTag("Ручная");
                SetFieldValue(639081, "Сделка созданная вручную");                                                      //Сайт
            }
            #endregion

            #region Результат звонка
            var applicationType = GetFieldValue(639075);                                                                //Тип обращения
            if (string.IsNullOrEmpty(GetFieldValue(644675)))                                                                          //Результат звонка
            {
                if ((applicationType is not null) && (applicationType.Contains("аявк")))
                {
                    SetFieldValue(644675, "Заявка с сайта");
                }
                else if ((applicationType is not null) && (applicationType.Contains("Jivosite")))
                {
                    SetFieldValue(644675, "Jivosite");
                }
                else if (tags.Where(x => x.name == "Ручная").Any())
                {
                    SetFieldValue(644675, "Ручная сделка");
                }
                else
                {
                    SetFieldValue(644675, applicationType);
                }
            }
            #endregion

            #region Сайт по тегу
            foreach (var l in sites)
            {
                if (tags.Any(x => x.name == l))
                {
                    SetFieldValue(639081, l);                                                                           //Сайт
                    break;
                }
            }
            #endregion

            Lead result = new();

            if (GetFieldValue(644675) == "Принят" &&                                                                    //Результат звонка
                lead.pipeline_id == 3198184 &&                                                                          //Продажи(Розница)
                lead.status_id == 32532880)                                                                             //Получен новый лид
            {
                result.pipeline_id = 3198184;                                                                           //Продажа(Розница)
                result.status_id = 32532883;                                                                            //Взят в работу
            }

            try { SaveLead(result); }
            catch (Exception e) { throw new Exception($"Phase 2: {e.Message}"); }
        }
        #endregion

        #region Соцсети
        private void SocialNetworks()
        {
            var pipelines = new List<(int, List<string>, string, string, string, int)>(){
                    (3326584, new List<string>(){"mzpo-s.ru", "insta"}, "mzpo-s.ru", "mzpo-s.ru_instagram", "instagram", 2576764),
                    (3223441, new List<string>(){"skillbank.su", "insta"}, "skillbank.su", "skillbank.su-instagram", "instagram", 2576764),
                    (3308590, new List<string>(){"mirk.msk.ru", "insta"}, "mirk.msk.ru", "mirk.msk.ru-instagram", "instagram", 2576764),
                    (3308629, new List<string>(){"mzpokurs.com", "insta"}, "mzpokurs.com", "mzpokurs.com-instagram", "instagram", 2576764),
                    (3467149, new List<string>(){"mzpo-s.ru", "vkontakte"}, "mzpo-s.ru", "mzpo-s.ru-vk", "vk", 2576764),
                    (3467545, new List<string>(){"mzpo-s.ru", "vkontakte"}, "mzpo-s.ru", "chempionat-vk", "vk", 2576764),
                    (3467551, new List<string>(){"mirk.msk.ru", "vkontakte"}, "mirk.msk.ru", "mirk.msk.ru-vk", "vk", 2576764),
                    (3467560, new List<string>(){"mzpo.education", "vkontakte"}, "mzpo.education", "mzpo.education-vk", "vk", 2576764),
                    (3467563, new List<string>(){"skillbank.su", "vkontakte"}, "skillbank.su", "skillbank.su-vk", "vk", 2576764),
                    (3499159, new List<string>(){"mirk.msk.ru", "telegram"}, "mirk.msk.ru", "mirk.msk.ru-telegram", "Telegram", 2576764),
                    (3812113, new List<string>(){"mzpo-s.ru", "facebook"}, "mzpo-s.ru", "mzpo-s.ru-facebook", "facebook", 2576764),
                    (3812128, new List<string>(){"mirk.msk.ru", "facebook"}, "mirk.msk.ru", "mirk.msk.ru-facebook", "facebook", 2576764),
                    (3812131, new List<string>(){"mzpo.education", "facebook"}, "mzpo.education", "mzpo.education-facebook", "facebook", 2576764),
                    (3812134, new List<string>(){"skillbank.su", "facebook"}, "skillbank.su", "skillbank.su-facebook", "facebook", 2576764),
                    (3812137, new List<string>(){"cruche-academy.ru", "facebook"}, "cruche-academy.ru", "cruche-academy.ru-facebook", "facebook", 2576764),
                    (4065031, new List<string>(){"ucheba.ru"}, "ucheba.ru", "Заявка с почты", "Заявка с почты uchebaru@mzpo-s.ru", 2576764),
                    (4648717, new List<string>(){"turbo.mirk.msk"}, "turbo.mirk.msk", "turbo.mirk.msk", "Заявка с сайта turbo.mirk.msk", 2576764),
                };                            //Список воронок соцсетей и соответствующих значений полей

            if (pipelines.Any(x => x.Item1 == lead.pipeline_id))                                                        //Если сделка в одной из воронок из списка
            {
                var line = pipelines.First(x => x.Item1 == lead.pipeline_id);                                           //получаем значения полей для этой воронки

                foreach (var tag in line.Item2)                                                                         //Задаём теги
                    SetTag(tag);

                SetFieldValue(639081, line.Item3);                                                                      //Сайт
                SetFieldValue(639085, line.Item4);                                                                      //Источник(Маркер)
                SetFieldValue(639075, line.Item5);                                                                      //Тип обращения
                SetFieldValue(644675, line.Item5);                                                                      //Результат звонка

                if (line.Item5 == "instagram")
                    ProcessInstaName();

                var referer = GetFieldValue(647449);
                if (!string.IsNullOrEmpty(referer))
                    AddNote(referer);

                Lead result = new() { responsible_user_id = line.Item6 };                                               //Создаём экземпляр сделки для передачи в амо

                try { SaveLead(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }

                result = new() { name = "Новая сделка", pipeline_id = 3198184, status_id = 32532880 };                  //Переводим сделку в основную воронку. Если переводить и менять ответственного одновременно, то срабатывает триггер в воронке, что может повлиять на запущенные процессы

                try { SaveLead(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }
                AddServiceNote("Processing finished.");
            }
        }
        #endregion
        #endregion

        #region Processor
        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"initial_{_leadNumber}");
                return;
            }
            try
            {
                if (lead is null)
                {
                    _processQueue.Remove($"initial_{_leadNumber}");
                    _log.Add($"Error: No lead returned from amoCRM: {_leadNumber}");
                    return;
                }
                if (pipelinesToProcess.Contains((int)lead.pipeline_id))
                {
                    bool sip = lead.name.Contains("sip");
                    
                    FormName();
                    PhaseOne();
                    AddServiceNote("Phase 1 finished.");
                    if (sip)
                        await CallResultWaiter();

                    UpdateLeadFromAmo();                                                                                    //Обновляем информацию о сделке, если она изменилась за время ожидания

                    if (lead is null)
                    {
                        _processQueue.Remove($"initial_{_leadNumber}");
                        _log.Add($"Error: No lead returned from amoCRM: {_leadNumber}");
                        return;
                    }

                    PhaseTwo();
                    AddServiceNote("Phase 2 finished.");
                }
                else
                {
                    SocialNetworks();
                }
                _processQueue.Remove($"initial_{_leadNumber}");
                _log.Add($"Success: Lead {_leadNumber}");
            }
            catch (Exception e) 
            {
                _processQueue.Remove($"initial_{_leadNumber}");
                _log.Add($"Error: Unable to process lead {_leadNumber}: {e.Message}");
                throw;
            }
        }
        #endregion
    }
}