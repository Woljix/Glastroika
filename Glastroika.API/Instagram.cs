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
        private static WebClient _web;
        private static WebClient client
        {
            get
            {
                if (_web == null)
                    _web = new WebClient();

                return _web;
            }
        }

        /// <summary>
        /// Gets most of the crucial data of a public Instagram account.
        /// </summary>
        /// <param name="Username">Username of the account that you want to get the date of.</param>
        //// <param name="SlimMode">Disabling SlimMode makes the program go through every single media to get it's information, which uses alot more time and bandwidth</param> // TODO
        /// <returns></returns>
        public static User GetUser(string Username)
        {
            try
            {
                User user = new User();

                string json = GetJsonFromIG(string.Format("https://www.instagram.com/{0}/", Username.Trim())) ?? null;
               
                if (string.IsNullOrEmpty(json)) return null;

                JObject ig = JObject.Parse(json);

                user.Username = (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["username"];

                 user.ProfilePicture = GetProfilePicture((long)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["id"]) ?? (string)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["profile_pic_url_hd"];
                //user.ProfilePicture = ;
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
                        media.Likes = (int)nodes[i]["node"]["edge_liked_by"]["count"];

                        media.Comments = null; // GetUser can't get the comments because that information isn't exposed on the main page. GetMedia can get tha information though.

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
                Debug.WriteLine(ex.ToString());
                // Don't to anything for now
                return null;
            }
        }

        /// <summary>
        /// Gets the most crucial data of a single (public) Instagram post.
        /// </summary>
        /// <param name="Shortcode">The 11 digit number that Instagram uses to differentiate their users posts.</param>
        /// <returns></returns>
        public static Media GetMedia(string Shortcode)
        {
            try
            {
                Media media = new Media();

                // TODO: Make it try multiple times, as instagram has a tendency to not work all the time.
                string json = GetJsonFromIG(string.Format("https://www.instagram.com/p/{0}/", Shortcode.Trim()));

                if (string.IsNullOrEmpty(json))
                    return null;

                JObject ig = JObject.Parse(json);

                media.Username = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["owner"]["username"];

                media.Shortcode = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["shortcode"];
                media.Timestamp = (int)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["taken_at_timestamp"];
                media.Likes = (int)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_media_preview_like"]["count"];

                string _type = (string)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["__typename"];

                switch (_type)
                {
                    default:
                    case "GraphImage": media.Type = MediaType.Image; break;
                    case "GraphVideo": media.Type = MediaType.Video; break;
                    case "GraphSidecar": media.Type = MediaType.Collage; break;
                }

                JArray caption = (JArray)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_media_to_caption"]["edges"] ?? null;

                if (caption.Count != 0 && caption != null)
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
                        // This will be subject to change.
                        JArray nodes = (JArray)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_sidecar_to_children"]["edges"];

                        for (int i = 0; i < nodes.Count; i++)
                        {
                            switch ((string)nodes[i]["node"]["__typename"])
                            {
                                default:
                                case "GraphImage":
                                    media.URL.Add((string)nodes[i]["node"]["display_url"]);
                                    break;

                                case "GraphVideo":
                                    media.URL.Add((string)nodes[i]["node"]["video_url"]);
                                    break;
                            } 
                        }

                        break;
                }

                //JArray comments = (JArray)ig["entry_data"]["PostPage"][0]["graphql"]["shortcode_media"]["edge_media_to_parent_comment"]["edges"] ?? null;

                //if (comments != null)
                //{
                //    for (int i = 0; i < comments.Count; i++)
                //    {
                //        media.Comments.Add(new Comment()
                //        {
                //            Owner = (string)comments[i]["node"]["owner"]["username"],
                //            ProfilePicture = (string)comments[i]["node"]["owner"]["profile_pic_url"],

                //            Text = (string)comments[i]["node"]["text"],
                //            Timestamp = (int)comments[i]["node"]["created_at"],

                //            Likes = (int)comments[i]["node"]["edge_liked_by"]["count"]
                //        });
                //    }
                //}

                return media;
            }
            catch (Exception ex) // An error should probably be caught here... oh well.
            {
                Debug.WriteLine("GetMedia: " + ex.ToString());
                return null;
            }           
        }

        /// <summary>
        /// Searches Instagram for a specific hashtag and then returns what it can find.
        /// </summary>
        /// <param name="Hashtag">The hashtag that you want to search for.</param>
        /// <returns></returns>
        public static Hashtag SearchHashtag(string Hashtag) // Without the actual Hashtag, ex: minecraftmemes
        {
            Hashtag _hashtag = new Hashtag();

            string json = GetJsonFromIG(string.Format("https://www.instagram.com/explore/tags/{0}/", Hashtag.Trim())) ?? null;

            if (string.IsNullOrEmpty(json)) return null;

            JToken ht = JObject.Parse(json)["entry_data"]["TagPage"][0]["graphql"]["hashtag"];

            _hashtag.Name = (string)ht["name"];
            _hashtag.PostAmount = (int)ht["edge_hashtag_to_media"]["count"];

            JArray edges = (JArray)ht["edge_hashtag_to_media"]["edges"];

            for (int i = 0; i < edges.Count; i++)
            {
                PostMedia media = new PostMedia();

                media.OwnerID = (string)edges[i]["node"]["owner"]["id"];
                media.Shortcode = (string)edges[i]["node"]["shortcode"];

                switch ((string)edges[i]["node"]["__typename"])
                {
                    default:
                    case "GraphImage": media.Type = MediaType.Image; break;
                    case "GraphVideo": media.Type = MediaType.Video; break;
                    case "GraphSidecar": media.Type = MediaType.Collage; break;
                }

                JArray caption = (JArray)edges[i]["node"]["edge_media_to_caption"]["edges"];

                if (caption.Count != 0)
                    media.Caption = (string)caption[0]["node"]["text"];
                else
                    media.Caption = string.Empty;

                media.Likes = (int)edges[i]["node"]["edge_liked_by"]["count"];
                media.ThumbnailUrl = (string)edges[i]["node"]["thumbnail_src"];

                _hashtag.Posts.Add(media);
            }

            return _hashtag;
        }

        public static string GetProfilePicture(string Username)
        {
            try
            {
                string json = GetJsonFromIG(string.Format("https://www.instagram.com/{0}/", Username.Trim())) ?? null;

                if (string.IsNullOrEmpty(json)) return null;

                JObject ig = JObject.Parse(json);

                long _id = (long)ig["entry_data"]["ProfilePage"][0]["graphql"]["user"]["id"];

                return GetProfilePicture(_id);
            }
            catch
            {
                return null;
            }            
        }

        //[Obsolete("Instagram depreciated the API")]
        // It now works again it seems
        public static string GetProfilePicture(long UserID)
        {
            string json = string.Empty;

            try
            {
                json = client.DownloadString(string.Format("https://i.instagram.com/api/v1/users/{0}/info/", UserID));

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
        internal static string GetJsonFromIG(string URL)
        {
            //Debug.WriteLine(string.Format("'{0}'", URL));

            string html = string.Empty;
            try
            {
                html = client.DownloadString(URL);
            }
            catch (WebException ex)
            {
                Debug.WriteLine(URL);
                Debug.WriteLine(ex.ToString());
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
