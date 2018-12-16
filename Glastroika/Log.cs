using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Glastroika
{
    public static class Log
    {
        public static void WriteLine(string Text, LogType logType = LogType.Info)
        {
            string LogFolder = Directory.CreateDirectory(Settings.CurrentSettings.LogFolder).FullName;

            using (FileStream fs = new FileStream(Path.Combine(LogFolder, Path.GetFileName(Settings.CurrentSettings.LogFileName)), FileMode.Append, FileAccess.Write))
            {
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

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(LogText);
                    sw.Close();
                }

                fs.Close();
            }   
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
    }

    public enum LogType
    {
        Info,
        Warning,
        Error,
        Raw,
    }
}
