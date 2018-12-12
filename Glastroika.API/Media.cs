using System;
using System.Collections.Generic;
using System.Text;

namespace Glastroika.API
{
    public class Media
    {
        public string Shortcode { get; internal set; }
        public string Type { get; internal set; }
        public string Caption { get; internal set; }

        public int Timestamp { get; internal set; }

        public List<string> URLs { get; internal set; }

        internal Media()
        {
            URLs = new List<string>();
        }
    }
}
