using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace saturdayclub.Controllers
{
    public class MsgController : Controller
    {
        public static readonly string MessageToken = "saturdayclub_chenwei";

        private bool ValidSignature(string signature, string timestamp, string nonce)
        {
            bool checkResult = false;
            do
            {
                if (string.IsNullOrEmpty(signature) ||
                    string.IsNullOrEmpty(timestamp) ||
                    string.IsNullOrEmpty(nonce))
                {
                    break;
                }

                string[] param = new[] { MessageToken, timestamp, nonce };
                Array.Sort<string>(param);
                string hashParam = string.Join(string.Empty, param);
                byte[] buf = Encoding.UTF8.GetBytes(hashParam);
                using (var sha1 = SHA1.Create())
                {
                    var hash = sha1.ComputeHash(buf);
                    StringBuilder sb = new StringBuilder();
                    foreach (var b in hash)
                    {
                        if (b < 0x10)
                            sb.Append('0');
                        sb.AppendFormat("{0:x}", b);
                    }
                    string hashStr = sb.ToString();
                    checkResult = (string.Compare(signature, hashStr, true) == 0);
                }
            } while (false);
            return checkResult;
        }

        //
        // GET: /Msg/

        [ActionName("valid")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Valid(string signature, string timestamp, string nonce, string echostr)
        {
            ActionResult result = Content("Please visit via your WeiXin client.");
            if (ValidSignature(signature, timestamp, nonce))
            {
                result = Content(echostr);
            }                
            return result;
        }

        [ActionName("trans")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult TranslateMessage()
        {

            return new EmptyResult();
        }

    }
}
