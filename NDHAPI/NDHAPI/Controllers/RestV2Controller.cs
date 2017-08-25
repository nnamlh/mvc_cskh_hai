using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NDHAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace NDHAPI.Controllers
{
    public class RestV2Controller : RestParentController
    {


        #region Methoh GetStaffC1
        /// <summary>
        /// Lây danh sách c1
        /// </summary>
        /// <param name="code"></param>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [HttpGet]
        public ResultAgency GetStaffC1(string code, string user, string token)
        {
            // check sesion for login
            // /api/rest/getstaffc1
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/restv2/getstaffc1",
                Sucess = 1,
                Content = "code : " + code + " ; user : " + user + " token : " + token
            };

            var result = new ResultAgency()
            {
                id = "1"
            };

            if (!checkLoginSession(user, token))
            {
                result.id = "0";
                result.msg = "Tài khoản bạn đã đăng nhập ở thiết bị khác.";
                history.Sucess = 0;
            }
            else
            {
                var staff = db.HaiStaffs.Where(p => p.Code == code).FirstOrDefault();

                if (staff == null)
                {
                    result.id = "0";
                    result.msg = "Không tìm thấy nhân viên này.";
                    history.Sucess = 0;
                }
                else
                {
                    var allC1 = db.C1Info.Where(p => p.HaiBrandId == staff.BranchId && p.IsActive == 1).ToList();
                    List<AgencyInfo> agences = new List<AgencyInfo>();
                    foreach (var item in allC1)
                    {
                        agences.Add(new AgencyInfo()
                        {
                            code = item.Code,
                            name = item.StoreName
                        });
                    }

                    result.agences = agences;

                }
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }
        #endregion

        #region
        /// <summary>
        /// tao khach hang cap 2
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ResultInfo CreateAgencyC2()
        {
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/restv2/createagencyc2",
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
                var paser = jsonserializer.Deserialize<AgencyCreate>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkC1 = db.C1Info.Where(p => p.Code == paser.c1Id).FirstOrDefault();

                if (checkC1 == null)
                    throw new Exception("Sai thông tin cấp 1");


                CInfoCommon cInfo = new CInfoCommon()
                {
                    Id = Guid.NewGuid().ToString(),
                    AddressInfo = paser.address,
                    AreaId = staff.HaiBranch.AreaId,
                    BranchCode = staff.HaiBranch.Code,
                    BusinessLicense = paser.businessLicense,
                    CGroup = paser.group,
                    CRank = paser.rank,
                    CDeputy = paser.deputy,
                    CName = paser.name,
                    CreateTime = DateTime.Now,
                    ProvinceName = paser.province,
                    DistrictName = paser.district,
                    Phone = paser.phone,
                    IdentityCard = paser.identityCard,
                    CType = "CII",
                    TaxCode = paser.taxCode,
                    WardId = "11111",
                    Lat = paser.lat,
                    Lng = paser.lng,
                    CCode = "new",
                    WardName = paser.ward,
                    Country = paser.country
                };

                db.CInfoCommons.Add(cInfo);
                db.SaveChanges();

                C2Info c2 = new C2Info()
                {
                    Id = Guid.NewGuid().ToString(),
                    C1Id = checkC1.Id,
                    StoreName = paser.name,
                    Deputy = paser.deputy,
                    IsActive = 0,
                    Code = "new",
                    InfoId = cInfo.Id
                };
                db.C2Info.Add(c2);
                db.SaveChanges();

                staff.C2Info.Add(c2);
                db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(log);
            db.SaveChanges();

            return result;
        }


        

        [HttpPost]
        public List<AgencyInfoC2Result> GetStaffAgencyC2()
        {
            /*
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/restv2/getstaffagencyc2",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };
            */
            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);
              //  log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");


                return getListAgency(staff);

            }
            catch
            {
            //    result.id = "0";
              //  result.msg = e.Message;
              //  log.Sucess = 0;
            }

         //   log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
          //  db.APIHistories.Add(log);
         //   db.SaveChanges();

            return new List<AgencyInfoC2Result>();
        }

        #endregion


        #region
        ///
        /// Check create calendar
        ///


        #endregion

        #region
        ///
        /// show calendar
        ///
        [HttpPost]
        public CheckInCalendarShow ShowStaffCalendar()
        {
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/restv2/showstaffcalendar",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CheckInCalendarShow()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<CheckInCalendarShowRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                //  if (!checkLoginSession(paser.user, paser.token))
                //    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkCalendar = db.CheckInCalendarHistories.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCalendar == null)
                    throw new Exception("Chưa có kế hoạch cho tháng này");

                result.hasApprove = checkCalendar.CStatus;
                result.month = checkCalendar.CMonth;
                result.year = checkCalendar.CYear;
                result.items = GetListStaffCalendar(paser.month, paser.year, staff.Id);
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(log);
            db.SaveChanges();

            return result;
        }
        #endregion

        #region
        [HttpGet]
        public CalendarCheckCreate CheckCalendarCreate(string user)
        {
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/restv2/calendarcheckcreate",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new CalendarCheckCreate()
            {
                id = "1",
                msg = "success",
                month = new List<string>()
            };



            try
            {
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                var nextMonth = currentMonth + 1;
                var nextYear = currentYear;
                if (currentMonth == 12)
                {
                    nextMonth = 1;
                    nextYear = currentYear + 1;
                }


                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();



                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkCalendarNextMonth = db.CheckInCalendarHistories.Where(p => p.CMonth == nextMonth && p.CYear == nextYear && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCalendarNextMonth == null)
                {
                    result.month.Add(nextMonth + "/" + nextYear);
                }


                var checkCurrentCalendar = db.CheckInCalendarHistories.Where(p => p.CMonth == currentMonth && p.CYear == currentYear && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCurrentCalendar == null)
                {
                    result.month.Add(currentMonth + "/" + currentYear);
                }



                result.status = GetListCheckInStatus();
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(log);
            db.SaveChanges();

            return result;

        }
        #endregion

        private List<CheckInStatus> GetListCheckInStatus()
        {
            List<CheckInStatus> listStatus = new List<CheckInStatus>();
            var data = db.CheckInCalendarStatus.ToList();

            foreach (var item in data)
            {
                listStatus.Add(new CheckInStatus()
                {
                    code = item.Id,
                    name = item.Name
                });
            }

            return listStatus;
        }

        private List<CheckInCalendarItemShow> GetListStaffCalendar(int month, int year, string staffId)
        {
            int days = DateTime.DaysInMonth(year, month);

            List<CheckInCalendarItemShow> items = new List<CheckInCalendarItemShow>();

            var listCalendarInMonth = db.checkin_getcalendar(month, year, staffId).ToList();


            for (int i = 1; i <= days; i++)
            {
                var allItemInDay = listCalendarInMonth.Where(p => p.CDay == i).ToList();
                CheckInCalendarItemShow itemDay = new CheckInCalendarItemShow()
                {
                    day = i,
                    calendar = new List<CheckInAgencyCalendar>()
                };

                if (allItemInDay.Count == 1)
                {
                    itemDay.status = allItemInDay[0].CheckInStatus;
                    itemDay.notes = allItemInDay[0].Notes;
                    itemDay.statusName = allItemInDay[0].StatusName;
                    if (allItemInDay[0].CheckInStatus == "CSKH")
                    {
                        itemDay.calendar.Add(new CheckInAgencyCalendar()
                        {
                            code = allItemInDay[0].CCode,
                            ctype = allItemInDay[0].CType,
                            deputy = allItemInDay[0].CDeputy,
                            name = allItemInDay[0].CName,
                            inPlan = allItemInDay[0].InPlan,
                            perform = allItemInDay[0].Perform

                        });
                    }
                }
                else
                {
                    itemDay.status = "CSKH";
                    itemDay.statusName = "Thăm khách hàng";
                    foreach (var agency in allItemInDay)
                    {
                        itemDay.calendar.Add(new CheckInAgencyCalendar()
                        {
                            code = agency.CCode,
                            ctype = agency.CType,
                            deputy = agency.CDeputy,
                            name = agency.CName,
                            inPlan = agency.InPlan,
                            perform = agency.Perform
                            

                        });
                    }
                }

                items.Add(itemDay);

            }


            return items;
        }


        [HttpPost]
        public ResultInfo CheckInCalendarCreate()
        {
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/restv2/CheckInCalendarCreate",
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
                var paser = jsonserializer.Deserialize<CalendarCreate>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkCalendar = db.CheckInCalendarHistories.Where(p => p.CMonth == paser.month && p.CYear == paser.year && p.StaffId == staff.Id).FirstOrDefault();

                if (checkCalendar != null)
                    throw new Exception("Kế hoạch này đã được tạo");

                // lap lich
                var checkInHistory = new CheckInCalendarHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    CMonth = paser.month,
                    CYear = paser.year,
                    Notes = paser.notes,
                    CStatus = 0,
                    CreateTime = DateTime.Now,
                    StaffId = staff.Id
                };
                db.CheckInCalendarHistories.Add(checkInHistory);
                db.SaveChanges();

                foreach (var item in paser.items)
                {
                    if (item.status != "CSKH")
                    {
                        CheckInCalendar plan = new CheckInCalendar()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CDate = item.day,
                            CMonth = paser.month,
                            CYear = paser.year,
                            CDay = item.day,
                            InPlan = 1,
                            Perform = 0,
                            CheckInStatus = item.status,
                            Notes = item.notes,
                            StaffId = staff.Id,
                            CInfoId = "none"
                        };
                        db.CheckInCalendars.Add(plan);
                        db.SaveChanges();
                    }
                    else
                    {

                        foreach (var cus in item.agencies)
                        {
                            var checkCus = db.CInfoCommons.Where(p => p.CCode == cus).FirstOrDefault();
                            if (checkCus != null)
                            {

                                CheckInCalendar plan = new CheckInCalendar()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CDate = item.day,
                                    CMonth = paser.month,
                                    CYear = paser.year,
                                    CDay = item.day,
                                    InPlan = 1,
                                    Perform = 0,
                                    CheckInStatus = "CSKH",
                                    Notes = "",
                                    StaffId = staff.Id,
                                    CCode = checkCus.CCode,
                                    CType = checkCus.CType,
                                    CInfoId = checkCus.Id
                                };

                                db.CheckInCalendars.Add(plan);
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
            db.APIHistories.Add(log);
            db.SaveChanges();

            return result;

        }

    }
}
