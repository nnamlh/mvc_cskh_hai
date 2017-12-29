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
        public ActionResult Show(int? page, string search = "", string status = "process")
        {
            int permit = Utitl.CheckRoleShowInfo(db, User.Identity.Name);
            //
            //
            ViewBag.OrderStatus = db.OrderStatus.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            List<HaiOrder> data = new List<HaiOrder>();

            ViewBag.Status = status;
            ViewBag.Search = search;

            if (permit == 1)
            {
                // xem toan bo
                data = db.HaiOrders.Where(p => (p.BrachCode.Contains(search) || p.C1Code.Contains(search)) && p.OrderStatus == status).ToList();
            }
            else if (permit == 2)
            {
                // get list cn
                var branchPermit = db.UserBranchPermisses.Where(p => p.UserName == User.Identity.Name).Select(p => p.BranchCode).ToList();
                data = db.HaiOrders.Where(p => branchPermit.Contains(p.BrachCode) && p.OrderStatus.Contains(status) && p.C1Code.Contains(search)).ToList();
            }

            return View(data.OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Detail(string id)
        {
            // 
            var checkOrder = db.HaiOrders.Find(id);

            if (checkOrder == null)
                return RedirectToAction("error", "home");

            ViewBag.PayType = db.PayTypes.ToList();
            ViewBag.ShipType = db.ShipTypes.ToList();

            var c1C2 = db.C2C1.Where(p => p.C2Code == checkOrder.CInfoCommon.CCode).ToList();
            List<CommonInfo> c1List = new List<CommonInfo>();
            foreach (var item in c1C2)
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

            c1List.Add(new CommonInfo()
            {
                code = "B",
                name = "Lấy tại chi nhánh"
            });

            ViewBag.C1 = c1List;


            return View(checkOrder);
        }

        [HttpPost]
        public ActionResult Update(string Id, string PayType, string ShipType, string ExpectDate, string Sender)
        {
            var checkOrder = db.HaiOrders.Find(Id);

            if (checkOrder == null)
                return RedirectToAction("error", "home");


            if(checkOrder.OrderStatus == "begin")
            {
                checkOrder.PayType = PayType;
                checkOrder.ShipType = ShipType;
                var date = DateTime.ParseExact(ExpectDate, "MM/dd/yyyy", null);

                if (Sender == "B")
                {
                    // lay tai chi nhanh
                    checkOrder.Sender = "B";
                    checkOrder.C1Code = "";
                    checkOrder.C1Id = "";
                    checkOrder.C1Name = "";
                } else
                {
                    checkOrder.Sender = "CI";
                    var c1 = db.C1Info.Where(p => p.Code == Sender).FirstOrDefault();
                    if (c1 != null)
                    {
                        checkOrder.C1Code = c1.Code;
                        checkOrder.C1Id = c1.Id;
                        checkOrder.C1Name = c1.StoreName;
                    }

                }

                checkOrder.ExpectDate = date;

                db.Entry(checkOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }

            return RedirectToAction("detail", "order", new { id = Id });
        }

        public ActionResult UpdateProductQuantity(string orderId, string productId, int? can, int? box)
        {
            if (can == null)
                can = 0;

            if (box == null)
                box = 0;

            var orderProduct = db.OrderProducts.Where(p => p.ProductId == productId && p.OrderId == orderId).FirstOrDefault();

            if (orderProduct == null && orderProduct.HaiOrder.OrderStatus == "begin")
                return RedirectToAction("error", "home");

            ProductInfo pInfo = orderProduct.ProductInfo;
            var quantity = can * pInfo.Quantity + box;

            if (quantity != 0)
            {
                orderProduct.Quantity = quantity;
                orderProduct.PerPrice = pInfo.Price;
                orderProduct.PriceTotal = pInfo.Price * quantity;
                db.Entry(orderProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // update order 
                double? priceTotal = 0;

                var allOrderProduct = db.OrderProducts.Where(p => p.OrderId == orderId).ToList();
                foreach(var item in allOrderProduct)
                {
                    priceTotal += item.PriceTotal;
                }


                var haiOrder = db.HaiOrders.Find(orderId);

                haiOrder.PriceTotal = priceTotal;

                db.Entry(haiOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }

            return RedirectToAction("detail", "order", new { id = orderId });
        }


        /*
        public ActionResult ModifyProduct(string order, string product)
        {
            var check = db.OrderProducts.Where(p => p.OrderId == order && p.ProductId == product).FirstOrDefault();
            if (check == null)
                return RedirectToAction("error", "home");

            var c1C2 = db.C2C1.Where(p => p.C2Code == check.HaiOrder.CInfoCommon.CCode).ToList();
            List<CommonInfo> c1List = new List<CommonInfo>();
            foreach (var item in c1C2)
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
                //check.C1Id = c1Check.Id;
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("detail", "order", new { id = order });
        }
        */
        [HttpPost]
        public ActionResult Approve(string id, string notes, int status)
        {
            var check = db.HaiOrders.Find(id);
            if (check == null)
                return RedirectToAction("error", "home");

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

            if (staff == null)
                return RedirectToAction("error", "home");



            if (check.OrderStatus == "begin" && status == 1)
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

                if (staffCreateOrder != null)
                {
                    Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng đã được xác nhận, cửa hàng đang chuẩn bị giao hàng", staffCreateOrder.HaiStaff.UserLogin, db, mongoHelp);
                }

                // c2
                Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng của " + check.CInfoCommon.CName + " đã được xác nhận", check.CInfoCommon.UserLogin, db, mongoHelp);

                // c1
                if (check.Sender == "CI")
                {
                    var checkC1 = db.C1Info.Find(check.C1Id);
                    if (checkC1 != null)
                    {
                        Utitl.Send("Bạn có 1 đơn hàng cần giao: " + check.Code, "đơn hàng của " + check.CInfoCommon.CName + " (" + check.CInfoCommon.CCode + ")", check.CInfoCommon.UserLogin, db, mongoHelp);
                    }
                }

            }
            else if (check.OrderStatus == "begin" && status == 0)
            {
                check.OrderStatus = "cancel";
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save
                var saveProcess = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = id,
                    ProcessId = "cancel",
                    StaffId = staff.Id,
                    Notes = notes
                };
                db.OrderStaffs.Add(saveProcess);
                db.SaveChanges();


                // thong bao
                // nhan vien thi truong
                var staffCreateOrder = check.OrderStaffs.Where(p => p.ProcessId == "create").FirstOrDefault();

                if (staffCreateOrder != null)
                {
                    Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng của " + check.CInfoCommon.CName + " đã hủy vì " + notes, staffCreateOrder.HaiStaff.UserLogin, db, mongoHelp);
                }

                // c2
                Utitl.Send("Đơn hàng " + check.Code, "Đơn hàng đã bị hủy vì " + notes, check.CInfoCommon.UserLogin, db, mongoHelp);

            }

            return RedirectToAction("detail", "order", new { id = id });
        }

        [HttpPost]
        public ActionResult Finish(string id, string notes, int status)
        {
            var check = db.HaiOrders.Find(id);
            if (check == null)
                return RedirectToAction("error", "home");

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

            if (staff == null)
                return RedirectToAction("error", "home");

            if (check.OrderStatus == "process")
            {
                // save
                var saveProcess = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = id,
                    StaffId = staff.Id,
                    Notes = notes
                };

                if (status == 1)
                {
                    check.OrderStatus = "finish";
                    saveProcess.ProcessId = "finish";
                } else if (status == 0)
                {
                    check.OrderStatus = "notfinish";
                    saveProcess.ProcessId = "notfinish";
                } else
                {
                    return RedirectToAction("error", "home");
                }

                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.OrderStaffs.Add(saveProcess);
                db.SaveChanges();


                string msgStaff = "Đơn hàng đã hoàn tất giao hàng tới đại lý " + check.CInfoCommon.CName + "-" + check.CInfoCommon.CCode;
                string msgC2 = "Đơn hàng đã giao hàng hoàn tất, quý khách kiểm tra và thông báo lại nếu có sai xót";

                if(status == 0)
                {
                    msgStaff = "Đơn hàng đã hủy giao hàng tới đại lý " + check.CInfoCommon.CName + "-" + check.CInfoCommon.CCode + ", vì " + notes;
                    msgC2 = "Đơn hàng của quý khách đã hủy vì " + notes;
                } 

                // thong bao
                // nhan vien thi truong
                var staffCreateOrder = check.OrderStaffs.Where(p => p.ProcessId == "create").FirstOrDefault();

                if (staffCreateOrder != null)
                {
                    Utitl.Send("Đơn hàng " + check.Code, msgStaff, staffCreateOrder.HaiStaff.UserLogin, db, mongoHelp);
                }

                // c2
                Utitl.Send("Đơn hàng " + check.Code, msgC2, check.CInfoCommon.UserLogin, db, mongoHelp);


            }

            return RedirectToAction("detail", "order", new { id = id });
        }

        [HttpPost]
        public ActionResult UpdateDelivery(string orderId, string productId, int? can, int? box)
        {
            if (can == null)
                can = 0;

            if (box == null)
                box = 0;

            var orderProduct = db.OrderProducts.Where(p => p.ProductId == productId && p.OrderId == orderId).FirstOrDefault();

            if (orderProduct == null)
                return RedirectToAction("error", "home");

            // check quantity
            int? quantityRemain = orderProduct.Quantity - orderProduct.QuantityFinish;

            var quantity = box + orderProduct.ProductInfo.Quantity * can;

            if (quantityRemain >= quantity)
            {
                orderProduct.QuantityFinish = orderProduct.QuantityFinish + quantity;

                db.Entry(orderProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save history

                var history = new OrderProductHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.Now,
                    Notes = "Nhan vien cong ty cap nhat",
                    OrderId = orderProduct.OrderId,
                    ProductId = orderProduct.ProductId,
                    Quantity = quantity
                };

                db.OrderProductHistories.Add(history);
                db.SaveChanges();
            }




            return RedirectToAction("detail", "order", new { id = orderId });
        }
    }
}