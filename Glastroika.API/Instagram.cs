using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Glastroika.API
{
    public static class Instagram
    {
        public static User GetUser(string Username)
        {
            try
            {
                User user = new User();

                string json = GetJsonFromIG(string.Format("https://www.instagram.com/{0}/", Username)) ?? null;
               
                if (string.IsNullOrEmpty(json)) return null;

                JObject ig = JObject.Parse(json);

                user.Username = (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["username"];

                // user.ProfilePicture = GetProfilePicture((string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["id"]);
                user.ProfilePicture = (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["profile_pic_url_hd"];
                user.FullName = (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["full_name"];
                user.Biography = (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["biography"];

                user.Followers = (int)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["edge_followed_by"]["count"];
                user.Following = (int)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["edge_follow"]["count"];

                user.IsPrivate = (bool)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["is_private"];
                user.IsVerified = (bool)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["is_verified"];

                JArray nodes = (JArray)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"];

                if (!user.IsPrivate) // Obviously you can't get media from an account if it is private.
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        Media media = new Media();

                        media.Username = (string)nodes[i]["node"]["owner"]["username"];

                        string _type = (string)nodes[i]["node"]["__typename"];

                        switch (_type)
                        {
                            default:
                            case "GraphImage": media.Type = MediaType.Image; break;
                            case "GraphVideo": media.Type = MediaType.Video; break;
                            case "GraphSidecar": media.Type = MediaType.Collage; break;
                        }

                        JArray caption = (JArray)nodes[i]["node"]["edge_media_to_caption"]["edges"];

                        if (caption.Count != 0)
                            media.Caption = (string)caption[0]["node"]["text"];
                        else
                            media.Caption = string.Empty;

                        media.Shortcode = (string)nodes[i]["node"]["shortcode"];
                        media.Timestamp = (int)nodes[i]["node"]["taken_at_timestamp"];

                        switch ((string)nodes[i]["node"]["__typename"])
                        {
                            default:
                            case "GraphImage":
                                media.URL.Add((string)nodes[i]["node"]["display_url"]);
                                break;

                            case "GraphVideo":
                            case "GraphSidecar":
                                media.URL.AddRange(GetMedia(media.Shortcode).URL);
                                break;
                        }

                        user.Media.Add(media);
                    }
                }
                
                return user;
            }
            catch (Exception ex)
            {
                // Don't to anything for now
                return null;
            }
        }

        public static Media GetMedia(string Shortcode)
        {
            try
            {
                Media media = new Media();

                string json = GetJsonFromIG(string.Format("https://www.instagram.com/p/{0}/", Shortcode));

                if (string.IsNullOrEmpty(json))
                    return null;

                JObject ig = JObject.Parse(json);

                media.Username = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["owner"]["username"];

                media.Shortcode = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["shortcode"];
                media.Timestamp = (int)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["taken_at_timestamp"];

                string _type = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["__typename"];

                switch (_type)
                {
                    default:
                    case "GraphImage": media.Type = MediaType.Image; break;
                    case "GraphVideo": media.Type = MediaType.Video; break;
                    case "GraphSidecar": media.Type = MediaType.Collage; break;
                }

                JArray caption = (JArray)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_media_to_caption"]["edges"];

                if (caption.Count != 0)
                    media.Caption = (string)caption[0]["node"]["text"];
                else
                    media.Caption = string.Empty;

                switch ((string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["__typename"])
                {
                    default:
                    case "GraphImage":
                        media.URL.Add((string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["display_url"]);
                        break;

                    case "GraphVideo":
                        media.URL.Add((string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["video_url"]);

                        break;

                    case "GraphSidecar":
                        JArray nodes = (JArray)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_sidecar_to_children"]["edges"];

                        for (int i = 0; i < nodes.Count; i++)
                        {
                            media.URL.Add((string)nodes[i]["node"]["display_url"]);
                        }

                        break;
                }

                return media;
            }
            catch (Exception ex)
            {
                return null;
            }           
        }

        [Obsolete("Instagram depreciated the API")]
        public static string GetProfilePicture(string UserID)
        {
            string json = string.Empty;

            try
            {
                json = new WebClient().DownloadString(string.Format("https://i.instagram.com/api/v1/users/{0}/info/", UserID));

                if (!string.IsNullOrEmpty(json))
                {
                    JObject obj = JObject.Parse(json);
                    return (string) obj["user"]["hd_profile_pic_url_info"]["url"];
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        internal static string GetJsonFromIG(string URL, bool A1 = false)
        {
            string html = string.Empty;
            try
            {
                html = new WebClient().DownloadString(URL);
            }
            catch (WebException ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
            if (string.IsNullOrEmpty(html))
                return (string)null;
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return ExtractJsonObject(htmlDocument.DocumentNode.SelectSingleNode("//script[contains(text(), 'window._sharedData = ')]").InnerText);
        }
        internal static string ExtractJsonObject(string mixedString)
        {
            for (var i = mixedString.IndexOf('{'); i > -1; i = mixedString.IndexOf('{', i + 1))
            {
                for (var j = mixedString.LastIndexOf('}'); j > -1; j = mixedString.LastIndexOf("}", j - 1))
                {
                    var jsonProbe = mixedString.Substring(i, j - i + 1);
                    try
                    {
                        return jsonProbe;
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }
    }
}
