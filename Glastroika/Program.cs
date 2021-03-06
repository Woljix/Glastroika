﻿using Glastroika.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Glastroika
{
    public class Program
    {
        static readonly string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
        static readonly string temp_name = "$put_name_here"; // The reason is has a character in the front is because "put_name_here" could be an actual username.

        static Thread bot_thread;
        static Thread heartbeat;

        static bool ShouldRun = true;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = (Exception)e.ExceptionObject;

                string exception = ex.ToString();

                if (e.IsTerminating)
                    Log.WriteLine("Fatal Unhandeled Exception: " + exception, LogType.Error);
                else
                    Log.WriteLine("Unhandeled Exception: " + exception, LogType.Error);
            };

            Console.CancelKeyPress += delegate
            {
                ShouldRun = false;              
            };

            Console.Title = "Glastroika Alpha";
            Console.CursorVisible = false;

            if (!File.Exists(SettingsFile))
            {
                // Create template.
                Settings.CurrentSettings = new Settings()
                {
                    Target = new List<string>(new string[] { temp_name }),
                };

                Settings.Save(SettingsFile);

                Console.WriteLine("TEMPLATE FILE CREATED\nPlease edit before restarting!\nPress ENTER to exit!");
                Console.ReadLine();
                Environment.Exit(0);
            }
    
            string old_hash = string.Empty;

            new Thread(() =>
            {
                while (ShouldRun)
                {
                    string base64String = Convert.ToBase64String(GetMD5(SettingsFile));
                    if (base64String != old_hash)
                    {
                        old_hash = base64String;
                        Debug.WriteLine("New Hash: " + base64String);
                        StartBots();
                    }
                    Thread.Sleep(5000);
                }
            }).Start();
        }

        public static void StartBots()
        {
            #region Settings Loader

            if (File.Exists(SettingsFile))
            {
                Settings.Load(SettingsFile);

                if (Settings.CurrentSettings.Target.Contains(temp_name))
                {
                    Console.WriteLine("Template name detected!");
                    Console.WriteLine("Please edit the settings file before restarting!");
                    Console.WriteLine("Please ENTER to exit!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            else
            {
                // This should not be displayed ever. Because it should be created right beforet this function executes.
                Console.WriteLine("Settings file not found!");
                Console.ReadLine();
                Environment.Exit(0);
            }

            Settings.Save(SettingsFile);

            #endregion

            Echo("Glastroika Started!", null);

            WebClient client = new WebClient();

            bot_thread = new Thread(() => 
            {
                while (bot_thread == Thread.CurrentThread)
                {
                    try
                    {
                        foreach (string target in Settings.CurrentSettings.Target)
                        {
                            User user = Instagram.GetUser(target) ?? null;

                            if (user != null)
                            {
                                string userDir = Path.Combine(Settings.CurrentSettings.DownloadFolder, user.Username);
                                string profilePicFolder = Path.Combine(userDir, "ProfilePicture");

                                if (!Directory.Exists(userDir))
                                    Directory.CreateDirectory(userDir);

                                if (!Directory.Exists(profilePicFolder))
                                    Directory.CreateDirectory(profilePicFolder);

                                string ppFileName = GetFileNameFromUrl(user.ProfilePicture);

                                string ppdl = Path.Combine(profilePicFolder, ppFileName);

                                if (!File.Exists(ppdl))
                                {
                                    Echo(string.Format("Downloading Profile Picture: '{0}'", ppFileName), user);

                                    client.DownloadFile(user.ProfilePicture, ppdl);
                                }

                                foreach (Media media in user.Media)
                                {
                                    if (media != null)
                                    {
                                        foreach (string url in media.URL)
                                        {
                                            string fileName = GetFileNameFromUrl(url);

                                            string dl = Path.Combine(userDir, fileName);

                                            if (!File.Exists(dl))
                                            {
                                                Echo(string.Format("Downloading: '{0}'", fileName), user);

                                                client.DownloadFile(url, dl);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(ex.ToString(), LogType.Error);
                    }
                    
                    Thread.Sleep(TimeSpan.FromMinutes(Settings.CurrentSettings.Interval));
                }             
            });

            bot_thread.Start();

            // Used to write to a file every minute to monitor if the program is still running.

            if (heartbeat == null)
            {
                heartbeat = new Thread(() =>
                {
                    while (heartbeat.ThreadState == System.Threading.ThreadState.Running)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(Path.Combine(Settings.CurrentSettings.LogFolder, "heartbeat.txt"), FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fs.SetLength(0);

                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    sw.WriteLine(DateTime.Now.ToString());

                                    sw.Close();
                                }

                                fs.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine("Heartbeat: " + ex.ToString(), LogType.Error);
                        }

                        Thread.Sleep(TimeSpan.FromMinutes(1));
                    }
                });
            }
            
            if (Settings.CurrentSettings.Heartbeat)
                heartbeat.Start();
        }

        public static void Echo(string Text, User user)
        {
            string name = (user == null ? "NULL" : user.Username); // If user is null, set the name as "NULL". If it is not null set it as the username

            string EText = string.Format("[{0}][{1}] {2}", DateTime.Now.ToString(), name, Text);

            Console.WriteLine(EText);
            Log.Echo(EText);
            //Log.WriteLine(EText, LogType.Raw);
        }

        private static readonly Uri SomeBaseUri = new Uri("http://canbeanything");
        public static string GetFileNameFromUrl(string url)
        {
            Uri result;
            if (!Uri.TryCreate(url, UriKind.Absolute, out result))
                result = new Uri(SomeBaseUri, url);
            return Path.GetFileName(result.LocalPath);
        }
        public static byte[] GetMD5(string fileLocation)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(fileLocation))
                    return md5.ComputeHash((Stream)fileStream);
            }
        }
    }
}