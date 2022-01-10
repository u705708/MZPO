using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class RetailRespProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly int _entityNumber;
        private readonly Log _log;
        private readonly Type _type;
        private readonly int _oldResp;
        private readonly int _modResp;

        public RetailRespProcessor(Amo amo, ProcessQueue processQueue, CancellationToken token, int entityNumber, Log log, Type type, int oldResp, int modResp)
        {
            _leadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(28395871).GetRepo<Contact>();
            _processQueue = processQueue;
            _token = token;
            _entityNumber = entityNumber;
            _log = log;
            _type = type;
            _oldResp = oldResp;
            _modResp = modResp;
        }

        private static readonly int[] admins = new[]
        {
            0,          //Робот
            2576764,    //Администратор
            7149397,    //Администратор(доступ)
            2375107,    //Кристина Гребенникова
            2375152     //Карен Оганисян
        };

        private static readonly int[] seniorsA = new[]
        {
            2375107     //Кристина Гребенникова
        };

        private static readonly int[] managersA = new[]
        {
            2375143,    //Екатерина Белоусова
            6158035,    //Анастасия Матюк
            7448173,    //Инна Апостол
            3835801,    //Наталья Кубышина
            7744360,    //Володина Мария
            3813670,    //Александра Федорова
        };

        private static readonly int[] seniorsB = new[]
        {
            2375152     //Карен Оганисян
        };

        private static readonly int[] managersB = new[]
        {
            6102562,    //Валерия Лукьянова
            6929800,    //Саида Исмаилова
            7358368,    //Лидия Ковш
            7771945,    //Сиренко Оксана
        };

        private static bool IsChangeAllowed(int oldResp, int modResp)
        {
            if (oldResp == modResp) return true;                //Если менеджер меняет с себя на другого

            if (admins.Contains(modResp)) return true;          //Если меняет Админстратор

            if (seniorsA.Contains(modResp) &&                   //Если меняет руководитель группы А
                managersA.Contains(oldResp))                    //С менеджера группы А
                return true;

            if (seniorsB.Contains(modResp) &&                   //Если меняет руководитель группы B
                managersB.Contains(oldResp))                    //С менеджера группы B
                return true;

            return false;
        }

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"setResp-{_entityNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                if (IsChangeAllowed(_oldResp, _modResp))
                {
                    _processQueue.Remove($"setResp-{_entityNumber}");
                    return Task.CompletedTask;
                }

                if (_type == typeof(Lead))
                {
                    Lead lead = new()
                    {
                        id = _entityNumber,
                        responsible_user_id = _oldResp
                    };

                    _leadRepo.Save(lead);
                }

                if (_type == typeof(Contact))
                {
                    Contact contact = new()
                    {
                        id = _entityNumber,
                        responsible_user_id = _oldResp
                    };

                    _contRepo.Save(contact);
                }

                _processQueue.Remove($"setResp-{_entityNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"setResp-{_entityNumber}");
                _log.Add($"Не получилось поменять ответственного в сделке {_entityNumber}: {e.Message}.");
                return Task.FromException(e);
            }
        }
    }
}