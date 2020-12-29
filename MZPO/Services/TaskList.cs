using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class TaskList
    {
        public class TaskEntry
        {
            public string taskId;
            public bool cancellationRequested;
            public string accountId;
            public string taskName;
            public DateTime timeStarted;
        }
        
        private List<(Task, CancellationTokenSource, string, string, string, DateTime)> _taskList;

        public TaskList()
        {
            _taskList = new List<(Task, CancellationTokenSource, string, string, string, DateTime)>();
        }
        
        public void Add(Task task, CancellationTokenSource tokenSource, string taskId, string accountId, string taskName)
        {
            _taskList.Add((task, tokenSource, taskId, accountId, taskName, DateTime.Now));
        }

        public void Remove(string id)
        {
            if (_taskList.Any())
            {
                var line = _taskList.Where(x => x.Item3 == id).FirstOrDefault();
                _taskList.Remove(line);
                GC.Collect();
            }
        }

        public List<TaskEntry> GetList()
        {
            var result = new List<TaskEntry>();
            foreach (var l in _taskList)
            {
                result.Add(new TaskEntry() 
                {
                    taskId = l.Item3,
                    cancellationRequested = l.Item2.IsCancellationRequested,
                    accountId = l.Item4,
                    taskName = l.Item5,
                    timeStarted = l.Item6
                });
            }
            return result;
        }

        public void Stop(string id)
        {
            if (_taskList.Any())
            {
                var line = _taskList.Where(x => x.Item3 == id).FirstOrDefault();
                if (_taskList.Contains(line)) line.Item2.Cancel();
            }
        }
    }
}
