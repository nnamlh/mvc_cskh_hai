using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using NDHSITE.Util;
using PagedList;
using System.Data.Entity;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{
    public class OrderController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        MongoHelper mongoHelp = new MongoHelper();
        // GET: Order
        public ActionResult Show(int? page, string OrderCode = "", string DateFrom = "", string DateTo = "", string StaffCode = "", string ProcessId = "process", string StatusId = "", string SalePlace = "", string C1Code = "")
        {
            int permit = Utitl.CheckRoleShowInfo(db, User.Identity.Name);

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            var current = DateTime.Now;

            DateTime dFrom;

            DateTime dTo;

            if (String.IsNullOrEmpty(DateFrom) || String.IsNullOrEmpty(DateTo))
            {
                dTo = current;
                dFrom = current.AddMonths(-1);
            }
            else
            {
                dFrom = DateTime.ParseExact(DateFrom, "d/M/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "d/MM/yyyy", null);
            }

            ViewBag.DateFrom = dFrom.ToString("dd/MM/yyyy");
            ViewBag.DateTo = dTo.ToString("dd/MM/yyyy");
            ViewBag.StaffCode = StaffCode;
            ViewBag.ProcessId = ProcessId;
            ViewBag.StatusId = StatusId;
            ViewBag.SalePlace = SalePlace;
            ViewBag.C1Code = C1Code;

            List<get_list_orders_Result> data = new List<get_list_orders_Result>();

            if (!String.IsNullOrEmpty(OrderCode))
            {
                data = db.get_list_orders_by_code("%" + OrderCode + "%").ToList();
            }
            else
            {
                data = db.get_list_orders(dFrom.ToString("yyyy-MM-dd"), dTo.ToString("yyyy-MM-dd"), "%" + ProcessId + "%", "%" + StatusId + "%", "%" + StaffCode + "%", "%" + C1Code + "%").ToList();
            }

            data = data.Where(p => p.SalePlace.Contains(SalePlace)).ToList();

            if (permit == 2)
            {
                // get list cn
                var branchPermit = db.UserBranchPermisses.Where(p => p.UserName == User.Identity.Name).Select(p => p.BranchCode).ToList();
                data = data.Where(p => branchPermit.Contains(p.BrachCode)).ToList();
            }

            return View(data.OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ListOrderExcel(string DateFrom = "", string DateTo = "", string StaffCode = "", string ProcessId = "process", string StatusId = "", string SalePlace = "", string C1Code = "")
        {
            int permit = Utitl.CheckRoleShowInfo(db, User.Identity.Name);

            var current = DateTime.Now;

            DateTime dFrom;

            DateTime dTo;

            if (String.IsNullOrEmpty(DateFrom) || String.IsNullOrEmpty(DateTo))
            {
                dTo = current;
                dFrom = current.AddMonths(-1);
            }
            else
            {
                dFrom = DateTime.ParseExact(DateFrom, "d/M/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "d/MM/yyyy", null);
            }

            List<get_list_orders_product_Result> data = new List<get_list_orders_product_Result>();


            data = db.get_list_orders_product(dFrom.ToString("yyyy-MM-dd"), dTo.ToString("yyyy-MM-dd"), "%" + ProcessId + "%", "%" + StatusId + "%", "%" + StaffCode + "%", "%" + C1Code + "%").ToList();

            data = data.Where(p => p.SalePlace.Contains(SalePlace)).ToList();

            if (permit == 2)
            {
                // get list cn
                var branchPermit = db.UserBranchPermisses.Where(p => p.UserName == User.Identity.Name).Select(p => p.BranchCode).ToList();
                data = data.Where(p => branchPermit.Contains(p.BrachCode)).ToList();
            }

            string pathRoot = Server.MapPath("~/haiupload/list_order_form.xlsx");
            string name = "donhang" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = i + 1;
                            worksheet.Cells[i + 2, 2].Value = data[i].CreateDate;
                            worksheet.Cells[i + 2, 3].Value = data[i].Code;
                            worksheet.Cells[i + 2, 4].Value = data[i].StaffName;
                            worksheet.Cells[i + 2, 5].Value = data[i].StaffCode;
                            worksheet.Cells[i + 2, 6].Value = data[i].AgencyCode;
                            worksheet.Cells[i + 2, 7].Value = data[i].Store;
                            worksheet.Cells[i + 2, 8].Value = data[i].ExpectDate;
                            worksheet.Cells[i + 2, 9].Value = data[i].C1Code;
                            worksheet.Cells[i + 2, 10].Value = data[i].C1Name;
                            worksheet.Cells[i + 2, 11].Value = data[i].PName;
                            worksheet.Cells[i + 2, 12].Value = data[i].OrderQuantity / data[i].PQuantity;
                            worksheet.Cells[i + 2, 13].Value = data[i].OrderQuantity % data[i].PQuantity;
                            worksheet.Cells[i + 2, 14].Value = data[i].QuantityFinish / data[i].PQuantity;
                            worksheet.Cells[i + 2, 15].Value = data[i].QuantityFinish % data[i].PQuantity;

                        }
                        catch
                        {
                            return RedirectToAction("error", "home");
                        }

                    }

                    package.Save();

                }

            }
            catch
            {
                return RedirectToAction("error", "home");
            }


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("ds-don-hang-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));
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


            if (checkOrder.OrderStatus == "begin")
            {
                checkOrder.PayType = PayType;
                checkOrder.ShipType = ShipType;
                var date = DateTime.ParseExact(ExpectDate, "MM/dd/yyyy", null);

                if (Sender == "B")
                {
                    // lay tai chi nhanh
                    checkOrder.SalePlace = "B";
                    checkOrder.C1Code = "";
                    checkOrder.C1Id = "";
                    checkOrder.C1Name = "";
                }
                else
                {
                    checkOrder.SalePlace = "CI";
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


        [HttpPost]
        public ActionResult UpdateDelivery(string orderId, string productId, int? can, int? box)
        {
            if (can == null || box == null)
                return Json(new { id = 0 }, JsonRequestBehavior.AllowGet);

            var orderProduct = db.OrderProducts.Where(p => p.ProductId == productId && p.OrderId == orderId).FirstOrDefault();

            if (orderProduct == null)
                return Json(new { id = 0 }, JsonRequestBehavior.AllowGet);

            var quantity = box + orderProduct.ProductInfo.Quantity * can;

            orderProduct.QuantityFinish =  quantity;

            db.Entry(orderProduct).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            string stt = "";

            if (quantity == 0)
            {
                stt = "Chưa giao";
            }
            else if (quantity == orderProduct.Quantity)
            {
                stt = "Giao đủ";
            }
            else if (quantity > orderProduct.Quantity)
            {
                stt = "Giao nhiều hơn";
            }
            else if (quantity < orderProduct.Quantity)
            {
                stt = "Giao ít hơn";
            }

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


            return Json(new { id = 1, money = (quantity*orderProduct.PerPrice).Value.ToString("C", Util.Cultures.VietNam), stt = stt }, JsonRequestBehavior.AllowGet);

        }
    }
}