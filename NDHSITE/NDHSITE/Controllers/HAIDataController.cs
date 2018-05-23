using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class HAIDataController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        // GET: HAIData
        public ActionResult UpdateC1Staff(string search = "")
        {

            if (!String.IsNullOrEmpty(search))
            {

                var check = db.find_staff_c1(search);

            }

            return View();
        }

        [HttpPost]
        public ActionResult UpdateC1Staff(string CI, string Staff)
        {

            CI = CI.Replace("\r", "");

            string[] listCI = Regex.Split(CI, "\n");

            Staff = Staff.Replace("\r", "");

            string[] listStaff = Regex.Split(Staff, "\n");

            foreach (var itemStaff in listStaff)
            {
                var find = db.HaiStaffs.Where(p => p.Code == itemStaff.Trim()).FirstOrDefault();

                if (find != null)
                {

                    foreach (var itemC1 in listCI)
                    {
                        var checkC1 = db.C1Info.Where(p => p.Code == itemC1.Trim()).FirstOrDefault();

                        if (checkC1 != null)
                        {
                            var checkStaffC1 = find.C1Info.Where(p => p.Id == checkC1.Id).FirstOrDefault();

                            if (checkStaffC1 == null)
                            {
                                find.C1Info.Add(checkC1);
                            }

                        }
                    }


                    db.SaveChanges();
                }
            }

            return View();
        }


        ///
        public ActionResult UpdateC2C1()
        {


            return View();

        }

        [HttpPost]
        public ActionResult UpdateC2C1(HttpPostedFileBase files)
        {
            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {
                    string fileSave = "cii_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
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
                        string c2 = Convert.ToString(sheet.Cells[i, 1].Value);

                        string c11 = Convert.ToString(sheet.Cells[i, 2].Value);

                        string c12 = Convert.ToString(sheet.Cells[i, 3].Value);

                        string[] listC1 = { c11, c12 };
                        var index = 1;
                        var checkC2 = db.C2Info.Where(p => p.Code == c2.Trim()).FirstOrDefault();

                        if (checkC2 != null)
                        {
                            foreach (var item in listC1)
                            {

                                var check = db.C2C1.Where(p => p.C2Code == c2.Trim() && p.C1Code == c11.Trim()).FirstOrDefault();

                                if (check == null)
                                {
                                    var checkC1 = db.C1Info.Where(p => p.Code == item.Trim()).FirstOrDefault();

                                    if (checkC1 != null)
                                    {
                                        var c2c1 = new C2C1()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            C1Code = checkC1.Code,
                                            C2Code = checkC2.Code,
                                            Priority = index == 1 ? 1 : 0,
                                            ModifyDate = DateTime.Now
                                        };

                                        db.C2C1.Add(c2c1);
                                        db.SaveChanges();
                                    }
                                    index++;

                                }
                            }

                        }



                    }
                }

            }

            return View();

        }

    }
}