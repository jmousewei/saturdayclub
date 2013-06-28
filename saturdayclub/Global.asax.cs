using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace saturdayclub
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "DefaultGet",
                "{action}",
                new { controller = "Msg", action = "valid" },
                new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                "DefaultPost",
                "{action}",
                new { controller = "Msg", action = "trans" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );

            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}", // URL with parameters
            //    new { controller = "Msg", action = "valid" } // Parameter defaults
            //);

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }
    }
}