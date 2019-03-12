using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glastroika.API
{
    public class Hashtag
    {
        public string Name { get; internal set; }
        public int PostAmount { get; internal set; }

        public List<PostMedia> Posts { get; internal set; } = new List<PostMedia>();
    }

    // Modifed version of the normal Media class.
    public class PostMedia
    {
        public string OwnerID { get; internal set; }

        public string Shortcode { get; internal set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public MediaType Type { get; internal set; }
        public string Caption { get; internal set; }

        public int Likes { get; internal set; }

        public string ThumbnailUrl { get; internal set; }
    }
}
