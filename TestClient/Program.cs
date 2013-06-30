using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using saturdayclub.Messages;
using HtmlAgilityPack;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //ClientTextMessage msg = new ClientTextMessage();
            //msg.To = "to";
            //msg.From = "from";
            //msg.CreateTime = ClientTextMessage.Time();
            //msg.Content = "this is the msg";
            //msg.MessageId = 1234;

            //var httpPost = WebRequest.Create("http://localhost:13764/") as HttpWebRequest;
            //httpPost.Method = "POST";
            //httpPost.ContentType = "text/xml";
            //var data = msg.GetMessageBytes();
            //httpPost.ContentLength = data.Length;

            //var request = httpPost.GetRequestStream();
            //request.Write(data, 0, data.Length);

            //var response = httpPost.GetResponse();

            //string replyMsg = string.Empty;
            //using (WebClient wc = new WebClient())
            //{
            //    wc.Encoding = Encoding.GetEncoding("GBK");
            //    replyMsg = wc.DownloadString("http://www.niwota.com/quan/13142806");
            //    //replyMsg = Server.HtmlEncode(replyMsg);

            //    List<string> activityList = new List<string>();
            //    HtmlDocument doc = new HtmlDocument();
            //    doc.LoadHtml(replyMsg);
            //    var root = doc.GetElementbyId("activities");
            //    var table = root.SelectNodes("//div[@class='col_body']/ul[@class='activities txt_light']/li[@class!='head']");
            //    foreach (var col in table)
            //    {
            //        HtmlNode temp = HtmlNode.CreateNode(col.OuterHtml);
            //        var node = temp.SelectSingleNode("//li/span[@class='theme']/a");
            //        activityList.Add(node.InnerText.Trim());
            //    }
            //}


            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        }
    }
}
