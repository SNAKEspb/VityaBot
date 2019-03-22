using System;
using System.IO;
using System.Text;

namespace LogUtil
{
    public class Log
    {
        private const string splitter = "==================================================================================================";
        private const string format = "{0}\r\nTime: {1}\r\n\r\n{2}\r\n\r\n";

        private static string path = AppDomain.CurrentDomain.BaseDirectory + "\\log" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";


        public static void info(object obj)
        {
            info(obj.ToString());
        }

        public static void info(string text, params object[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(text, args);
            info(stringBuilder.ToString());
        }

        public static void info(string text)
        {
            Console.WriteLine(text);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(format, splitter, System.DateTime.Now.ToString(), text);
            File.AppendAllText(path, stringBuilder.ToString());
        }

        public async static void infoAsync(object obj)
        {
            infoAsync(obj.ToString());
        }

        public async static void infoAsync(string text, params object[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(text, args);
            infoAsync(stringBuilder.ToString());
        }

        public async static void infoAsync(string text)
        {
            info(text);
            // Console.WriteLine(text);
            // StringBuilder stringBuilder = new StringBuilder();
            // stringBuilder.AppendFormat(format, splitter, System.DateTime.Now.ToString(), text);
            //await File.AppendAllTextAsync(path, stringBuilder.ToString());
        }
    }
}