using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using System.Data.Entity;
using PagedList;
using OfficeOpenXml;
using System.IO;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class HaiReportController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        public ActionResult ChooseEvent(string DateFrom = null, string DateTo = null, string search = "", int? page = 1)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;
            if (DateFrom != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }

            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;
            ViewBag.SearchText = search;

            var allEvent = (from log in db.EventInfoes
                            where DbFunctions.TruncateTime(log.BeginTime)
                                               >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.BeginTime)
                                               <= DbFunctions.TruncateTime(dTo) && log.Name.Contains(search)
                            select log).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize);

            return View(allEvent);

        }


        public ActionResult AgencyReport(string Ctype, string EventId, int? page)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            var checkEvent = db.EventInfoes.Find(EventId);
            if (checkEvent == null)
                return RedirectToAction("error", "home");

            int pageSize = 20;

            ViewBag.CType = Ctype;
            ViewBag.EventInfo = checkEvent;

            int pageNumber = (page ?? 1);
            var data = db.report_event_agency(Ctype, EventId).ToList();

            return View(data.OrderBy(p => p.CName).ToPagedList(pageNumber, pageSize));
        }


        public ActionResult ExportAgencyReport(string Ctype, string EventId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/agencyreport.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);
                var data = db.report_event_agency(Ctype, EventId).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["DSNT"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = i + 1;
                            worksheet.Cells[i + 2, 2].Value = data[i].CCode;
                            worksheet.Cells[i + 2, 3].Value = data[i].CName;
                            worksheet.Cells[i + 2, 4].Value = data[i].CDeputy;
                            worksheet.Cells[i + 2, 5].Value = data[i].IdentityCard;

                            worksheet.Cells[i + 2, 7].Value = data[i].DistrictName;
                            worksheet.Cells[i + 2, 8].Value = data[i].ProvinceName;
                            worksheet.Cells[i + 2, 9].Value = data[i].BranchCode;
                            worksheet.Cells[i + 2, 10].Value = data[i].Phone;
                            worksheet.Cells[i + 2, 11].Value = data[i].PName;
                            worksheet.Cells[i + 2, 12].Value = data[i].quantity;
                            worksheet.Cells[i + 2, 13].Value = data[i].Point;
                            worksheet.Cells[i + 2, 14].Value = data[i].AllPoint;
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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-khuyen-mai-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }


        // report
        public ActionResult ProductRemain(int? page, string Code, string DateTo, string DateFrom)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            if (String.IsNullOrEmpty(Code))
                Code = "AX";

            var product = db.ProductInfoes.Where(p => p.Barcode == Code).FirstOrDefault();

            if (product == null)
                return RedirectToAction("error", "home");


            int pageSize = 10;
            int pageNumber = (page ?? 1);
            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;

            string strDateFrom;
            string strDateTo;

            if (!String.IsNullOrEmpty(DateTo) && !String.IsNullOrEmpty(DateTo))
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }
           

            strDateFrom = dFrom.ToString("yyyy-MM-dd");
            strDateTo = dTo.ToString("yyyy-MM-dd");


            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;
            ViewBag.Code = Code;
            ViewBag.Product = product;

            var listRemain = db.report_remain_product(Code, strDateFrom, strDateTo).ToList();



            return View(listRemain.OrderBy(p => p.WCode).ToPagedList(pageNumber, pageSize));
        }


        public ActionResult ExportProductRemain(string Code, string DateTo, string DateFrom)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/productremain.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_remain_product(Code,DateFrom, DateTo).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Report"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = data[i].WCode;
                            worksheet.Cells[i + 2, 2].Value = data[i].WName;

                            if (data[i].WType == "W")
                            {
                                worksheet.Cells[i + 2, 3].Value = "Kho";
                            } else if (data[i].WType == "B")
                            {
                                worksheet.Cells[i + 2, 3].Value = "Chi nhánh";
                            }
                            else if (data[i].WType == "CI")
                            {
                                worksheet.Cells[i + 2, 3].Value = "Đại lý cấp 1";
                            }
                            else if (data[i].WType == "CII")
                            {
                                worksheet.Cells[i + 2, 3].Value = "Đại lý cấp 2";
                            }
                            else if (data[i].WType == "FARMER")
                            {
                                worksheet.Cells[i + 2, 3].Value = "Nông dân";
                            }
                            worksheet.Cells[i + 2, 4].Value = data[i].countNK;
                            worksheet.Cells[i + 2, 5].Value = data[i].countXK;
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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ton-kho-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }


        // report
        public ActionResult WarehouseRemain(int? page, string Code, string DateTo, string DateFrom, List<string> products)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            if (String.IsNullOrEmpty(Code))
                Code = "HAI";

            var branch = db.HaiBranches.Where(p => p.Code == Code).FirstOrDefault();

            string wCode = "";
            string wName = "";
            string wType = "";

            if (branch != null)
            {
                wCode = branch.Code;
                wType = "Chi nhánh";
                wName = branch.Name;
            }
            else
            {
                var cinfo = db.CInfoCommons.Where(p => p.CCode == Code).FirstOrDefault();

                if (cinfo != null)
                {
                    wCode = cinfo.CCode;
                   
                    wName = cinfo.CName;

                    if (cinfo.CType == "CI")
                        wType = "Đại lý cấp 1";
                    else if (cinfo.CType == "CII")
                        wType = "Đại lý cấp 2";
                    else if (cinfo.CType == "FARMER")
                        wType = "Nông dân";
                }
            }

            if (String.IsNullOrEmpty(wCode))
                return RedirectToAction("error", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;

            string strDateFrom;
            string strDateTo;

            if (!String.IsNullOrEmpty(DateTo) && !String.IsNullOrEmpty(DateTo))
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }


            strDateFrom = dFrom.ToString("yyyy-MM-dd");
            strDateTo = dTo.ToString("yyyy-MM-dd");


            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;
            ViewBag.Code = Code;
            ViewBag.WName = wName;
            ViewBag.WType = wType;

            var listRemain = db.report_remain_wcode(Code, strDateFrom, strDateTo).ToList();

            return View(listRemain.OrderBy(p => p.ProductCode).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ExportWarehouseRemain( string Code, string DateTo, string DateFrom)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/warehouseremain.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_remain_wcode(Code, DateFrom, DateTo).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Report"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = data[i].ProductCode;
                            worksheet.Cells[i + 2, 2].Value = data[i].PName;
                            worksheet.Cells[i + 2, 3].Value = data[i].countNK;
                            worksheet.Cells[i + 2, 4].Value = data[i].countXK;
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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ton-kho-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }


        public ActionResult RemainProductDetail(int? page ,string DateTo, string DateFrom)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "RemainProductDetail", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;

            string strDateFrom;
            string strDateTo;

            if (!String.IsNullOrEmpty(DateTo) && !String.IsNullOrEmpty(DateTo))
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }

            strDateFrom = dFrom.ToString("yyyy-MM-dd");
            strDateTo = dTo.ToString("yyyy-MM-dd");

            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;

            var listRemain = db.report_cii_product(strDateFrom, strDateTo).ToList();


            return View(listRemain.OrderBy(p=> p.ctime).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ExcelRemainProductDetail(string DateTo, string DateFrom)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "RemainProductDetail", 0))
                return RedirectToAction("relogin", "home");

            var data = db.report_cii_product(DateFrom, DateTo).ToList();

            string pathRoot = Server.MapPath("~/haiupload/barcodereportdeatail.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
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
                            worksheet.Cells[i + 2, 1].Value = data[i].CaseCode;
                            worksheet.Cells[i + 2, 2].Value = data[i].branchExport;
                            worksheet.Cells[i + 2, 3].Value = data[i].C1;
                            worksheet.Cells[i + 2, 4].Value = data[i].WareHouse;
                            worksheet.Cells[i + 2, 5].Value = data[i].WareHouseName;
                            worksheet.Cells[i + 2, 6].Value = data[i].branch;
                            worksheet.Cells[i + 2, 8].Value = data[i].Staff;
                            worksheet.Cells[i + 2,10].Value = data[i].PName;
                            worksheet.Cells[i + 2, 11].Value = data[i].Quantity;
                            worksheet.Cells[i + 2, 12].Value = data[i].BoxPoint;
                            worksheet.Cells[i + 2, 13].Value = data[i].ctime;
                            worksheet.Cells[i + 2, 14].Value = data[i].staffhelp;
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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ton-kho-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }

        public ActionResult ReportOrder()
        {
            ViewBag.Month = DateTime.Now.Month;
            ViewBag.Year = DateTime.Now.Year;
            ViewBag.Day = DateTime.Now.Day;

            return View();
        }


        public ActionResult ReportOrderDetail (int month, int year, int fDay, int tDay)
        {
           

            string pathRoot = Server.MapPath("~/haiupload/report_order_detail.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_order_detail(year + "-" + month + "-" + fDay, year + "-" + month + "-" + tDay).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = i + 1;
                            worksheet.Cells[i + 2, 2].Value = data[i].CreateDate;
                            worksheet.Cells[i + 2, 3].Value = data[i].OrderCode;
                            worksheet.Cells[i + 2, 4].Value = data[i].StaffCode;
                            worksheet.Cells[i + 2, 5].Value = data[i].StaffName;
                            worksheet.Cells[i + 2, 6].Value = data[i].C2Code;
                            worksheet.Cells[i + 2, 7].Value = data[i].SMSCode;
                            worksheet.Cells[i + 2, 8].Value = data[i].StoreName;
                            worksheet.Cells[i + 2, 9].Value = data[i].OderType;
                            worksheet.Cells[i + 2, 10].Value = data[i].C1Code;
                            worksheet.Cells[i + 2, 11].Value = data[i].C1Name;
                            worksheet.Cells[i + 2, 12].Value = data[i].ProductCode;
                            worksheet.Cells[i + 2, 13].Value = data[i].ProductName;
                            worksheet.Cells[i + 2, 14].Value = data[i].Unit;
                            worksheet.Cells[i + 2, 15].Value = data[i].Quantity;
                            worksheet.Cells[i + 2, 16].Value = data[i].PerPrice;
                            worksheet.Cells[i + 2, 17].Value = data[i].QuantityBuy/data[i].Quantity;
                            worksheet.Cells[i + 2, 18].Value = data[i].PriceTotal;
                            if (data[i].QuantityBuy > data[i].QuantityFinish)
                            {
                                worksheet.Cells[i + 2, 19].Value = "Đang giao";
                            } else
                            {
                                worksheet.Cells[i + 2, 19].Value = "Đã giao";
                            }
                            worksheet.Cells[i + 2, 21].Value = data[i].QuantityFinish / data[i].Quantity;
                            worksheet.Cells[i + 2, 22].Value = data[i].QuantityFinish * data[i].PerPrice;
                        }
                        catch
                        {

                        }

                    }
                    package.Save();
                }
            }
            catch
            {
                return RedirectToAction("error", "home");
            }

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-don-hang-chi-tiet-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }
    }
}