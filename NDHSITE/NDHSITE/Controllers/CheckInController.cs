﻿using System;
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

        public ActionResult ShowCalendar(int? page, int? month, int? year, int status = -1, string branch = "", string staff = "")
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");


            int pageSize = 30;
            int pageNumber = (page ?? 1);

            if (month == null)
                month = DateTime.Now.Month;

            if (year == null)
                year = DateTime.Now.Year;

            ViewBag.Month = month;
            ViewBag.Year = year;
            ViewBag.Status = status;

            var listCalendar = new List<CalendarInfo>();


            List<string> branches = new List<string>();
            int permiss = Utitl.CheckRoleShowInfo(db, User.Identity.Name);
            if (permiss == 2)
            {
                branches = Utitl.GetBranchesPermiss(db, User.Identity.Name, false);
            }
            else if (permiss == 1)
            {
                branches = Utitl.GetBranchesPermiss(db, User.Identity.Name, true);
            }

            if (!String.IsNullOrEmpty(branch))
            {

                if (branches.Contains(branch))
                {
                    branches.Clear();
                    branches.Add(branch);
                }
                else
                {
                    branches.Clear();
                }
            }

            ViewBag.Branch = branch;
            ViewBag.Staff = staff;

            if (status == -1)
            {
                listCalendar = db.CalendarInfoes.Where(p => p.CMonth == month && p.CYear == year && p.HaiStaff.Code.Contains(staff) && branches.Contains(p.HaiStaff.HaiBranch.Code)).ToList();
            }
            else
            {
                listCalendar = db.CalendarInfoes.Where(p => p.CMonth == month && p.CYear == year && p.CStatus == status && p.HaiStaff.Code.Contains(staff) && branches.Contains(p.HaiStaff.HaiBranch.Code)).ToList();
            }

            return View(listCalendar.OrderBy(p => p.CStatus).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult RemoveCalendar(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 1))
                return RedirectToAction("relogin", "home");

            var check = db.CalendarInfoes.Find(id);

            if (check == null || check.CStatus != 0)
                return RedirectToAction("error", "home");

            var actionRemove = db.calendar_remove(check.StaffId, check.CMonth, check.CYear);

            return RedirectToAction("showcalendar", "checkin");

        }

        public ActionResult ShowCalendarDetail(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");

            var calendar = db.CalendarInfoes.Find(id);
            var detail = db.checkin_getcalendar(calendar.CMonth, calendar.CYear, calendar.StaffId).OrderBy(p => p.CDay).ToList();
            ViewBag.GroupType = db.checkin_calendartype_group(calendar.CMonth, calendar.CYear, calendar.StaffId).ToList();
            ViewBag.Calendar = calendar;

            ViewBag.AgencyReport = db.checkin_count_day_agency(calendar.CMonth, calendar.CYear, calendar.StaffId).OrderBy(p => p.AgencyCode).ToList();

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


        public ActionResult ExcelCheckInDetail(int month, int year, string brand = "", string staff = "")
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");

            if (!String.IsNullOrEmpty(staff))
            {
                var checkStaff = db.HaiStaffs.Where(p => p.Code == staff).FirstOrDefault();
                if (checkStaff != null)
                {
                    return ExcelCheckInStaffDetail(month, year, checkStaff.Id);
                }
            }

            string pathRoot = Server.MapPath("~/haiupload/report-check-in-detail.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);
            try
            {
                FileInfo newFile = new FileInfo(pathTo);
                brand = "%" + brand + "%";

                string branch = "";
                List<string> branches = new List<string>();
                int permiss = Utitl.CheckRoleShowInfo(db, User.Identity.Name);
                if (permiss == 2)
                {
                    branches = Utitl.GetBranchesPermiss(db, User.Identity.Name, false);
                }
                else if (permiss == 1)
                {
                    branches = Utitl.GetBranchesPermiss(db, User.Identity.Name, true);
                }

                if (!String.IsNullOrEmpty(branch))
                {

                    if (branches.Contains(branch))
                    {
                        branches.Clear();
                        branches.Add(branch);
                    }
                    else
                    {
                        branches.Clear();
                    }
                }

                var data = db.report_checkin_detail_by_branch(month, year, brand).ToList();

                data = data.Where(p => branches.Contains(p.Branch)).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = data[i].Branch;
                            worksheet.Cells[i + 2, 2].Value = data[i].StaffCode;
                            worksheet.Cells[i + 2, 3].Value = data[i].StaffName;
                            worksheet.Cells[i + 2, 4].Value = data[i].CalendarType;
                            worksheet.Cells[i + 2, 5].Value = data[i].DayInWeek;
                            string date = data[i].CalendarDay + "/" + data[i].CalendarMonth + "/" + data[i].CalendarYear;
                            worksheet.Cells[i + 2, 6].Value = date;

                            worksheet.Cells[i + 2, 7].Value = data[i].AgencyCode;
                            worksheet.Cells[i + 2, 8].Value = data[i].StoreName;
                            if (data[i].InPlan == 1)
                                worksheet.Cells[i + 2, 9].Value = "X";

                            if (data[i].Perform == 1)
                                worksheet.Cells[i + 2, 10].Value = "X";

                            if (data[i].CalendarId == "HOLIDAY")
                            {
                                worksheet.Cells[i + 2, 11].Value = "X";
                            }


                            if (data[i].InPlan == 1 && data[i].Perform == 1)
                            {
                                worksheet.Cells[i + 2, 12].Value = "ĐÚNG KẾ HOẠCH";
                            }

                            if (data[i].InPlan == 1 && data[i].Perform == 0)
                            {
                                if (!String.IsNullOrEmpty(data[i].AgencyCode))
                                    worksheet.Cells[i + 2, 12].Value = "RỚT";
                                else
                                    worksheet.Cells[i + 2, 12].Value = "NGÀY NGHỈ";
                            }

                            if (data[i].InPlan == 0 && data[i].Perform == 1)
                            {
                                worksheet.Cells[i + 2, 12].Value = "NGOÀI KẾ HOẠCH";
                            }

                            if (data[i].InPlan == 0 && data[i].Perform == 0)
                            {
                                worksheet.Cells[i + 2, 12].Value = "NGOÀI KẾ HOẠCH (RỚT)";
                            }


                            worksheet.Cells[i + 2, 13].Value = data[i].CInTime;
                            worksheet.Cells[i + 2, 14].Value = data[i].COutTime;
                            // lay toa do khach hang
                            worksheet.Cells[i + 2, 15].Value = data[i].Lat;
                            worksheet.Cells[i + 2, 16].Value = data[i].Lng;
                        }
                        catch (Exception e)
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

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-checkin-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }

        public ActionResult ExcelCheckInStaffDetail(int month, int year, string staff = "")
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 0))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/report-check-in-detail.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);
            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_checkin_detail_by_staff(month, year, staff).ToList();


                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["hai"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 1, 1].Value = data[i].Branch;
                            worksheet.Cells[i + 1, 2].Value = data[i].StaffCode;
                            worksheet.Cells[i + 1, 3].Value = data[i].StaffName;
                            worksheet.Cells[i + 1, 4].Value = data[i].CalendarType;
                            worksheet.Cells[i + 1, 5].Value = data[i].DayInWeek;
                            string date = data[i].CalendarDay + "/" + data[i].CalendarMonth + "/" + data[i].CalendarYear;
                            worksheet.Cells[i + 1, 6].Value = date;

                            worksheet.Cells[i + 1, 7].Value = data[i].AgencyCode;
                            worksheet.Cells[i + 1, 8].Value = data[i].StoreName;
                            if (data[i].InPlan == 1)
                                worksheet.Cells[i + 1, 9].Value = "X";

                            if (data[i].Perform == 1)
                                worksheet.Cells[i + 1, 10].Value = "X";

                            if (data[i].CalendarId == "HOLIDAY")
                            {
                                worksheet.Cells[i + 1, 11].Value = "X";
                            }


                            if (data[i].InPlan == 1 && data[i].Perform == 1)
                            {
                                worksheet.Cells[i + 1, 12].Value = "ĐÚNG KẾ HOẠCH";
                            }

                            if (data[i].InPlan == 1 && data[i].Perform == 0)
                            {
                                if (!String.IsNullOrEmpty(data[i].AgencyCode))
                                    worksheet.Cells[i + 1, 12].Value = "RỚT";
                                else
                                    worksheet.Cells[i + 1, 12].Value = "NGÀY NGHỈ";
                            }

                            if (data[i].InPlan == 0 && data[i].Perform == 1)
                            {
                                worksheet.Cells[i + 1, 12].Value = "NGOÀI KẾ HOẠCH";
                            }

                            if (data[i].InPlan == 0 && data[i].Perform == 0)
                            {
                                worksheet.Cells[i + 1, 12].Value = "NGOÀI KẾ HOẠCH (RỚT)";
                            }


                            worksheet.Cells[i + 1, 13].Value = data[i].CInTime;
                            worksheet.Cells[i + 1, 14].Value = data[i].COutTime;
                            // lay toa do khach hang
                            worksheet.Cells[i + 1, 15].Value = data[i].Lat;
                            worksheet.Cells[i + 1, 16].Value = data[i].Lng;

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

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-checkin-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

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

                    for (int i = 0; i < days; i++)
                    {
                        worksheet.Cells[tableIndex, i + 12].Value = i + 1;
                        worksheet.Cells[tableIndex, i + 12].Style.Font.Bold = true;
                    }

                    // general data
                    tableIndex++;
                    var agency = "";
                    var agencyCount = 1;
                    for (int i = 0; i < data.Count(); i++)
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
                            }

                        }
                        worksheet.Cells[tableIndex, 11].Value = data[i].CType;

                        for (int j = 0; j < days; j++)
                        {
                            worksheet.Cells[tableIndex, j + 12].Value = data[i].getValue(j + 1);
                        }


                        tableIndex++;
                    }

                    package.Save();
                }

            }
            catch
            {
                return RedirectToAction("error", "home");
            }


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-ke-hoach-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }

        public ActionResult ReportCheckIn()
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 1))
                return RedirectToAction("relogin", "home");
            ViewBag.Month = DateTime.Now.Month;
            ViewBag.Year = DateTime.Now.Year;
            ViewBag.Day = DateTime.Now.Day;
            return View();
        }


        public ActionResult ReportGeneral(int month, int year, int fDay, int tDay)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 1))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/report_checkin_general.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_checkin_general(month, year, fDay, tDay).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 4, 2].Value = i + 1;
                            worksheet.Cells[i + 4, 3].Value = data[i].BranchCode;
                            worksheet.Cells[i + 4, 4].Value = data[i].Code;
                            worksheet.Cells[i + 4, 5].Value = data[i].FullName;
                            worksheet.Cells[i + 4, 6].Value = data[i].AllAgency;
                            worksheet.Cells[i + 4, 7].Value = data[i].AllAgencyCloser;
                            worksheet.Cells[i + 4, 8].Value = data[i].AllAgencyCheckIn;
                            worksheet.Cells[i + 4, 9].Value = data[i].AllDayCSBH;
                            worksheet.Cells[i + 4, 10].Value = data[i].AllDayCSBH4;
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

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-checkin-tong-hop-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }

        //
        public ActionResult ReportGeneralDay(int month, int year, int fDay)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckIn", 1))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/report_checkin_general_day.xlsx");
            string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                var data = db.report_checkin_general_day(month, year, fDay).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 4, 2].Value = i + 1;
                            worksheet.Cells[i + 4, 3].Value = data[i].BranchCode;
                            worksheet.Cells[i + 4, 4].Value = data[i].Code;
                            worksheet.Cells[i + 4, 5].Value = data[i].FullName;
                            worksheet.Cells[i + 4, 6].Value = data[i].AgencyCheckInDay;
                            worksheet.Cells[i + 4, 7].Value = data[i].AgencyCheckInInPlanDay;
                            worksheet.Cells[i + 4, 8].Value = data[i].AgencyCheckInOutPlanDay;
                            worksheet.Cells[i + 4, 9].Value = data[i].CheckInNotCSKH;

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

            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-checkin-tong-hop-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }
    }
}