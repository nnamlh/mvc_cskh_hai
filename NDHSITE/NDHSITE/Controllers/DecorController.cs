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
    [Authorize]
    public class DecorController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();
        // GET: Decor
        public ActionResult Show(int? page, int? month, int? year, string group = "TRUNGBAY", string branch = "", string agency = "")
        {
            ViewBag.GroupDecor = db.DecorGroups.ToList();

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            if (month == null)
                month = DateTime.Now.Month;

            if (year == null)
                year = DateTime.Now.Year;

            ViewBag.GroupChoose = group;
            ViewBag.Month = month;
            ViewBag.Branch = branch;
            ViewBag.Year = year;
            ViewBag.Agency = agency;

            branch = "%" + branch + "%";
            agency = "%" + agency + "%";

            var data = db.get_decor_info(branch, agency, group, month, year).ToList();

            return View(data.OrderByDescending(p => p.DDay).ToPagedList(pageNumber, pageSize));
        }



        public ActionResult ReportDetail(int? month, int? year, string group = "TRUNGBAY", string branch = "", string agency = "")
        {
            if (month == null)
                month = DateTime.Now.Month;

            if (year == null)
                year = DateTime.Now.Year;

            branch = "%" + branch + "%";
            agency = "%" + agency + "%";

            string pathRoot = Server.MapPath("~/haiupload/report-decor-detail.xlsx");
            string name = "report-decor-" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);
 
            System.IO.File.Copy(pathRoot, pathTo);

            FileInfo newFile = new FileInfo(pathTo);
            

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["hai"];
                var data = db.get_decor_info(branch, agency, group, month, year).ToList();

                for (int i = 0; i < data.Count; i++)
                {

                    try
                    {
                        worksheet.Cells[i + 2, 1].Value = data[i].DDay + "/" + data[i].DMonth + "/" + data[i].DYear;
                        worksheet.Cells[i + 2, 2].Value = data[i].DTime;
                        worksheet.Cells[i + 2, 3].Value = data[i].StaffCode;
                        worksheet.Cells[i + 2, 4].Value = data[i].StaffName;
                        worksheet.Cells[i + 2, 5].Value = data[i].StaffBranch;
                        worksheet.Cells[i + 2, 6].Value = data[i].Agency;
                        worksheet.Cells[i + 2, 7].Value = data[i].AgencyName;
                        worksheet.Cells[i + 2, 8].Value = data[i].BranchCode;
                        worksheet.Cells[i + 2, 9].Value = data[i].AddressInfo;
                        worksheet.Cells[i + 2, 10].Value = data[i].DistrictName;
                        worksheet.Cells[i + 2, 11].Value = data[i].ProvinceName;

                    }
                    catch
                    {
                        return RedirectToAction("error", "home");
                    }
                }
                package.Save();
            }

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-decor-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }
    }
}