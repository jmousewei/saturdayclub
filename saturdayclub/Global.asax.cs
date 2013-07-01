using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;

namespace saturdayclub
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class SaturdayClubApplication : System.Web.HttpApplication
    {
        public static readonly IDictionary<string, string> CommonTranslateTable = new Dictionary<string, string>();

        static SaturdayClubApplication()
        {
            CommonTranslateTable[@"梅子"] = @"小妖";
        }

        public static string TranslateToCommonWord(string word)
        {
            string commonWord = string.Empty;
            if (!CommonTranslateTable.TryGetValue(word, out commonWord))
            {
                return word;
            }
            return commonWord;
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            CreateCacheEntry();
        }

        internal static void CreateCacheEntry()
        {
            if (HttpRuntime.Cache["ActivityWatchdog"] != null)
                return;
            saturdayclub.Controllers.MsgController msg = new Controllers.MsgController();
            Thread worker = new Thread(() =>
                {
                    msg.WebClientTest();
                });
            worker.IsBackground = true;
            worker.Priority = ThreadPriority.Lowest;
            worker.Start();
        }

        internal static void ReCreateCacheEntry(string key, object value, System.Web.Caching.CacheItemRemovedReason reason)
        {
            if (reason == System.Web.Caching.CacheItemRemovedReason.Expired)
            {
                CreateCacheEntry();
            }
        }
    }
}