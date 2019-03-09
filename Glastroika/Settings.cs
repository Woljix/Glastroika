using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glastroika
{
    public class Settings
    {
        public static Settings CurrentSettings { get; set; } = new Settings();

        public static void Save(string Filename)
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented));
        }

        public static void Load(string Filename)
        {
            CurrentSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Filename));
        }

        public Settings() { }

        public string DownloadFolder = "Downloads";
        public string LogFolder = "Logs";
        public string LogFileName = "Log.txt";
        public double Interval = 2.5; // Minutes
        public bool Heartbeat = false;
        public List<string> Target = new List<string>();
    }
}
