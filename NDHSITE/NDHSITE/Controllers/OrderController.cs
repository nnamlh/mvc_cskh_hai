using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using NDHSITE.Util;
using PagedList;

namespace NDHSITE.Controllers
{
    public class OrderController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        // GET: Order
        public ActionResult Show( int? page, string agency = "", string search = "")
        {
            int permit = Utitl.CheckRoleShowInfo(db, User.Identity.Name);
            //
            //
            ViewBag.OrderStatus = db.OrderStatus.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            List<HaiOrder> data = new List<HaiOrder>();

            if (permit == 1)
            {
                // xem toan bo
               data = db.HaiOrders.Where(p => p.Agency.Contains(agency) && (p.BrachCode.Contains(search))).ToList();

            }
            else if (permit == 2)
            {
                // get list cn
                var branchPermit = db.UserBranchPermisses.Where(p => p.UserName == User.Identity.Name).Select(p => p.BranchCode).ToList();
                data = db.HaiOrders.Where(p => branchPermit.Contains(p.BrachCode)).ToList();

            }

            return View(data.OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Detail(string id)
        {
            // 
            var checkOrder = db.HaiOrders.Find(id);

            if (checkOrder == null)
                return RedirectToAction("error", "home");


            return View(checkOrder);
        }

        public ActionResult ModifyProduct(string order, string product)
        {
            var check = db.OrderProducts.Where(p => p.OrderId == order && p.ProductId == product).FirstOrDefault();
            if (check == null)
                return RedirectToAction("error", "home");

            var c1C2= db.C2C1.Where(p => p.C2Code == check.HaiOrder.CInfoCommon.CCode).ToList();
            List<CommonInfo> c1List = new List<CommonInfo>();
            foreach(var item in c1C2)
            {
                var c1Check = db.C1Info.Where(p => p.Code == item.C1Code).FirstOrDefault();
                if (c1Check != null)
                {
                    //
                    c1List.Add(new CommonInfo()
                    {
                        code = c1Check.Code,
                        name = c1Check.StoreName
                    });
                }
            }

            ViewBag.C1 = c1List;

            return View(check);
        }
    }
}