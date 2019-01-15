using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glastroika.API
{
    public static class Helper
    {
        public static void DownloadJsonFromUsername(string Username, string FileLocation)
        {
            JObject ig = JObject.Parse(Instagram.GetJsonFromIG(string.Format("https://www.instagram.com/{0}/", Username)));
            File.WriteAllText(FileLocation, JsonConvert.SerializeObject(ig, Formatting.Indented));
        }

        public static void DownloadJsonFromShortcode(string Shortcode, string FileLocation)
        {
            JObject ig = JObject.Parse(Instagram.GetJsonFromIG(string.Format("https://www.instagram.com/p/{0}/", Shortcode)));
            File.WriteAllText(FileLocation, JsonConvert.SerializeObject(ig, Formatting.Indented));
        }

        public static string SerializedGetUser(string Username)
        {
            return JsonConvert.SerializeObject(Instagram.GetUser(Username), Formatting.Indented);
        }

        public static string SerializedGetMedia(string Shortcode)
        {
            return JsonConvert.SerializeObject(Instagram.GetMedia(Shortcode), Formatting.Indented);
        }
    }
}
