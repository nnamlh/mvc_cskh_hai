using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using PagedList;

namespace NDHSITE.Controllers
{
    public class BarcodeController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();

        // GET: Barcode

        public ActionResult ChangeKho(string msg)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 1))
                return RedirectToAction("relogin", "home");

            ViewBag.MSG = msg;
            return View();
        }

        public ActionResult Delete(string msg)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 1))
                return RedirectToAction("relogin", "home");
            ViewBag.MSG = msg;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 1))
                return RedirectToAction("relogin", "home");
            //
            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "excel_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                    string path = Server.MapPath("~/temp/" + fileSave);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 1; i <= totalRows; i++)
                    {
                        string barcode = Convert.ToString(sheet.Cells[i, 1].Value);
                        if (!String.IsNullOrEmpty(barcode) && barcode.Length == 17)
                        {
                            var caseCode = barcode.Substring(0, 15);

                            var pHistory = db.PHistories.Where(p => p.CaseCode == caseCode).ToList();

                            foreach (var item in pHistory)
                            {
                                db.PHistories.Remove(item);
                            }
                            db.SaveChanges();

                            var pTracking = db.PTrackings.Where(p => p.CaseCode == caseCode).ToList();
                            foreach (var item in pTracking)
                            {
                                db.PTrackings.Remove(item);
                            }
                            db.SaveChanges();

                            var barcodeHistory = db.BarcodeHistories.Where(p => p.CaseCode == caseCode).ToList();

                            foreach (var item in barcodeHistory)
                            {
                                db.BarcodeHistories.Remove(item);

                            }
                            db.SaveChanges();

                            // save history
                            var saveHistory = new BarcodeChangeHistory()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Barcode = barcode,
                                CaseCode = caseCode,
                                CreateTime = DateTime.Now,
                                InAction = 0,
                                UserName = User.Identity.Name
                            };

                            db.BarcodeChangeHistories.Add(saveHistory);
                            db.SaveChanges();
                        }
                        else
                        {
                            return RedirectToAction("delete", "barcode", new { msg = "Mã sai: " + barcode });
                        }

                    }

                }
            }

            return RedirectToAction("delete", "barcode", new { msg = "Hoàn thành" });
        }


        [HttpPost]
        public ActionResult ChangeKho(HttpPostedFileBase files, string wOld, string wNew)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 1))
                return RedirectToAction("relogin", "home");
            //
            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "excel_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                    string path = Server.MapPath("~/temp/" + fileSave);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    var checkWcode = db.HaiBranches.Where(p => p.Code == wNew).FirstOrDefault();

                    string wtype = "";
                    string wname = "";
                    string wcode = wNew;

                    if (checkWcode != null)
                    {
                        wtype = "B";
                        wname = checkWcode.Name;
                    }
                    else
                    {
                        var cinfo = db.CInfoCommons.Where(p => p.CCode == wNew).FirstOrDefault();
                        if (cinfo != null)
                        {
                            wtype = cinfo.CType;
                            wname = cinfo.CName;
                        }
                    }

                    if (String.IsNullOrEmpty(wtype))
                        return RedirectToAction("error", "home");

                    for (int i = 1; i <= totalRows; i++)
                    {
                        string barcode = Convert.ToString(sheet.Cells[i, 1].Value);
                        if (!String.IsNullOrEmpty(barcode) && barcode.Length == 17)
                        {
                            var caseCode = barcode.Substring(0, 15);

                            var pHistory = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode == wOld).ToList();

                            foreach (var item in pHistory)
                            {
                                item.WCode = wcode;
                                item.WType = wtype;
                                item.WName = wname;
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            var pTracking = db.PTrackings.Where(p => p.CaseCode == caseCode && p.WCode == wOld).ToList();
                            foreach (var item in pTracking)
                            {
                                item.WCode = wcode;
                                item.WType = wtype;
                                item.WName = wname;
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            var barcodeHistory = db.BarcodeHistories.Where(p => p.CaseCode == caseCode && p.WareHouse == wOld).ToList();

                            foreach (var item in barcodeHistory)
                            {
                                item.WareHouse = wcode;
                                item.WareHouseName = wname;
                                item.WareHouseType = wtype;
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }


                            var barcodeRelative = db.BarcodeHistories.Where(p => p.CaseCode == caseCode && p.WareRelative == wOld).ToList();

                            foreach (var item in barcodeRelative)
                            {
                                item.WareRelative = wcode;
                                item.WareRelativeName = wname;
                                item.WareRelativeType = wtype;
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            // save history
                            var saveHistory = new BarcodeChangeHistory()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Barcode = barcode,
                                CaseCode = caseCode,
                                CreateTime = DateTime.Now,
                                InAction = 1,
                                UserName = User.Identity.Name,
                                WChange = wNew,
                                WCode = wcode
                            };

                            db.BarcodeChangeHistories.Add(saveHistory);
                            db.SaveChanges();
                        }
                        else
                        {
                            return RedirectToAction("changekho", "barcode", new { msg = "Mã sai: " + barcode });
                        }

                    }

                }
            }

            return RedirectToAction("changekho", "barcode", new { msg = "Hoàn thành" });
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



        public ActionResult ReportTrackingDetail()
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 0))
                return RedirectToAction("relogin", "home");
            return View();
        }


        public ActionResult ExcelReportTrackingDetail(string DateTo, string DateFrom)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "Barcode", 0))
                return RedirectToAction("relogin", "home");

            List<string> dsB5 = new List<string>();
            dsB5.Add("1AGG000009");
            dsB5.Add("1AGG000012");
            dsB5.Add("1KGG000011");
            dsB5.Add("1LAN000009");
            dsB5.Add("1HGG000006");

            string pathRoot = Server.MapPath("~/haiupload/report-tracking-barcodev2.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);


            if (!String.IsNullOrEmpty(DateTo) && !String.IsNullOrEmpty(DateTo))
            {
                DateTime dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                DateTime dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);


                var allBarcode = (from log in db.PTrackings
                                  where DbFunctions.TruncateTime(log.ImportTime)
                                                     >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.ImportTime)
                                                     <= DbFunctions.TruncateTime(dTo)
                                  orderby log.ImportTime
                                  select log.CaseCode).Distinct().ToList();

                FileInfo newFile = new FileInfo(pathTo);

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; ;

                    for (int i = 0; i < allBarcode.Count(); i++)
                    {

                        // get all tracking
                        string caseCode = allBarcode[i];
                        var trackingInfo = db.PTrackings.Where(p => p.CaseCode == caseCode).ToList();
                        var history = db.BarcodeHistories.Where(p => p.CaseCode == caseCode && p.IsSuccess == 1).ToList();

                        // kho
                        var kho = trackingInfo.Where(p => p.WType == "W").FirstOrDefault();

                        if (kho != null)
                        {
                            // import excel
                            worksheet.Cells[i + 5, 1].Value = kho.CaseCode;
                            worksheet.Cells[i + 5, 2].Value = kho.ProductInfo.PName;

                            if (kho.ImportTime == null)
                            {
                                worksheet.Cells[i + 5, 3].Value = "CHƯA NHẬP KHO";
                            }
                            else
                            {
                                worksheet.Cells[i + 5, 3].Value = kho.ImportTime;
                            }

                            if (kho.ExportTime == null)
                            {
                                worksheet.Cells[i + 5, 4].Value = "CHƯA XUẤT KHO";
                            }
                            else
                            {
                                worksheet.Cells[i + 5, 4].Value = kho.ExportTime;

                                worksheet.Cells[i + 5, 5].Value = kho.WCode;

                                // tim kho nhan
                                var barcodeHistory = history.Where(p => p.PStatus == "XK" && p.WareHouse == kho.WCode).FirstOrDefault();
                                if (barcodeHistory != null)
                                {
                                    worksheet.Cells[i + 5, 6].Value = barcodeHistory.WareRelative;
                                }
                            }


                        }

                        // chi nhanh
                        var chiNhanh = trackingInfo.Where(p => p.WType == "B").FirstOrDefault();

                        if (chiNhanh != null)
                        {
                            if (chiNhanh.ExportTime == null)
                            {
                                worksheet.Cells[i + 5, 7].Value = "CHƯA XUẤT KHO";
                                worksheet.Cells[i + 5, 8].Value = chiNhanh.WCode;
                            }
                            else
                            {
                                //worksheet.Cells[i + 4, 7].Value = chiNhanh.ExportTime;
                                var barcodeHistory = history.Where(p => p.PStatus == "XK" && p.WareHouse == chiNhanh.WCode).FirstOrDefault();
                                if (barcodeHistory != null)
                                {
                                    // kiem tra co phai B5
                                    if (dsB5.Contains(barcodeHistory.WareRelative))
                                    {
                                        // la b5
                                        worksheet.Cells[i + 5, 7].Value = chiNhanh.ExportTime;
                                        worksheet.Cells[i + 5, 8].Value = chiNhanh.WCode;
                                        worksheet.Cells[i + 5, 9].Value = barcodeHistory.WareRelative;
                                        worksheet.Cells[i + 5, 10].Value = barcodeHistory.WareRelativeName;
                                    }
                                    else
                                    {
                                        worksheet.Cells[i + 5, 11].Value = chiNhanh.ExportTime;
                                        worksheet.Cells[i + 5, 12].Value = chiNhanh.WCode;
                                        worksheet.Cells[i + 5, 13].Value = barcodeHistory.WareRelative;
                                        worksheet.Cells[i + 5, 14].Value = barcodeHistory.WareRelativeName;
                                    }
                                }
                            }
                        }

                        // lay c2
                        var c2 = trackingInfo.Where(p => p.WType == "CII").FirstOrDefault();
                        if (c2 != null)
                        {
                            // barcode history
                            var barcodeHistory = history.Where(p => p.PStatus == "NK" && p.StaffHelp != null && p.WareHouse == c2.WCode).FirstOrDefault();
                            worksheet.Cells[i + 5, 19].Value = c2.WCode;
                            worksheet.Cells[i + 5, 20].Value = c2.WName;
                            worksheet.Cells[i + 5, 15].Value = c2.ImportTime;

                            var checkC2 = db.C2Info.Where(p => p.Code == c2.WCode.Trim()).FirstOrDefault();
                            if (checkC2 != null)
                            {
                                worksheet.Cells[i + 5, 21].Value = checkC2.CInfoCommon.BranchCode;
                            }

                            // 
                            if (barcodeHistory != null)
                            {
                                worksheet.Cells[i + 5, 16].Value = barcodeHistory.StaffHelp;
                                var haiStaff = db.HaiStaffs.Where(p => p.Code == barcodeHistory.StaffHelp).FirstOrDefault();
                                if (haiStaff != null)
                                {
                                    worksheet.Cells[i + 5, 17].Value = haiStaff.FullName;
                                    worksheet.Cells[i + 5, 18].Value = haiStaff.HaiBranch.Code;
                                }
                            }

                            // kiem tra co c2 nao quet nua ko
                            var checkC2Other = db.BarcodeHistories.Where(p => p.CaseCode == caseCode && p.IsSuccess == 0 && p.WareHouse == c2.WCode && p.WareHouseType == "CII").FirstOrDefault();
                            if (checkC2Other != null)
                            {
                                worksheet.Cells[i + 5, 22].Value = checkC2Other.StaffHelp;
                                var haiStaffHelpC2Other = db.HaiStaffs.Where(p => p.Code == checkC2Other.StaffHelp).FirstOrDefault();
                                if (haiStaffHelpC2Other != null)
                                {
                                    worksheet.Cells[i + 5, 23].Value = haiStaffHelpC2Other.FullName;
                                    worksheet.Cells[i + 5, 24].Value = haiStaffHelpC2Other.HaiBranch.Code;
                                }
                                var c2Other = db.C2Info.Where(p => p.Code == checkC2Other.WareHouse.Trim()).FirstOrDefault();
                                worksheet.Cells[i + 5, 25].Value = checkC2Other.WareHouse;
                                worksheet.Cells[i + 5, 26].Value = checkC2Other.WareHouseName;
                                if (c2Other != null)
                                {
                                    worksheet.Cells[i + 5, 27].Value = c2Other.CInfoCommon.BranchCode;
                                }

                            }

                            // kiem tra co tham gia chuong tỉnh ko
                            var checkPermiss = db.BarcodeNotPermisses.Where(p => p.CaseCode == c2.CaseCode).FirstOrDefault();
                            if(checkPermiss != null)
                            {
                                worksheet.Cells[i + 5, 28].Value = "Không được tham gia";
                                worksheet.Cells[i + 5, 29].Value = checkPermiss.Notes;

                            }
                        }

                    }

                    package.Save();

                }

            }

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ton-kho-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }


        // add barcode not permit
        public ActionResult BarcodeNotPermiss(int? page, string barcode = "")
        {

            // get list barcode not permiss
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            ViewBag.Barcode = barcode;

            ViewBag.Branch = db.HaiBranches.ToList();

            var barcodes = db.BarcodeNotPermisses.Where(p => p.Barcode.Contains(barcode)).OrderByDescending(p => p.CreateTime).ToPagedList(pageNumber, pageSize);

            return View(barcodes);
        }

        public ActionResult DeleteBarcodeNotPermiss(string barcode)
        {
            if (!String.IsNullOrEmpty(barcode) && barcode.Length == 17)
            {
                var caseCode = barcode.Substring(0, 15);

                var check = db.BarcodeNotPermisses.Where(p => p.CaseCode == caseCode).FirstOrDefault();

                if(check != null)
                {
                    db.BarcodeNotPermisses.Remove(check);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("BarcodeNotPermiss", "Barcode");

        }

        [HttpPost]
        public ActionResult BarcodeNotPermiss(string branch, HttpPostedFileBase files)
        {

            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {
                    string fileSave = "excel_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                    string path = Server.MapPath("~/temp/" + fileSave);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string barcode = Convert.ToString(sheet.Cells[i, 1].Value);
                        string notes = Convert.ToString(sheet.Cells[i, 2].Value);
                        if (!String.IsNullOrEmpty(barcode) && barcode.Length == 17)
                        {
                            var caseCode = barcode.Substring(0, 15);
                            string productCode = barcode.Substring(8, 2);

                            var product = db.ProductInfoes.Where(p => p.Barcode == productCode).FirstOrDefault();

                            var check = db.BarcodeNotPermisses.Where(p => p.CaseCode == caseCode).FirstOrDefault();

                            if (check == null)
                            {
                                var save = new BarcodeNotPermiss()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Barcode = barcode,
                                    CaseCode = caseCode,
                                    CreateTime = DateTime.Now,
                                    BranchCode = branch,
                                    Notes = notes,
                                    ProductBarcode = productCode,
                                    UserUpload = User.Identity.Name
                                };

                                if (product != null)
                                    save.ProductName = product.PName;

                                db.BarcodeNotPermisses.Add(save);
                                db.SaveChanges();
                            }

                        }
                    }
                }
            }

            return RedirectToAction("BarcodeNotPermiss", "Barcode");
        }
    }
}