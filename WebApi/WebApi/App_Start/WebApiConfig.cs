using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace WebApi
{
    public static class WebApiConfig
    {

        public class CustomJSONFormatter : JsonMediaTypeFormatter
        {
            public CustomJSONFormatter()
            {
                this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            }
            public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
            {
                base.SetDefaultContentHeaders(type, headers, mediaType);
                headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        }

        public static void Register(HttpConfiguration config)
        {

            /*
              // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            ); */

            //retorna Json em vez de xml
            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.Formatters.Add(new CustomJSONFormatter());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "MipWebApi",
                //routeTemplate: "{controller}/v1/{action}/{id}",
                routeTemplate: "{controller}/v1/{action}"
            //defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
