using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{
    public class TrackingController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();


        public ActionResult ProductTracking(string barcode)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Tracking", 0))
                return RedirectToAction("relogin", "home");

            var product = GetProduct(barcode);
            ViewBag.Barcode = barcode;
            if (product != null)
            {
                ViewBag.Product = product;
                var boxCode = barcode.Substring(0, 15);

                List<PTrackingInfo> productTracking = new List<PTrackingInfo>();

                var pHistoryTracking = db.PTrackings.Where(p => p.CaseCode == boxCode).OrderBy(p => p.ImportTime).ToList();


                foreach (var item in pHistoryTracking)
                {
                    PTrackingInfo pTracking = new PTrackingInfo();

                    if (item.WType == "CI")
                        pTracking.name = "CẤP 1: " + item.WName;
                    else if (item.WType == "CII")
                        pTracking.name = "CẤP 2: " + item.WName;
                    else if (item.WType == "B")
                        pTracking.name = "CHI NHÁNH: " + item.WName;
                    else if (item.WType == "FARMER")
                        pTracking.name = "NÔNG DÂN: " + item.WName;
                    else if (item.WType == "W")
                        pTracking.name = "TỔNG KHO: " + item.WName;

                    if (item.ImportTime != null)
                        pTracking.importTime = "Nhập kho lúc " + item.ImportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.importTime = "Chưa nhập kho";

                    if (item.ExportTime != null)
                        pTracking.exportTime = "Xuất kho lúc " + item.ExportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.exportTime = "Chưa xuất kho";


                    if (item.Quantity == 1 || item.Quantity == 0)
                    {
                        pTracking.status = Convert.ToInt32(item.Quantity) + " thùng";
                    }
                    else
                    {
                        int quantity = Convert.ToInt32(item.Quantity * item.ProductInfo.QuantityBox);
                        pTracking.status = quantity + " hộp";
                    }

                    productTracking.Add(pTracking);
                }

                ViewBag.Tracking = productTracking;
            }



            return View();
        }


        public ActionResult HistoryTracking(int? page, int? type, string code)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Tracking", 0))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            // 1:ma san pham
            // theo ma kho
            // theo mabarcode
            if (type == null)
                type = 1;


            ViewBag.Type = type;
            ViewBag.Code = code;

            switch (type)
            {
                case 1:
                    var history = db.PHistories.Where(p => p.ProductCode.Contains(code)).ToList();
                    return View(history.OrderBy(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 2:
                    history = db.PHistories.Where(p => p.WCode.Contains(code)).ToList();
                    return View(history.OrderBy(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 3:
                    history = db.PHistories.Where(p => p.CaseCode.Contains(code)).ToList();
                    return View(history.OrderBy(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
            }

            return View();
        }


        private ProductInfo GetProduct(string barcode)
        {

            if (String.IsNullOrEmpty(barcode))
                return null;

            if (barcode.Length < 17)
                return null;


            string countryCode = barcode.Substring(0, 3);
            if (countryCode != "893")
                return null;

            string companyCode = barcode.Substring(3, 5);
            if (companyCode != "52433")
                return null;

            string productCode = barcode.Substring(8, 2);

            var product = db.ProductInfoes.Where(p => p.Barcode == productCode).FirstOrDefault();

            return product;

        }

        public ActionResult ExcelHistory(int? type, string code)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Tracking", 0))
                return RedirectToAction("relogin", "home");
            string pathRoot = Server.MapPath("~/haiupload/historytracking.xlsx");
            string name = "historytracking" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                List<PHistory> data = new List<PHistory>();

                switch (type)
                {
                    case 1:
                        data = db.PHistories.Where(p => p.ProductCode.Contains(code)).ToList();
                        break;
                    case 2:
                        data = db.PHistories.Where(p => p.WCode.Contains(code)).ToList();
                        break;

                    case 3:
                        data = db.PHistories.Where(p => p.CaseCode.Contains(code)).ToList();
                        break;

                    default:
                        return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-seri" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

                }

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["HAI"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = i + 1;
                            worksheet.Cells[i + 2, 2].Value = data[i].Barcode;
                            worksheet.Cells[i + 2, 3].Value = data[i].WName;
                            worksheet.Cells[i + 2, 4].Value = data[i].WCode;


                            string wtype = "";
                            if (data[i].WType == "CI")
                                wtype = "CẤP 1";
                            else if (data[i].WType == "CII")
                                wtype = "CẤP 2";

                            else if (data[i].WType == "B")
                                wtype = "CHI NHÁNH";
                            else if (data[i].WType == "FARMER")
                                wtype = "NÔNG DÂN";
                            else if (data[i].WType == "W")
                                wtype = "TỔNG KHO";

                            worksheet.Cells[i + 2, 5].Value = wtype;

                            worksheet.Cells[i + 2, 6].Value = data[i].ProductCode;

                            worksheet.Cells[i + 2, 7].Value = data[i].PStatus;
                            worksheet.Cells[i + 2, 8].Value = data[i].Quantity;
                            worksheet.Cells[i + 2, 9].Value = data[i].CreateDate.Value.ToString("dd/MM/yyyy HH:mm");
                            worksheet.Cells[i + 2, 10].Value = data[i].UserSend;
                            worksheet.Cells[i + 2, 11].Value = data[i].CaseCode;
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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-seri" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));
        }
    }
}