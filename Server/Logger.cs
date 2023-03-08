using System;
using System.IO;

namespace Logger
{
    public abstract class LogBase
    {
        public abstract void Log(string Messsage);
    }

    public class Logger : LogBase
    {
        private String CurrentDirectory { get; set; }
        private String FileName { get; set; }
        private String FilePath { get; set; }


        public Logger()
        {
            this.CurrentDirectory = Directory.GetCurrentDirectory();
            this.FileName = "Log.txt";
            this.FilePath = this.CurrentDirectory + "/" + this.FileName;
        }

        public override void Log(string Messsage)
        {
            System.Console.WriteLine($"Logged : {Messsage}");

            using (System.IO.StreamWriter w = System.IO.File.AppendText(this.FilePath))
            {
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}: {Messsage}");
            }
        }
    }
}