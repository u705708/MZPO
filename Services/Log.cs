using System;
using System.IO;
using System.Threading;

namespace MZPO.Services
{
    public class Log
    {
        private readonly SemaphoreSlim _ss;

        public Log()
        {
            _ss = _ss = new(1, 1);
        }

        public void Add(string message)
        {
            _ss.Wait();

            using StreamWriter sw = new("log.txt", true, System.Text.Encoding.Default);
            sw.Write("{0} : ", DateTime.Now.ToString());
            sw.WriteLine(message);

            _ss.Release();
        }

        public string GetLog()
        {
            using StreamReader sr = new("log.txt");
            return sr.ReadToEnd();
        }
    }
}