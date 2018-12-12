using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Glastroika
{
    public static class Log
    {
        private static FileStream fs;

        public static void WriteLine(string Text, LogType logType = LogType.Info)
        {
            if (fs == null)
            { 
                string LogFolder = Directory.CreateDirectory(Settings.CurrentSettings.LogFolder).FullName;

                fs = new FileStream(Path.Combine(LogFolder, Path.GetFileName(Settings.CurrentSettings.LogFileName)), FileMode.Append, FileAccess.Write);
            }

            string _type = string.Empty;

            // Yes, i know i can just use .ToString() on the enum, but what is the fun in that?
            switch (logType)
            {
                default:
                case LogType.Info: _type = "INFO"; break;
                case LogType.Warning: _type = "WARNING"; break;
                case LogType.Error: _type = "ERROR"; break;
                case LogType.Raw: /* BLANK */ break;
            }

            string LogText = string.Empty;

            if (logType != LogType.Raw)
            {
                LogText = string.Format("[{0}][{1}] {2}", DateTime.Now.ToString(), _type, Text);
            }
            else
            {
                LogText = Text;
            }

            Debug.WriteLine(LogText);
            StreamWriter streamWriter = new StreamWriter((Stream)fs);
            streamWriter.WriteLine(LogText);
            streamWriter.Close();
        }

        public static void Echo(string Text)
        {
            string LogFolder = Directory.CreateDirectory(Settings.CurrentSettings.LogFolder).FullName;

            FileStream fileStream = new FileStream(Path.Combine(LogFolder, DateTime.Now.ToString("dd-MM-yyyy") + ".txt"), FileMode.Append, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter((Stream)fileStream);
            streamWriter.WriteLine(Text);
            streamWriter.Close();
            fileStream.Close();
        }

        public static void Dispose()
        {
            if (fs == null) return;

            fs.Close();
            fs.Dispose();
            fs = null;
        }
    }

    public enum LogType
    {
        Info,
        Warning,
        Error,
        Raw,
    }
}
