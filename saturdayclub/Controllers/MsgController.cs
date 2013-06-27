using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace saturdayclub.Controllers
{
    public class MsgController : Controller
    {
        //
        // GET: /Msg/

        [ActionName("valid")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Valid()
        {
            return View("Default.aspx");
        }

    }
}
