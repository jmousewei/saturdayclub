using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace saturdayclub
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class SaturdayClubApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            CreateCacheEntry();
        }

        private void CreateCacheEntry()
        {
            if (HttpRuntime.Cache["ActivityWatchdog"] != null)
                return;
            saturdayclub.Controllers.MsgController msg = new Controllers.MsgController();
            msg.WebClientTest();
            HttpRuntime.Cache.Add(
                "ActivityWatchdog",
                DateTime.UtcNow,
                null,
                DateTime.MaxValue,
                TimeSpan.FromMinutes(30),
                System.Web.Caching.CacheItemPriority.Normal,
                new System.Web.Caching.CacheItemRemovedCallback(ReCreateCacheEntry));
        }

        private void ReCreateCacheEntry(string key, object value, System.Web.Caching.CacheItemRemovedReason reason)
        {
            if (reason == System.Web.Caching.CacheItemRemovedReason.Expired)
            {
                CreateCacheEntry();
            }
        }
    }
}