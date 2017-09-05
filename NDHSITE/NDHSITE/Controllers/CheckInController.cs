using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;

namespace NDHSITE.Controllers
{
    public class CheckInController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();

        public ActionResult ShowCalendar(int? page, int? month, int? year)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (month == null)
                month = DateTime.Now.Month;

            if (year == null)
                year = DateTime.Now.Year;

            ViewBag.Month = month;
            ViewBag.Year = year;

            var listCalendar = db.CalendarInfoes.Where(p=> p.CMonth==month && p.CYear == year).ToList();
            
            return View(listCalendar.OrderBy(p=> p.CStatus).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ShowCalendarDetail(string id)
        {
            var calendar = db.CalendarInfoes.Find(id);
            var detail = db.checkin_getcalendar(calendar.CMonth, calendar.CYear, calendar.StaffId).OrderBy(p=>p.CDay).ToList();
            ViewBag.GroupType = db.checkin_calendartype_group(calendar.CMonth, calendar.CYear, calendar.StaffId).ToList();
            ViewBag.Calendar = calendar;
            return View(detail);

        }

        [HttpPost]
        public ActionResult ApproveCalendar(string id, string notes)
        {
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
    }
}