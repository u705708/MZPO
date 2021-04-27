using System;
using System.Collections.Generic;
using System.Linq;

namespace MZPO.Services
{
    public class RecentlyUpdatedEntityFilter
    {
        private readonly Dictionary<int, DateTime> _entities;
        private readonly int _waitTime;
        private readonly object _locker;
        private int _counter;

        public RecentlyUpdatedEntityFilter()
        {
            _entities = new();
            _locker = new();
            _waitTime = 30;
            _counter = 0;
        }

        private void ClearExpiredEntites()
        {
            lock (_locker)
            {
                foreach (var e in _entities)
                    if (e.Value < DateTime.UtcNow)
                        _entities.Remove(e.Key);
            }
        }

        public DateTime AddEntity(int id)
        {
            _counter++;

            if (_counter > 30)
                ClearExpiredEntites();

            lock (_locker)
            {
                if (_entities.ContainsKey(id))
                {
                    _entities[id] = DateTime.UtcNow.AddSeconds(_waitTime);
                    return _entities[id];
                }

                _entities.Add(id, DateTime.UtcNow.AddSeconds(_waitTime));
                return _entities[id];
            }
        }

        public bool CheckEntityIsValid(int id)
        {
            _counter++;

            if (_counter > 30)
                ClearExpiredEntites();

            lock (_locker)
            {
                if (_entities.ContainsKey(id))
                {
                    if (_entities[id] < DateTime.UtcNow)
                    {
                        _entities.Remove(id);
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        public List<(int, DateTime)> GetFilterEntries()
        {
            _counter++;

            if (_counter > 30)
                ClearExpiredEntites();

            var result = _entities.Select(x => (x.Key, x.Value)).ToList();


            return result;
        }
    }
}