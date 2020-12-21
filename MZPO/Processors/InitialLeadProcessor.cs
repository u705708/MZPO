using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Processors
{
    public class InitialLeadProcessor : AbstractProcessor                                                               //Процессор осуществляет первоначальную обработку сделок
    {
        #region Definition
        public InitialLeadProcessor(int leadNumber, AmoAccount acc, TaskList processQueue, CancellationToken token) 
            : base(leadNumber, acc, processQueue, token) { }

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
            "mirk.vrach.kosmetolog"
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
        private IEnumerable<Note> FormName()                                                                            //Проверяем значение поля id_form и добавляем комментарий к сделке
        {
            try
            {
                string fieldValue = GetFieldValue(644511);
                if (fieldValue != null)
                {
                    switch (fieldValue)
                    {
                        case "contactForm_tel":
                            return _leadRepo.AddNotes(new Note() { entity_id = lead.id, note_type = "common", parameters = new Note.Params() { text = "Форма: \"Заказать обратный звонок\"" } });
                        case "contactForm":
                            return _leadRepo.AddNotes(new Note() { entity_id = lead.id, note_type = "common", parameters = new Note.Params() { text = "Форма: \"Записаться\"" } });
                    }
                }
                return null;
            }
            catch (Exception e) { throw new Exception($"FormName: {e.Message}"); }
        }
        #endregion

        #region Phase1
        private void PhaseOne()
        {
            #region Тег по сайту
            var site = GetFieldValue(639081);
            var applicationType = GetFieldValue(639075);

            if (site != null)                                                                                           //Если поле сайт не пустое
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
            else if (applicationType != null)                                                                           //Если тип обращения не пустой
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
                    if (city != null)                                                                                   //Если оно не пустое
                        SetFieldValue(639087, _acc.GetCity(city));                                                      //Подставляем значение из базы городов
                }
                catch (Exception e)
                {
                    Log.Add($"Warning message: {e.Message}; Сделка: {lead.id}");
                }
            }
            #endregion

            Lead result = new Lead() { id = lead.id, name = "Новая сделка", _embedded = new Lead.Embedded() };           //Создаём и наполняем экземпляр сделки для отправки в амо
            result.custom_fields_values = custom_fields_values;
            result._embedded.tags = tags;
            try { _leadRepo.Save(result); }
            catch (Exception e) { throw new Exception($"Phase 1: {e.Message}"); }
        }
        #endregion

        #region Ожидание телефонии
        private void CallResultWaiter()                                                                                 //Метод проверяет, была ли сделка из телефонии, если да, то ждёт полчаса
        {
            var source = GetFieldValue(639085);
            if (source == null) return;
            if (source.Contains("Прямой") || source.Contains("Коллтрекинг"))
            {
                for (int i = 1; i <= 30; i++)
                {
                    if (_token.IsCancellationRequested) return;                                                         //Если получили токен, то завершаем раньше, проверяем раз в минуту
                    Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
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
            if (source != null)
            {
                if (tags.Any(x => x.name == "Коллтрекинг") || source.Contains("Коллтрекинг"))                           //Если сделка из коллтрекинга
                {
                    SetFieldValue(639075, "Коллтрекинг");                                                               //Тип обращения
                    SetFieldValue(639085, GetFieldValue(645583));                                                       //Источник(Маркер), roistat_marker
                    SetTag("Коллтрекинг");
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
            if (tags.Count() == 0)                                                                                      //Если тегов нет
            {
                SetTag("Ручная");
                SetFieldValue(639081, "Сделка созданная вручную");                                                      //Сайт
            }
            #endregion

            #region Результат звонка
            var applicationType = GetFieldValue(639075);                                                                //Тип обращения
            if (GetFieldValue(644675) == null)                                                                          //Результат звонка
            {
                if ((applicationType != null) && (applicationType.Contains("аявк")))
                {
                    SetFieldValue(644675, "Заявка с сайта");
                }
                else if ((applicationType != null) && (applicationType.Contains("Jivosite")))
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

            Lead result = new Lead() { id = lead.id, _embedded = new Lead.Embedded() };
            result.custom_fields_values = custom_fields_values;
            result._embedded.tags = tags;

            #region Взять в работу
            if ((GetFieldValue(644675) == "Принят") &&                                                                  //Результат звонка
                (lead.pipeline_id == 3198184) &&                                                                        //Продажи(Розница)
                (lead.status_id == 32532880))                                                                           //Получен новый лид
            {
                result.pipeline_id = 3198184;                                                                           //Продажа(Розница)
                result.status_id = 32532883;                                                                            //Взят в работу
            }
            #endregion

            try { _leadRepo.Save(result); }
            catch (Exception e) { throw new Exception($"Phase 2: {e.Message}"); }
        }
        #endregion

        #region Соцсети
        private void SocialNetworks()
        {
            var pipelines = new List<(int, List<string>, string, string, string, int)>(){
                    (3326584, new List<string>(){"mzpo-s.ru", "insta"}, "mzpo-s.ru", "mzpo-s.ru_instagram", "instagram", 2576764),
                    (3223441, new List<string>(){"skillbank.ru", "insta"}, "skillbank.ru", "skillbank.su-instagram", "instagram", 2576764),
                    (3308590, new List<string>(){"mirk.msk.ru", "insta"}, "mirk.msk.ru", "mirk.msk.ru-instagram", "instagram", 6158035),
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
                    (3812137, new List<string>(){"mzpokurs.com", "facebook"}, "mzpokurs.com", "mzpokurs.com-facebook", "facebook", 2576764)
                };                            //Список воронок соцсетей и соответствующих значений полей

            if (pipelines.Any(x => x.Item1 == lead.pipeline_id))                                                        //Если сделка в одной из воронок из списка
            {
                var line = pipelines.Where(x => x.Item1 == lead.pipeline_id).First();                                   //получаем значения полей для этой воронки

                foreach (var tag in line.Item2)                                                                         //Задаём теги
                    SetTag(tag);

                SetFieldValue(639081, line.Item3);                                                                      //Сайт
                SetFieldValue(639085, line.Item4);                                                                      //Источник(Маркер)
                SetFieldValue(639075, line.Item5);                                                                      //Тип обращения
                SetFieldValue(644675, line.Item5);                                                                      //Результат звонка

                Lead result = new Lead() { id = lead.id, _embedded = new Lead.Embedded() };                             //Создаём экземпляр сделки для передачи в амо
                result.custom_fields_values = custom_fields_values;                                                     //Задаём значения полей
                result._embedded.tags = tags;                                                                           //Теги
                result.responsible_user_id = line.Item6;                                                                //Ответственный

                try { _leadRepo.Save(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }

                result = new Lead() { id = lead.id, name = "Новая сделка", _embedded = new Lead.Embedded() };           //Переводим сделку в основную воронку. Если переводить и менять ответственного одновременно, то срабатывает триггер в воронке, что может повлиять на запущенные процессы
                result.pipeline_id = 3198184;                                                                           //Продажа(Розница)
                result.status_id = 32532880;                                                                            //Получен новый лид

                try { _leadRepo.Save(result); }
                catch (Exception e) { throw new Exception($"SocialNetwork: {e.Message}"); }
                _leadRepo.AddNotes(lead.id, "Processing finished.");
            }
        }
        #endregion
        #endregion

        #region Processor
        public override async void Run()
        {
            if (_token.IsCancellationRequested) return;
            try
            {
                if (lead == null) throw new Exception("No lead returned from amoCRM");
                if (lead.pipeline_id == 3198184)                                                                            //Если сделка в основной воронке
                {
                    FormName();
                    PhaseOne();
                    _leadRepo.AddNotes(lead.id, "Phase 1 finished.");

                    await Task.Run(() => CallResultWaiter());

                    lead = _leadRepo.GetById(lead.id);                                                                      //Обновляем информацию о сделке, если она изменилась за время ожидания
                    tags = lead._embedded.tags;
                    custom_fields_values = new List<Lead.Custom_fields_value>();

                    PhaseTwo();
                    _leadRepo.AddNotes(lead.id, "Phase 2 finished.");
                }
                else
                {
                    SocialNetworks();
                }

                _processQueue.Remove(lead.id.ToString());
                Log.Add($"Success: Lead {lead.id}");
            }
            catch (Exception e) 
            {
                _processQueue.Remove(lead.id.ToString());
                Log.Add($"Error: Unable to process lead {lead.id}: {e.Message}");
            }
        }
        #endregion
    }
}