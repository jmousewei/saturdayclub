using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;
using System.Net;

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

            //CreateCacheEntry();

            HttpRuntime.Cache.Add(
                "Watchdog",
                DateTime.Now,
                null,
                DateTime.Now.AddSeconds(20),
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.NotRemovable,
                ReCreateCacheEntry);
        }

        internal static void CreateCacheEntry()
        {
            if (HttpRuntime.Cache["Watchdog"] != null)
                return;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadData("http://saturdayclubscrawl.apphb.com");
                }
                catch
                { }
            }
            HttpRuntime.Cache.Add(
                "Watchdog",
                DateTime.Now,
                null,
                DateTime.Now.AddMinutes(15),
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.NotRemovable,
                ReCreateCacheEntry);
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