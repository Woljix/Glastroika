using System;
using System.Collections.Generic;
using System.Text;

namespace Glastroika.API
{
    public class User
    {
        public string Username { get; internal set; }

        public string FullName { get; internal set; }
        public string Biography { get; internal set; }

        public string ProfilePicture { get; internal set; }

        public List<Media> Media { get; internal set; } = new List<Media>();

        internal User() { }
    }
}
