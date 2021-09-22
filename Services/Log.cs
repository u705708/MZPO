using System;
using System.IO;

namespace MZPO.Services
{
    public class Log
    {
        private readonly object _locker;

        public Log()
        {
            _locker = new();
        }

        public void Add(string message)
        {
            lock (_locker)
            {
                using StreamWriter sw = new($@"logs\log_{DateTime.Today.Year}-{DateTime.Today.Month}.txt", true, System.Text.Encoding.Default);
                sw.Write("{0} : ", DateTime.Now.ToString());
                sw.WriteLine(message);
            }
        }

        public string GetLog()
        {
            using StreamReader sr = new($@"logs\log_{DateTime.Today.Year}-{DateTime.Today.Month}.txt");
            return sr.ReadToEnd();
        }
    }
}