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
            public string timeStarted;
        }
        
        private readonly List<(Task, CancellationTokenSource, string, string, string, DateTime)> _taskList;

        public TaskList()
        {
            _taskList = new List<(Task, CancellationTokenSource, string, string, string, DateTime)>();
        }
        
        public void AddTask(Task task, CancellationTokenSource tokenSource, string taskId, string accountId, string taskName)
        {
            lock (_taskList)
            {
                _taskList.Add((task, tokenSource, taskId, accountId, taskName, DateTime.Now));
            } 
        }

        public void AddSubTask(string oldTaskId, string subTaskId, string subTaskName)
        {
            lock (_taskList)
            { 
                if (_taskList.Any(x => x.Item3 == oldTaskId))
                {
                    var oldTask = _taskList.First(x => x.Item3 == oldTaskId);
                    var newTask = (oldTask.Item1, oldTask.Item2, subTaskId, oldTask.Item4, subTaskName, DateTime.Now);
                    _taskList.Add(newTask);
                }
            }
        }

        public void UpdateTaskName(string id, string taskName)
        {
            lock (_taskList)
            { 
                if (_taskList.Any(x => x.Item3 == id))
                {
                    var oldTask = _taskList.First(x => x.Item3 == id);
                    var newTask = (oldTask.Item1, oldTask.Item2, oldTask.Item3, oldTask.Item4, taskName, oldTask.Item6);
                    _taskList.Remove(oldTask);
                    _taskList.Add(newTask);
                }
            }
        }

        public void Remove(string id)
        {
            lock (_taskList)
            { 
                if (_taskList.Any(x => x.Item3 == id))
                {
                    var line = _taskList.First(x => x.Item3 == id);
                    line.Item2.Dispose();
                    _taskList.Remove(line);
                }
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
                    timeStarted = $"{l.Item6.ToShortDateString()} {l.Item6.ToShortTimeString()}"
                });
            }
            return result;
        }

        public void Stop(string id)
        {
            if (_taskList.Any(x => x.Item3 == id))
            {
                var line = _taskList.First(x => x.Item3 == id);
                if (_taskList.Contains(line)) line.Item2.Cancel();
            }
        }

        public bool CheckIfRunning(string id) => _taskList.Any(x => x.Item3 == id);
    }
}