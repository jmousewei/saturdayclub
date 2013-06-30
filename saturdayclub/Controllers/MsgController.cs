using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using saturdayclub.Messages;
using System.Threading;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace saturdayclub.Controllers
{
    public class MsgController : Controller
    {
        public static readonly string MessageToken = "saturdayclub_chenwei";

        [ActionName("test")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Test()
        {
            return View("test");
        }

        [ActionName("webclient")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult WebClientTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string replyMsg = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.GetEncoding("GBK");
                    replyMsg = wc.DownloadString("http://www.niwota.com/quan/13142806");
                    //replyMsg = Server.HtmlEncode(replyMsg);

                    List<string> activityList = new List<string>();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(replyMsg);
                    var root = doc.GetElementbyId("activities");
                    var table = root.SelectNodes("//div[@class='col_body']/ul[@class='activities txt_light']/li[@class!='head']");
                    foreach (var col in table)
                    {
                        HtmlNode temp = HtmlNode.CreateNode(col.OuterHtml);
                        var node = temp.SelectSingleNode("//li/span[@class='theme']/a");
                        string title = node.InnerText.Trim();
                        if (!string.IsNullOrEmpty(title))
                        {
                            activityList.Add(title);
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    for(int i=0;i<activityList.Count;i++)
                    {
                        sb.AppendLine((i + 1) + ". " + activityList[i]);
                    }
                    replyMsg = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                replyMsg = ex.ToString();
            }

            HttpRuntime.Cache.Add(
                "Activities",
                replyMsg,
                null,
                DateTime.MaxValue,
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);

            sw.Stop();
            return Content(sw.ElapsedMilliseconds + " ms.   " + replyMsg);
        }

        //
        // GET: /Msg/

        [ActionName("valid")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Valid(string signature, string timestamp, string nonce, string echostr)
        {
            ActionResult result = Content("Please visit via your WeiXin client.");
            if (MessageValidator.ValidateChecksum(signature, MessageToken, timestamp, nonce))
            {
                result = Content(echostr);
            }
            return result;
        }

        [ActionName("trans")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult TranslateMessage()
        {
            try
            {
                ClientTextMessage msg = new ClientTextMessage();
                msg.Deserialize(this.Request.InputStream);
                string replyMsg = string.Empty;
                if (string.Compare(msg.Content, @"查活动", true) == 0)
                {
                    replyMsg = HttpRuntime.Cache["Activities"] as string;
                    if (string.IsNullOrEmpty(replyMsg))
                    {
                        replyMsg = "No activity.";
                    }
                    DateTime utc = (DateTime)HttpRuntime.Cache["ActivityWatchdog"];
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    var time = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
                    replyMsg = "当前的活动列表: \r\n" + replyMsg + "\r\n\r\n此信息更新于: " + time.ToLongTimeString();
                }
                else
                {
                    replyMsg = "Hello, I'm a repeater, and I will repeat your every msg! Your msg is: '" + msg.Content + "' in encoding of '" + this.Request.ContentEncoding.EncodingName + "'.";
                }
                ReplyTextMessage reply = new ReplyTextMessage()
                {
                    To = msg.From,
                    From = msg.To,
                    CreateTime = ReplyTextMessage.Time(),
                    Content = "Message Length: " + replyMsg.Length + ", Content: " + replyMsg,
                    Functions = FunctionFlags.None
                };
                return Content(reply.GetMessageContent());
            }
            catch
            { }
            return new EmptyResult();
        }

    }
}
