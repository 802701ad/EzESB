using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EzESB.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return Content("");
        }

        public ActionResult Run()
        {
            return Content("");
        }

        public ActionResult Send()
        {
            if (Request.QueryString["message_type"] == null)
                throw new Exception("Please specify a message_type as a URL parameter.");
            string message_id = Request.QueryString["message_id"] ?? Guid.NewGuid().ToString();
            string message_type = Request.QueryString["message_type"];
            db.SaveMessage(message_id: message_id, message_type: message_type, values: Request.Form);
            ESB_Main.RunEvents();
            return Content(message_id);
        }

    }
}
