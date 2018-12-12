using System;
using System.Collections.Generic;
using System.Text;
using Glastroika.API;
using Grapevine;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Newtonsoft.Json;

namespace Glastroika
{
    // TODO
    public class Rest
    {
        public Rest()
        {

        }

        public void Start()
        {

        }
    }

    [RestResource]
    public class Resource
    {
        [RestRoute(HttpMethod = Grapevine.Shared.HttpMethod.GET, PathInfo = "/getuser")]
        public IHttpContext GetUser(IHttpContext context)
        {
            string username = context.Request.QueryString["username"] ?? string.Empty;

            if (!string.IsNullOrEmpty(username))
            {
                User user = Instagram.GetUser(username);

                if (user != null)
                {
                    string json = JsonConvert.SerializeObject(user, Formatting.Indented);
                    context.Response.ContentType = Grapevine.Shared.ContentType.JSON;

                    context.Response.SendResponse(json);
                    return context;
                }
            }


            context.Response.SendResponse("ERROR");
            return context;
        }

        [RestRoute(HttpMethod = Grapevine.Shared.HttpMethod.GET, PathInfo = "/getmedia")]
        public IHttpContext GetMedia(IHttpContext context)
        {
            string shortcode = context.Request.QueryString["shortcode"] ?? string.Empty;

            if (!string.IsNullOrEmpty(shortcode))
            {
                Media media = Instagram.GetMedia(shortcode);

                if (media != null)
                {
                    string json = JsonConvert.SerializeObject(media, Formatting.Indented);
                    context.Response.ContentType = Grapevine.Shared.ContentType.JSON;

                    context.Response.SendResponse(json);
                    return context;
                }
            }


            context.Response.SendResponse("ERROR");
            return context;
        }

        // Should be at the bottom, so it will work as a fallback.
        [RestRoute]
        public IHttpContext Default(IHttpContext context)
        {
            context.Response.SendResponse("404");
            return context;
        }
    }
}
