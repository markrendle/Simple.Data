using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Simple.Web.Controllers
{
    public class PeopleController : Controller
    {
        //
        // GET: /People/

        public ActionResult Index()
        {
            return View();
        }

        // GET: /People/id
        public ActionResult Get(int id)
        {
            ViewData["Id"] = id;
            return View();
        }
    }
}
