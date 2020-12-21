using System;
using System.IO;

namespace MZPO
{
    public static class Log
    {
        public static void Add(string message)
        {
            using StreamWriter sw = new StreamWriter("log.txt", true, System.Text.Encoding.Default);
            sw.Write("{0} : ", DateTime.Now.ToString());
            sw.WriteLine(message);
        }

        public static string GetLog()
        {
            using StreamReader sr = new StreamReader("log.txt");
            return sr.ReadToEnd();
        }
    }
}
