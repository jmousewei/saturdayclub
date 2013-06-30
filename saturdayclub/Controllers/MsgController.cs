using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using saturdayclub.Messages;
using saturdayclub.Analyze;
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
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string replyMsg = string.Empty;
            try
            {
                using (var context = new AnalyzeContext())
                {
                    ActivityAnalyzer analyzer = new ActivityAnalyzer();
                    AnalyzeToken token = new AnalyzeToken();
                    using (ManualResetEvent evt = new ManualResetEvent(false))
                    {
                        context.RunAsync(() =>
                        {
                            token = analyzer.Analyze(null);
                            evt.Set();
                        });
                        if (!evt.WaitOne(20000))
                        {
                            throw new TimeoutException("Cannot start new thread!");
                        }
                        var result = analyzer.RetrieveResult(token, 20000) as List<string>;
                        replyMsg = string.Join("; ", result.ToArray());
                    }
                    context.Exit();
                }
            }
            catch (Exception ex)
            {
                replyMsg = ex.ToString();
            }
            if (string.IsNullOrEmpty(replyMsg))
            {
                replyMsg = "No activity.";
            }
            sw.Stop();
            return Content(replyMsg + " " + sw.ElapsedMilliseconds + " ms.");
        }

        [ActionName("webclient")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult WebClientTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string replyMsg = string.Empty;
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
                replyMsg = string.Join("; ", activityList.ToArray());
            }
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
                    replyMsg = string.Join("; ", activityList.ToArray());
                }
            }
            catch(Exception ex)
            {
                replyMsg = ex.ToString();
            }
            if (string.IsNullOrEmpty(replyMsg))
            {
                replyMsg = "No activity.";
            }
            try
            {
                ClientTextMessage msg = new ClientTextMessage();
                msg.Deserialize(this.Request.InputStream);
                ReplyTextMessage reply = new ReplyTextMessage()
                {
                    To = msg.From,
                    From = msg.To,
                    CreateTime = ReplyTextMessage.Time(),
                    //Content = "Hello, I'm a repeater, and I will repeat every ur msg! Your msg is: '" + msg.Content + "' in encoding of '" + this.Request.ContentEncoding.EncodingName + "'.",
                    Content = replyMsg.Length + ": " + replyMsg,
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
