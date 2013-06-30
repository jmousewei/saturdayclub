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
using System.IO;

namespace saturdayclub.Controllers
{
    public class MsgController : Controller
    {
        public static readonly string MessageToken = "saturdayclub_chenwei";

        [ActionName("test")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Test(bool? doLogin, string username, string password)
        {
            string html = string.Empty;
            Encoding enc = Encoding.GetEncoding("GBK");
            CookieContainer ckc = new CookieContainer();

            var request = WebRequest.CreateHttp("http://www.niwota.com/quan/13142806");
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";
            request.KeepAlive = true;
            request.CookieContainer = ckc;

            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = response.GetResponseStream();
            byte[] buf = new byte[4096];
            using (MemoryStream ms = new MemoryStream())
            {
                int length = responseStream.Read(buf, 0, buf.Length);
                while (length > 0)
                {
                    ms.Write(buf, 0, length);
                    length = responseStream.Read(buf, 0, buf.Length);
                }
                html = enc.GetString(ms.ToArray(), 0, (int)ms.Length);
            }


            if (doLogin.HasValue && doLogin.Value)
            {
                request = WebRequest.CreateHttp("http://www.niwota.com/user/login_page.jsp");
                request.Method = "POST";
                request.Referer = "http://www.niwota.com/quan/13142806";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = true;
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                request.CookieContainer = ckc;

                string postData = string.Format(
                    "doLogin=true&username={0}&password={1}&remember=true&Submit=%B5%C7%C2%BC",
                    Server.UrlEncode(username ?? string.Empty),
                    Server.UrlEncode(password ?? string.Empty));
                byte[] data = enc.GetBytes(postData);

                request.ContentLength = data.Length;
                var reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);

                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                using (MemoryStream ms = new MemoryStream())
                {
                    int length = responseStream.Read(buf, 0, buf.Length);
                    while (length > 0)
                    {
                        ms.Write(buf, 0, length);
                        length = responseStream.Read(buf, 0, buf.Length);
                    }
                    html = enc.GetString(ms.ToArray(), 0, (int)ms.Length);
                }
            }

            return Content(html, "text/html", enc);
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
                        node = temp.SelectSingleNode("//li/span[@class='host']/a");
                        string contact = SaturdayClubApplication.TranslateToCommonWord(node.InnerText.Trim());
                        if (!string.IsNullOrEmpty(title))
                        {
                            activityList.Add(title + @" (联系人: " + contact + ")");
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
                System.Web.Caching.Cache.NoAbsoluteExpiration,
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
            if (RequestHelper.ValidateChecksum(signature, MessageToken, timestamp, nonce))
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
                ReplyTextMessage reply = null;
                do
                {
                    string replyMsgContent = string.Empty;
                    var xmlMsg = XmlMessage.CreateClient(this.Request.InputStream);
                    if (xmlMsg.MessageType == MessageTypes.Text)
                    {
                        var textMsg = xmlMsg as ClientTextMessage;
                        if (string.Compare(textMsg.Content, @"查活动", true) == 0)
                        {
                            replyMsgContent = HttpRuntime.Cache["Activities"] as string;
                            if (string.IsNullOrEmpty(replyMsgContent))
                            {
                                replyMsgContent = "No activity.";
                            }
                            DateTime utc = (DateTime)HttpRuntime.Cache["ActivityWatchdog"];
                            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                            var time = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
                            replyMsgContent = "当前的活动列表: \r\n\r\n" + replyMsgContent + "\r\n此信息更新于: " + time;
                        }
                        else if (string.Compare(textMsg.Content, @"帮助", true) == 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(@"当前支持的功能：");
                            sb.AppendLine();
                            sb.AppendLine(@"1. 直接发送‘帮助’获取当前可用功能列表和说明。");
                            sb.AppendLine(@"2. 直接发送‘查活动’获取当前活动列表。活动列表每30分钟自动更新，所以您可能获取的不是最新的列表。");
                            sb.AppendLine(@"3. 直接发送任意文字、基础表情获取系统测试性回复。");
                            replyMsgContent = sb.ToString();
                        }
                        else
                        {
                            replyMsgContent = "Hello, I'm a repeater, and I will repeat your every msg! Your msg is: '" + textMsg.Content + "' in encoding of '" + this.Request.ContentEncoding.EncodingName + "'.";
                        }
                    }
                    else if (xmlMsg.MessageType == MessageTypes.Event)
                    {
                        var eventMsg = xmlMsg as EventMessage;
                        if (eventMsg.Event == MessageEvents.Subscribe)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(@"欢迎加入星期六测试计划。");
                            sb.AppendLine();
                            sb.AppendLine(@"本账户会为您提供基于圈网的功能性服务，不提供任何聊天功能。当然，如果您输入文字刚好是星期六感兴趣的话题，也许您会收到回复。");
                            sb.AppendLine();
                            sb.AppendLine(@"为了提高服务质量，您向本账号发送的任何消息将被记录并保留5天时间。");
                            sb.AppendLine();
                            sb.AppendLine(@"本账户不会要求您输入任何个人信息（包括您圈网的用户名和密码），如发现有人通过本账户向您索取个人信息，请及时通知开发人员处理。");
                            sb.AppendLine();
                            sb.AppendLine(@"加入测试计划代表您同意：");
                            sb.AppendLine(@"1. 开发人员会在未经通知的情况下向您推送测试消息（包括文字、图文或语音）。");
                            sb.AppendLine(@"2. 测试系统功能，并反馈任何您所遇到的问题（包括改善建议）。");
                            sb.AppendLine(@"3. 同意我们的隐私策略。");
                            sb.AppendLine();
                            sb.AppendLine(@"您可以向公用账户发送‘帮助’来获取当前可用的功能。注意：任何输入不带引号哦。");
                            sb.AppendLine(@"感谢您的参与，我们将竭诚为您服务。");
                            sb.AppendLine();
                            sb.AppendLine(@"开发团队");
                            replyMsgContent = sb.ToString();
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(replyMsgContent))
                        break;
                    reply = new ReplyTextMessage()
                    {
                        To = xmlMsg.From,
                        From = xmlMsg.To,
                        CreateTime = ReplyTextMessage.Time(),
                        Content = replyMsgContent,
                        Functions = FunctionFlags.None
                    };

                } while (false);

                if (reply != null)
                {
                    return Content(reply.GetMessageContent());
                }
            }
            catch
            { }
            return new EmptyResult();
        }

    }
}
