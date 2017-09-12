using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;
using System.IO;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;

namespace NDHSITE.Controllers
{
    public class CheckInController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();

        public ActionResult ShowCalendar(int? page, int? month, int? year)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");


            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (month == null)
                month = DateTime.Now.Month;

            if (year == null)
                year = DateTime.Now.Year;

            ViewBag.Month = month;
            ViewBag.Year = year;

            var listCalendar = db.CalendarInfoes.Where(p => p.CMonth == month && p.CYear == year).ToList();

            return View(listCalendar.OrderBy(p => p.CStatus).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ShowCalendarDetail(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");

            var calendar = db.CalendarInfoes.Find(id);
            var detail = db.checkin_getcalendar(calendar.CMonth, calendar.CYear, calendar.StaffId).OrderBy(p => p.CDay).ToList();
            ViewBag.GroupType = db.checkin_calendartype_group(calendar.CMonth, calendar.CYear, calendar.StaffId).ToList();
            ViewBag.Calendar = calendar;
            return View(detail);

        }

        [HttpPost]
        public ActionResult ApproveCalendar(string id, string notes)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");
            var calendar = db.CalendarInfoes.Find(id);
            if (calendar != null)
            {
                calendar.Notes = notes;
                calendar.CStatus = 1;
                db.Entry(calendar).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("showcalendardetail", "checkin", new { id = id });
        }

        public ActionResult ExcelPlanReport(int month, int year, string staffId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");
            int days = DateTime.DaysInMonth(year, month);

            HaiStaff staff = db.HaiStaffs.Find(staffId);

            if (staff == null)
                return RedirectToAction("error", "home");

            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            

            try
            {
                FileInfo newFile = new FileInfo(pathTo);
                if (newFile.Exists)
                {
                    newFile.Delete(); // ensures we create a new workbook
                    newFile = new FileInfo(pathTo);
                }
                var maxday = new SqlParameter("@maxday", SqlDbType.Int);

                maxday.Value = days;
                var monthParams = new SqlParameter("@month", SqlDbType.VarChar);
                monthParams.Value = month;
                var yearParams = new SqlParameter("@year", year);
                var staffParams = new SqlParameter("@staffId", staffId);
                var data = db.Database.SqlQuery<checkin_report_plan_result>("checkin_report_plan @maxday,@month,@year,@staffId", maxday, monthParams, yearParams, staffParams).ToList();
                var sumary = db.checkin_calendartype_group(month, year, staffId).ToList();
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(staff.FullName);
                  
                    worksheet.Cells[1, 2].Value = "KẾ HOẠCH CHĂM SÓC KHÁCH HÀNG";
                    worksheet.Cells[1, 2].Style.Font.Bold = true;
                    worksheet.Cells[3, 2].Value = "Chi nhánh: " + staff.HaiBranch.Code;
                    worksheet.Cells[4, 2].Value = "Tên NV: " + staff.FullName;
                    worksheet.Cells[5, 2].Value = "Mã Nv: " + staff.Code;
                    worksheet.Cells[6, 2].Value = "Khu vực: " + staff.HaiBranch.HaiArea.Name;

                   
                    for (var i = 0; i < sumary.Count(); i++)
                    {
                        worksheet.Cells[i + 1, 9].Value = sumary[i].Name + ": " + sumary[i].countday;
                    }


                    var tableIndex = 6;
                    if (tableIndex < sumary.Count)
                        tableIndex = sumary.Count;

                    tableIndex += 2;

                    // 
                    worksheet.Cells[tableIndex, 1].Value = "STT";
                    worksheet.Cells[tableIndex, 1].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 2].Value = "CỤM";
                    worksheet.Cells[tableIndex, 2].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 3].Value = "MÃ KH";
                    worksheet.Cells[tableIndex, 3].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 4].Value = "TÊN KH";
                    worksheet.Cells[tableIndex, 4].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 5].Value = "TÊN CỬA HÀNG";
                    worksheet.Cells[tableIndex, 5].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 6].Value = "CN";
                    worksheet.Cells[tableIndex, 6].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 7].Value = "ĐỊA CHỈ";
                    worksheet.Cells[tableIndex, 7].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 8].Value = "HUYỆN";
                    worksheet.Cells[tableIndex, 8].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 9].Value = "TỈNH";
                    worksheet.Cells[tableIndex, 9].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 10].Value = "CẤP 1";
                    worksheet.Cells[tableIndex, 10].Style.Font.Bold = true;
                    worksheet.Cells[tableIndex, 11].Value = "MIÊU TẢ";
                    worksheet.Cells[tableIndex, 11].Style.Font.Bold = true;

                    for (int i = 0; i< days; i++)
                    {
                        worksheet.Cells[tableIndex, i + 12].Value = i + 1;
                        worksheet.Cells[tableIndex, i + 12].Style.Font.Bold = true;
                    }

                    // general data
                    tableIndex++;
                    var agency = "";
                    var agencyCount = 1;
                    for(int i= 0; i< data.Count(); i++)
                    {
                        if (agency != data[i].AgencyCode)
                        {
                            agency = data[i].AgencyCode;

                            var c2 = db.C2Info.Where(p => p.Code == agency).FirstOrDefault();

                            if (c2 != null)
                            {
                                worksheet.Cells[tableIndex, 1].Value = agencyCount;
                                agencyCount++;
                                worksheet.Cells[tableIndex, 2].Value = "";
                                worksheet.Cells[tableIndex, 3].Value = agency;
                                worksheet.Cells[tableIndex, 4].Value = c2.Deputy;
                                worksheet.Cells[tableIndex, 5].Value = c2.StoreName;
                                worksheet.Cells[tableIndex, 6].Value = c2.CInfoCommon.BranchCode;
                                worksheet.Cells[tableIndex, 7].Value = c2.CInfoCommon.AddressInfo;
                                worksheet.Cells[tableIndex, 8].Value = c2.CInfoCommon.DistrictName;
                                worksheet.Cells[tableIndex, 9].Value = c2.CInfoCommon.ProvinceName;
                                worksheet.Cells[tableIndex, 10].Value = c2.C1Info.Code;
                            }

                        }
                        worksheet.Cells[tableIndex, 11].Value = data[i].CType;

                        for (int j = 0; j < days; j++)
                        {
                            worksheet.Cells[tableIndex, j + 12].Value = data[i].getValue(j+1);
                        }


                        tableIndex++;
                    }
                  
                    package.Save();
                }

            }
            catch(Exception e)
            {
                return RedirectToAction("error", "home");
            }


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ke-hoach-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }
    }
}