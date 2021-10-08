using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class Uber
    {
        private class User
        {
            public int id;
            public Action<UberLead> sendLead;
            public bool requestedLead;
            public DateTime validity;

            public void SendLead(UberLead lead)
            {
                sendLead?.Invoke(lead);
            }
        }

        private readonly List<User> _activeUsers;
        private readonly List<Task> _tasks;
        private readonly Queue<UberLead> _leads;
        private readonly object _locker;
        private readonly TimeSpan _timeOut = TimeSpan.FromSeconds(90);

        public Uber()
        {
            _leads = new();
            _tasks = new();
            _activeUsers = new();
            _locker = new();
        }

        private Task waitToDistribute;
        private Task<UberLead> receiveAcception;
        private Task<UberLead> receiveDecline;
        private Task waitForUpdateTask;
        private bool distributing = false;

        private User GetUser(int id)
        {
            if (_activeUsers.Any(x => x.id == id))
            {
                return _activeUsers.First(x => x.id == id);
            }

            return null;
        }

        private IEnumerable<User> GetNextUser()
        {
            ClearExpiredUsers();
            var readyUsers = _activeUsers.Where(x => x.requestedLead).ToList();
            return readyUsers;
        }

        private void ClearExpiredUsers()
        {
            DateTime dt = DateTime.Now;
            List<User> users = new(_activeUsers);
            lock (_locker)
            {
                foreach (var u in users)
                    if (dt > u.validity)
                        _activeUsers.Remove(u);
            }
        }

        private async Task Distribute()
        {
            while (distributing)
            {
                
                if (!_leads.TryDequeue(out var distributedLead))
                { 
                    distributing = false; 
                    break; 
                }
                
                bool distributed = false;

                while (!distributed)
                {
                    foreach (var u in GetNextUser())
                    {
                        if (distributed) break;
                        _tasks.Add(waitToDistribute = Task.Delay(TimeSpan.FromSeconds(20)));
                        _tasks.Add(waitForUpdateTask = Task.Delay(TimeSpan.FromSeconds(1)));
                        bool working = true;

                        u.SendLead(distributedLead);

                        while (working)
                        {
                            await Task.WhenAny(_tasks);

                            //Отработал таймер 20 сек, время распеделения вышло
                            if (waitToDistribute.IsCompleted)
                            {
                                _tasks.Remove(waitToDistribute);
                                _tasks.Remove(waitForUpdateTask);
                                working = false;
                                continue;
                            }

                            //Сделка принята потребителем
                            if (_tasks.Contains(receiveAcception) &&
                                receiveAcception.IsCompleted &&
                                receiveAcception.Result == distributedLead)
                            {
                                _tasks.Remove(receiveAcception);
                                _tasks.Remove(waitToDistribute);
                                _tasks.Remove(waitForUpdateTask);
                                distributed = true;
                                working = false;
                                continue;
                            }

                            //Сделка отклонена потребителем
                            if (_tasks.Contains(receiveDecline) &&
                                receiveDecline.IsCompleted &&
                                receiveDecline.Result == distributedLead)
                            {
                                _tasks.Remove(receiveDecline);
                                _tasks.Remove(waitToDistribute);
                                _tasks.Remove(waitForUpdateTask);
                                working = false;
                                continue;
                            }

                            //из за таймера по 2 запуска одновременно
                            //Отработал таймер 1 сек
                            if (waitForUpdateTask.IsCompleted)
                            {
                                _tasks.Remove(waitForUpdateTask);
                                _tasks.Add(waitForUpdateTask = Task.Delay(TimeSpan.FromSeconds(1)));
                            }
                        }
                    }

                    if (!distributed) await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        public void RequestLead(int id, Action<UberLead> eventFunc)
        {
            RefreshUser(id, eventFunc, true);
        }

        public void RefreshUser(int id, Action<UberLead> eventFunc = null, bool requestedLead = false)
        {
            if (_activeUsers.Any(x => x.id == id))
            {
                User user = _activeUsers.First(x => x.id == id);
                user.validity = DateTime.Now.Add(_timeOut);
                if (eventFunc is not null)
                    user.sendLead = eventFunc;
                if (requestedLead)
                    user.requestedLead = requestedLead;
                return;
            }

            lock (_locker) 
            { 
                _activeUsers.Add(new() { 
                    id = id, 
                    validity = DateTime.Now.Add(_timeOut),
                    sendLead = eventFunc,
                    requestedLead = requestedLead
                }); 
            }
        }

        public async Task<bool> AcceptLead(UberLead lead, int id)
        {
            User user = GetUser(id);
            user.sendLead = null;
            user.requestedLead = false;

            _tasks.Add(receiveAcception = Task.FromResult(lead));

            return true;
        }

        public void DeclineLead(UberLead lead, int id)
        {
            User user = GetUser(id);
            user.sendLead = null;
            user.requestedLead = false;

            _tasks.Add(receiveDecline = Task.FromResult(lead));
        }

        public void AddToQueue(UberLead lead)
        {
            if (_leads.Count > 12) return; //Для тестирования, на бою убрать
            lock (_locker)
            {
                if (!_leads.Any(x => x.leadId == lead.leadId))
                    _leads.Enqueue(lead);
            }

            if (!distributing)
            {
                distributing = true;
                Task.Run(() => Distribute());
            }
        }
    }
}