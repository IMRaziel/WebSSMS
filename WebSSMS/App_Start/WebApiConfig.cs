using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebSSMS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			config.MapHttpAttributeRoutes();
			config.Routes.MapHttpRoute(
				"Default", // Route name
				"{controller}/{action}", // URL with parameters
				new { controller = "WebSsms", action = "Index"} // Parameter defaults
				);
			var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
			config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

			var cors = new EnableCorsAttribute(
						origins: "*",
						headers: "*",
						methods: "*");
			config.EnableCors(cors);
		}
    }
}
