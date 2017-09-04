using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class CheckInController : RestMainController
    {
        #region
        [HttpGet]
        public CheckCalendarResult CalendarCheck(string user)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/calendarcheck",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CheckCalendarResult()
            {
                id = "1",
                msg = "success",
                month = new List<string>()
            };

            try
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");


                // lay current month year and next
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                var nextMonth = currentMonth + 1;
                var nextYear = currentYear;

                if (currentMonth == 12)
                {
                    nextMonth = 1;
                    nextYear = currentYear + 1;
                }

                // check next month
                var calendarNextMonth = db.CalendarInfoes.Where(p => p.CMonth == nextMonth && p.CYear == nextYear && p.StaffId == staff.Id).FirstOrDefault();

                if (calendarNextMonth == null)
                {
                    result.month.Add(nextMonth + "/" + nextYear);
                }

                // check current month
                var calendarCurrent = db.CalendarInfoes.Where(p => p.CMonth == currentMonth && p.CYear == currentYear && p.StaffId == staff.Id).FirstOrDefault();

                if (calendarCurrent == null)
                {
                    result.month.Add(currentMonth + "/" + currentYear);
                }

                result.status = db.CalendarTypes.OrderBy(p=> p.TGroup).ToList();
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(log);

            return result;

        }
        #endregion
    }
}
