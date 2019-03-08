using System;
using System.Collections.Generic;
using System.Text;

namespace Glastroika.API
{
    public class Media
    {
        public string Username { get; internal set; }

        public string Shortcode { get; internal set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public MediaType Type { get; internal set; }
        public string Caption { get; internal set; }

        public int Likes { get; internal set; }
        public List<Comment> Comments { get; internal set; }

        public int Timestamp { get; internal set; }

        public List<string> URL { get; internal set; }

        internal Media()
        {
            URL = new List<string>();
            Comments = new List<Comment>();
        }
    }

    public class Comment
    {
        public string Owner { get; internal set; }
        public string ProfilePicture { get; internal set; }

        public string Text { get; internal set; }
        public int Timestamp { get; internal set; }

        public int Likes { get; internal set; }

        internal Comment() { }
    }

    public enum MediaType
    {
        Collage,
        Video,
        Image,
    }
}
