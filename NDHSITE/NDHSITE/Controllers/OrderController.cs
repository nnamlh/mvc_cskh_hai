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
        MongoHelper mongoHelp = new MongoHelper();
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

        [HttpPost]
        public ActionResult ModifyProduct(string c1, string order, string product)
        {
            var check = db.OrderProducts.Where(p => p.OrderId == order && p.ProductId == product).FirstOrDefault();
            if (check == null)
                return RedirectToAction("error", "home");

            var c1Check = db.C1Info.Where(p => p.Code == c1).FirstOrDefault();
            if (c1Check != null)
            {
                check.C1Id = c1Check.Id;
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("detail", "order", new { id = order });
        }

        [HttpPost]
        public ActionResult Approve(string id, string notes)
        {
            var check = db.HaiOrders.Find(id);
            if (check == null)
                return RedirectToAction("error", "home");

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

            if (staff == null)
                return RedirectToAction("error", "home");

            if (check.OrderStatus == "begin")
            {
                check.OrderStatus = "process";
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save
                var saveProcess = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = id,
                    ProcessId = "approve",
                    StaffId = staff.Id,
                    Notes = notes
                };
                db.OrderStaffs.Add(saveProcess);
                db.SaveChanges();


                // thong bao
                // nhan vien thi truong
                var staffCreateOrder = check.OrderStaffs.Where(p => p.ProcessId == "create").FirstOrDefault();

                if(staffCreateOrder != null)
                {
                    Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng đã được xác nhận, cửa hàng đang chuẩn bị giao hàng", staffCreateOrder.HaiStaff.UserLogin, db, mongoHelp);
                }

                // c2
                Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng của " + check.CInfoCommon.CName + " đã được xác nhận", check.CInfoCommon.UserLogin, db, mongoHelp);

                // c1
                foreach(var item in check.OrderProducts)
                {
                    Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng của " + check.CInfoCommon.CName + " đã được xác nhận, vào phần đơn hàng để xem chi tiết", staffCreateOrder.HaiStaff.UserLogin, db, mongoHelp);
                }

            }

            return RedirectToAction("detail", "order", new { id = id });
        }

        [HttpPost]
        public ActionResult Finish(string id, string notes)
        {
            var check = db.HaiOrders.Find(id);
            if (check == null)
                return RedirectToAction("error", "home");

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

            if (staff == null)
                return RedirectToAction("error", "home");

            if (check.OrderStatus == "process")
            {
                check.OrderStatus = "finish";
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save
                var saveProcess = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = id,
                    ProcessId = "finish",
                    StaffId = staff.Id,
                    Notes = notes
                };
                db.OrderStaffs.Add(saveProcess);
                db.SaveChanges();
            }

            return RedirectToAction("detail", "order", new { id = id });
        }
    }
}