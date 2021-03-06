﻿using saturdayclub.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Class1
    {
        public void Test()
        {
            ClientTextMessage msg = new ClientTextMessage();
            msg.To = "to";
            msg.From = "from";
            msg.CreateTime = ClientTextMessage.Time();
            msg.Content = "this is the msg";
            msg.MessageId = 1234;

            var httpPost = WebRequest.Create("http://localhost:13764/") as HttpWebRequest;
            httpPost.Method = "POST";
            httpPost.ContentType = "text/xml";
            var data = msg.GetMessageBytes();
            httpPost.ContentLength = data.Length;

            var request = httpPost.GetRequestStream();
            request.Write(data, 0, data.Length);

            var response = httpPost.GetResponse();
        }
    }
}
