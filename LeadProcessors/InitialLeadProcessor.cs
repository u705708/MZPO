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
        public InitialLeadProcessor(int leadNumber, AmoAccount acc, TaskList processQueue, Log log, CancellationToken token) 
            : base(leadNumber, acc, processQueue, log, token) { }

        private readonly List<string> sites = new List<string>
        {
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
        private readonly List<(string, string)> wz = new List<(string, string)>
        {
            ( "WZ (mirk.msk_WA)", "mirk.msk.ru" ),
            ( "WZ (mzpo-s.ru_WA)", "mzpo-s.ru" ),
            ( "WZ (mzpokurs.com_WA)", "mzpokurs.com" ),
            ( "WZ (mzpo.education_WA)", "mzpo.education" ),
            ( "WZ (skillbank_WA)", "skillbank.su" ),
            ( "WZ (cruche-academy_WA)", "cruche-academy.ru" ),
            ( "WZ (inst-s_WA)", "inst-s.ru" )
        };                                     //Список каналов whatsapp
        #endregion

        #region Realization
        #region Название формы
        private void FormName()                                                                                         //Проверяем значение поля id_form и добавляем комментарий к сделке
        {
            try
            {
                string fieldValue = GetFieldValue(644511);
                if (fieldValue is not null)
                {
                    switch (fieldValue)
                    {
                        case "contactForm_tel":
                            {
                                AddNote(new Note() { entity_id = lead.id, note_type = "common", parameters = new Note.Params() { text = "Форма: \"Заказать обратный звонок\"" } });
                                return;
                            }
                        case "contactForm":
                            {
                                AddNote(new Note() { entity_id = lead.id, note_type = "common", parameters = new Note.Params() { text = "Форма: \"Записаться\"" } });
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

            if (pageURL is not null)
            {
                if (pageURL.Contains("vrach-kosmetolog"))                                                               //Если посадочная страница содержит врач-косметолог
                {
                    SetFieldValue(639081, "mirk.vrach.kosmetolog");                                                     //Устанавливаем сайт
                    //SetFieldValue(639075, "");                                                                        //И тип обращения
                }
            }
            #endregion

            #region Тег по сайту
            var site = GetFieldValue(639081);
            var applicationType = GetFieldValue(639075);

            if (site is not null)                                                                                           //Если поле сайт не пустое
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
            else if (applicationType is not null)                                                                           //Если тип обращения не пустой
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
                    if (city is not null)                                                                               //Если оно не пустое
                        SetFieldValue(639087, _acc.GetCity(city));                                                      //Подставляем значение из базы городов
                }
                catch (Exception e)
                {
                    _log.Add($"Warning message: {e.Message}; Сделка: {lead.id}");
                }
            }
            #endregion

            #region Акции
            if (applicationType is not null &&
                applicationType.Contains("Акция"))
            {
                string[] words = applicationType.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder sb = new StringBuilder(applicationType.Length - 5);

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
                SetTag("Вебинар НМО неврология");
            #endregion

            try { SaveLead("Новая сделка"); }
            catch (Exception e) { throw new Exception($"Phase 1: {e.Message}"); }
        }
        #endregion

        #region Ожидание телефонии
        private void CallResultWaiter()                                                                                 //Метод проверяет, была ли сделка из телефонии, если да, то ждёт полчаса
        {
            var source = GetFieldValue(639085);
            if (source is null) return;
            if (source.Contains("Прямой") || source.Contains("Коллтрекинг") || source.Contains("instagram"))
            {
                for (int i = 1; i <= 30; i++)
                {
                    if (_token.IsCancellationRequested) return;                                                         //Если получили токен, то завершаем раньше, проверяем раз в минуту
                    try { Task.Delay((int)TimeSpan.FromSeconds(60).TotalMilliseconds, _token).Wait(); }
                    catch { _log.Add($"LeadProcessor {_leadNumber}: Task cancelled."); }
                    UpdateLeadFromAmo();                                                                                //Загружаем сделку
                    if (GetFieldValue(644675) is not null) return;                                                      //Если Результат звонка заполнен, идём дальше
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
            if (source is not null)
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
            if (GetFieldValue(644675) is null)                                                                          //Результат звонка
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
                    (3812137, new List<string>(){ "cruche-academy.ru", "facebook"}, "cruche-academy.ru", "cruche-academy.ru-facebook", "facebook", 2576764),
                    (4065031, new List<string>(){ "ucheba.ru"}, "ucheba.ru", "Заявка с почты", "Заявка с почты uchebaru@mzpo-s.ru", 2576764),
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

                Lead result = new() { responsible_user_id = line.Item6 };                                               //Создаём экземпляр сделки для передачи в амо

                try { SaveLead(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }

                result = new() { name = "Новая сделка", pipeline_id = 3198184, status_id = 32532880 };                  //Переводим сделку в основную воронку. Если переводить и менять ответственного одновременно, то срабатывает триггер в воронке, что может повлиять на запущенные процессы

                try { SaveLead(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }
                AddNote("Processing finished.");
            }
        }
        #endregion
        #endregion

        #region Processor
        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_leadNumber.ToString());
                return;
            }
            try
            {
                if (lead is null)
                {
                    _processQueue.Remove(_leadNumber.ToString());
                    _log.Add($"Error: No lead returned from amoCRM: {_leadNumber}");
                    return;
                }
                if (lead.pipeline_id == 2231320 ||                                                                          //Если сделка в корп. воронке
                    lead.pipeline_id == 3198184)                                                                            //Если сделка в основной воронке
                {
                    FormName();
                    PhaseOne();
                    AddNote("Phase 1 finished.");

                    await Task.Run(() => CallResultWaiter());

                    UpdateLeadFromAmo();                                                                                    //Обновляем информацию о сделке, если она изменилась за время ожидания

                    if (lead is null)
                    {
                        _processQueue.Remove(_leadNumber.ToString());
                        _log.Add($"Error: No lead returned from amoCRM: {_leadNumber}");
                        return;
                    }

                    PhaseTwo();
                    AddNote("Phase 2 finished.");
                }
                else
                {
                    SocialNetworks();
                }
                _processQueue.Remove(_leadNumber.ToString());
                _log.Add($"Success: Lead {_leadNumber}");
            }
            catch (Exception e) 
            {
                _processQueue.Remove(_leadNumber.ToString());
                _log.Add($"Error: Unable to process lead {_leadNumber}: {e.Message}");
                throw;
            }
        }
        #endregion
    }
}