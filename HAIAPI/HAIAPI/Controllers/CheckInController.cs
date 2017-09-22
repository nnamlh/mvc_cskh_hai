﻿using HAIAPI.Models;
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

                result.status = db.CalendarTypes.OrderBy(p => p.TGroup).ToList();
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

        #region
        [HttpPost]
        public ResultInfo CalendarCreate()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/calendarcreate",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CalendarCreateRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkCalendar = db.CalendarInfoes.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCalendar != null)
                    throw new Exception("Kế hoạch này đã được tạo");

                // lap lich
                // luu lich voi trang thai chua xac nhan
                var checkInHistory = new CalendarInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    CMonth = paser.month,
                    CYear = paser.year,
                    CStatus = 0,
                    CreateTime = DateTime.Now,
                    StaffId = staff.Id
                };
                db.CalendarInfoes.Add(checkInHistory);
                db.SaveChanges();
                // lich chi tiet
                foreach (var item in paser.items)
                {
                    if (item.agencies == null || item.agencies.Count() == 0)
                    {
                        CalendarWork plan = new CalendarWork()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CDate = item.day + "",
                            CMonth = paser.month,
                            CYear = paser.year,
                            CDay = item.day,
                            InPlan = 1,
                            Perform = 0,
                            CIn = 0,
                            COut = 0,
                            AllTime = 0,
                            Distance = 0,
                            TypeId = item.status,
                            Notes = item.notes,
                            StaffId = staff.Id
                        };

                        db.CalendarWorks.Add(plan);
                        db.SaveChanges();
                    }
                    else
                    {
                        foreach (var cus in item.agencies)
                        {
                            var checkCus = db.CInfoCommons.Where(p => p.CCode == cus).FirstOrDefault();
                            if (checkCus != null)
                            {
                                CalendarWork plan = new CalendarWork()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CDate = item.day + "",
                                    CMonth = paser.month,
                                    CYear = paser.year,
                                    CDay = item.day,
                                    InPlan = 1,
                                    Perform = 0,
                                    CIn = 0,
                                    COut = 0,
                                    AllTime = 0,
                                    Distance = 0,
                                    AgencyCode = checkCus.CCode,
                                    AgencyType = checkCus.CType,
                                    TypeId = item.status,
                                    Notes = item.notes,
                                    StaffId = staff.Id
                                };

                                db.CalendarWorks.Add(plan);
                                db.SaveChanges();
                            }
                        }
                    }
                }
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


        #region
        ///
        /// lay danh sach checkin
        ///
        [HttpPost]
        public CalendarShowResult CalendarShow()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/calendarshow",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CalendarShowResult()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CalendarShowRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkCalendar = db.CalendarInfoes.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCalendar == null)
                    throw new Exception("Chưa có kế hoạch cho tháng này");

                result.hasApprove = checkCalendar.CStatus;
                result.notes = checkCalendar.Notes;
                result.month = checkCalendar.CMonth;
                result.year = checkCalendar.CYear;

                result.typeDetail = GetCalendarShowTypeDetail(paser.month, paser.year, staff.Id);

                result.items = GetCalendarShowItemes(paser.month, paser.year, staff.Id);
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

        private List<CalendarShowItem> GetCalendarShowItemes(int month, int year, string staffId)
        {
            int days = DateTime.DaysInMonth(year, month);

            List<CalendarShowItem> items = new List<CalendarShowItem>();

            var listCalendarInMonth = db.checkin_getcalendar(month, year, staffId).ToList();


            for (int i = 1; i <= days; i++)
            {
                var allItemInDay = listCalendarInMonth.Where(p => p.CDay == i).ToList();
                CalendarShowItem itemDay = new CalendarShowItem()
                {
                    day = i,
                    agences = new List<CalendarShowAgencyItem>()
                };

                if (allItemInDay.Count > 0)
                {

                    itemDay.type = allItemInDay[0].TypeId;
                    itemDay.notes = allItemInDay[0].Notes;
                    itemDay.typeName = allItemInDay[0].TypeName;

                    foreach (var agency in allItemInDay)
                    {
                        if (!String.IsNullOrEmpty(agency.AgencyCode))
                        {
                            itemDay.agences.Add(new CalendarShowAgencyItem()
                            {
                                code = agency.AgencyCode,
                                ctype = agency.AgencyType,
                                deputy = agency.Deputy,
                                name = agency.StoreName,
                                inPlan = agency.InPlan,
                                perform = agency.Perform
                            });
                        }
                    }
                   
                }

                items.Add(itemDay);
            }

            return items;
        }

        private List<CalendarShowTypeDetail> GetCalendarShowTypeDetail(int month, int year, string staffId)
        {
            var typeDetails = db.checkin_calendartype_group(month, year, staffId).ToList();

            List<CalendarShowTypeDetail> calendarShowTypeDetails = new List<CalendarShowTypeDetail>();
            foreach(var item in typeDetails)
            {
                calendarShowTypeDetails.Add(new CalendarShowTypeDetail()
                {
                    typeId = item.TypeId,
                    typeName = item.Name,
                    number = Convert.ToInt32(item.countday)
                });
            }

            return calendarShowTypeDetails;
        }

        #endregion


        #region
        [HttpPost]
        public CheckInGetPlanResult CheckInGetPlan()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/checkingetplan",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CheckInGetPlanResult()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CheckInGetPlanRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                 if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                  throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkPlan = db.CalendarInfoes.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.StaffId == staff.Id).FirstOrDefault();

                if (checkPlan == null)
                    throw new Exception("Không có kế hoạch");

                if (checkPlan.CStatus != 1)
                    throw new Exception("kế hoạch đang được duyệt");

                // kiem tra dung ngay 
                if(!compareDateCurrent(paser.day, paser.month, paser.year))
                    throw new Exception("Sai thông tin ngày tháng");

                result.inplan = new List<string>();
                result.outplan = new List<string>();

                var listPlan = db.CalendarWorks.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.CDay == paser.day && p.StaffId == staff.Id).ToList();
                foreach (var item in listPlan)
                {
                    if (item.InPlan == 1)
                    {
                       if (item.Perform == 0 )
                        {
                            result.inplan.Add(item.AgencyCode);
                        }
                    } 

                    // tat ca ma ko co ke hoach va da thuc hien + ma trong ke hoach
                    if(item.InPlan == 0)
                    {
                        if(item.Perform == 1)
                        {
                            result.outplan.Add(item.AgencyCode);
                        }
                    } else
                    {
                        result.outplan.Add(item.AgencyCode);
                    }
 
                }


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

        private bool compareDateCurrent(int day, int month, int year)
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateTime.Now.Day;

            if (day != currentDay || month != currentMonth || year != currentYear)
            {
                return false;
            }

            return true;

        }

        #endregion

        #region
        [HttpPost]
        public CheckInTaskResult CheckInTask()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/checkintask",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CheckInTaskResult()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CheckInTaskRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được sử dụng");

                var checkUser = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Lỗi");

                var role = checkUser.AspNetRoles.FirstOrDefault();

                // check inplan hay new plan
                var cinfo = db.CInfoCommons.Where(p => p.CCode == paser.code).FirstOrDefault();
                if (cinfo == null)
                    throw new Exception("Sai mã khách hàng");

                // check 
                var day = DateTime.Now.Day;
                var month = DateTime.Now.Month;
                var year = DateTime.Now.Year;

                int timeRequireCheckIn = 0;
                var processCheckIn = role.ProcessWorks.OrderBy(p=> p.SortIndex).ToList();
                List<TaskInfo> taskInfo = new List<TaskInfo>();
                foreach(var item in processCheckIn)
                {
                    taskInfo.Add(new TaskInfo()
                    {
                        code = item.Id,
                        name = item.ProcessName,
                        time = Convert.ToInt32(item.TimeRequire)
                    });
                    timeRequireCheckIn += Convert.ToInt32(item.TimeRequire);
                }

                result.tasks = taskInfo;
                result.agencyCode = cinfo.CCode;
                result.agencyDeputy = cinfo.CDeputy;
                result.agencyName = cinfo.CName;

                // kiem tra lich
                var checkCalendar = db.CalendarWorks.Where(p => p.CMonth == month && p.CYear == year && p.CDay == day && p.StaffId == staff.Id && p.AgencyCode == paser.code).FirstOrDefault();

                if (checkCalendar != null)
                {
                    // kiem tra da thuc hien xong chua
                    if (checkCalendar.Perform == 1)
                        throw new Exception("Đã hoàn thành ghé thăm");

                    result.inPlan = Convert.ToInt32(checkCalendar.InPlan);

                    if (checkCalendar.CIn == 1)
                    {
                        // da check in
                        // kiem tra thoi gian
                        TimeSpan span = DateTime.Now.TimeOfDay.Subtract(checkCalendar.CInTime.Value);
                        int minuteDistance = span.Hours * 60 + span.Minutes;
                        if (minuteDistance >= timeRequireCheckIn)
                            minuteDistance = 0;
                        else
                            minuteDistance = timeRequireCheckIn - minuteDistance;
                        result.timeRemain = minuteDistance;
                    } else
                    {
                        checkCalendar.CIn = 1;
                        checkCalendar.CInTime = DateTime.Now.TimeOfDay;
                        db.Entry(checkCalendar).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        result.timeRemain = timeRequireCheckIn;
                    }

                    saveHistoryProcess("begintask", checkCalendar.Id);

                }
                else
                {
                    // ngoai ke hoach va tao moi
                    CalendarWork calendar = new CalendarWork()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AgencyCode = cinfo.CCode,
                        AgencyType = cinfo.CType,
                        CDate = day + "",
                        CDay = day,
                        CMonth = month,
                        CYear = year,
                        TypeId = "CSKH",
                        InPlan = 0,
                        Perform = 0,
                        CIn = 1,
                        CInTime = DateTime.Now.TimeOfDay,
                        COut = 0,
                        AllTime = 0,
                        Distance = 0,
                        StaffId = staff.Id,
                        TimeCheck = DateTime.Now,
                        Notes = "Tham ngoai ke hoach"
                    };
                    db.CalendarWorks.Add(calendar);
                    db.SaveChanges();

                    saveHistoryProcess("begintask", calendar.Id);

                    result.timeRemain = timeRequireCheckIn;
                    result.inPlan = 0;
                }

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

        #region
        [HttpPost]
        public ResultInfo CheckIn()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/checkin/checkin",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CheckInRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được sử dụng");

                // check inplan hay new plan
                var cinfo = db.CInfoCommons.Where(p => p.CCode == paser.agency).FirstOrDefault();
                if (cinfo == null)
                    throw new Exception("Sai mã khách hàng");

                // check 
                var day = DateTime.Now.Day;
                var month = DateTime.Now.Month;
                var year = DateTime.Now.Year;
                int timeRequireCheckIn = 0;
                var checkUser = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Lỗi");

                var role = checkUser.AspNetRoles.FirstOrDefault();
                var processCheckIn = role.ProcessWorks.ToList();
                foreach (var item in processCheckIn)
                {
                    timeRequireCheckIn += Convert.ToInt32(item.TimeRequire);
                }
                var checkCalendar = db.CalendarWorks.Where(p => p.CMonth == month && p.CYear == year && p.CDay == day && p.StaffId == staff.Id && p.AgencyCode == paser.agency).FirstOrDefault();

                if (checkCalendar != null)
                {
                    // kiem tra da thuc hien xong chua
                    if (checkCalendar.Perform == 1)
                        throw new Exception("Đã hoàn thành ghé thăm");
                    TimeSpan span = DateTime.Now.TimeOfDay.Subtract(checkCalendar.CInTime.Value);
                    int minuteDistance = span.Hours * 60 + span.Minutes;
                    if (minuteDistance >= timeRequireCheckIn)
                        minuteDistance = 0;
                    else
                        minuteDistance = timeRequireCheckIn - minuteDistance;

                    if (minuteDistance > 0)
                        throw new Exception("Còn " + minuteDistance + " phút để có thể checkin");

                    checkCalendar.COut = 1;
                    checkCalendar.Perform = 1;
                    checkCalendar.TimeCheck = DateTime.Now;
                    checkCalendar.Distance = paser.distance;
                    checkCalendar.LatCheck = paser.lat;
                    checkCalendar.LngCheck = paser.lng;
                    checkCalendar.AllTime = span.Hours * 60 + span.Minutes;

                    db.Entry(checkCalendar).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    saveHistoryProcess("endtask", checkCalendar.Id);

                }

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
