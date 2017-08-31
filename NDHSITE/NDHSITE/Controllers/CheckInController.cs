using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;


namespace NDHSITE.Controllers
{
    public class CheckInController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();

        public ActionResult Approve()
        {
            return View();
        }
    }
}