using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace saturdayclub
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DefaultGet",
                url: "{action}",
                defaults: new { controller = "Msg", action = "valid" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "DefaultPost",
                url: "{action}",
                defaults: new { controller = "Msg", action = "trans" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
        }
    }
}