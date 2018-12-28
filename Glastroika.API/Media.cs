using System;
using System.Collections.Generic;
using System.Text;

namespace Glastroika.API
{
    public class Media
    {
        public string Shortcode { get; internal set; }
        public MediaType Type { get; internal set; }
        public string Caption { get; internal set; }

        public int Timestamp { get; internal set; }

        public List<string> URL { get; internal set; }

        internal Media()
        {
            URL = new List<string>();
        }
    }

    public enum MediaType
    {
        Collage,
        Video,
        Image,
    }
}
