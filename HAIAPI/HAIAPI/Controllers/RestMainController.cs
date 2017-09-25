using HAIAPI.Models;
using HAIAPI.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class RestMainController : ApiController
    {
        protected NDHDBEntities db;
        protected MongoHelper mongoHelper;
        public RestMainController()
        {
            db = new NDHDBEntities();
            mongoHelper = new MongoHelper();

        }


        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        [HttpPost]
        public MainInfoResult MainInfo()
        {
            // update regid firebase
            // /api/rest/getmaininfo
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/restmain/maininfo",
                Sucess = 1
            };

            var result = new MainInfoResult()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<MainInfoRequest>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var checkUser = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Lỗi");

                var role = checkUser.AspNetRoles.FirstOrDefault();

                // get topic
                result.topics = GetUserTopics(paser.user);

                result.function = GetUserFunction(paser.user, "main");

                if (paser.isUpdate == 1)
                {
                    result.products = GetProductCodeInfo();
                }

                if (role.GroupRole == "HAI")
                {
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                    if (staff == null)
                        throw new Exception("Không lấy được thông tin");

                    result.code = staff.Code;
                    result.name = staff.FullName;
                    result.type = "Công ty HAI";
                       
                    if (paser.isUpdate == 1)
                    {
                        result.c2 = GetListC2(staff);
                        result.c1 = GetListC1(staff);
                    }
                   
                }
                else
                {
                    var cinfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    if (cinfo == null)
                        throw new Exception("Không lấy được thông tin");


                    result.code = cinfo.CCode;
                    result.name = cinfo.CDeputy;
                    if (cinfo.CType == "CII")
                        result.type = "Đại lý cấp 2";
                    else if (cinfo.CType == "CI")
                        result.type = "Đại lý cấp 1";
                    else
                        result.type = "Chưa xác nhận";

                    result.c2 = new List<AgencyInfoC2>();
                    result.c1 = new List<AgencyInfo>();
                }
               
                result.productGroups = GetGroupProduct();
                var notiReg = db.RegFirebases.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (notiReg == null)
                {
                    notiReg = new RegFirebase()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserLogin = paser.user,
                        RegId = paser.regId,
                        CreateDate = DateTime.Now
                    };

                    db.RegFirebases.Add(notiReg);
                    db.SaveChanges();
                }
                else
                {
                    notiReg.RegId = paser.regId;
                    notiReg.ModifyDate = DateTime.Now;
                    db.Entry(notiReg).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(history);

            return result;
        }

        protected List<ProductInfoResult> GetProductCodeInfo()
        {
            var product = db.product_list().ToList();
            List<ProductInfoResult> productCodes = new List<ProductInfoResult>();
            foreach (var item in product)
            {
                productCodes.Add(new ProductInfoResult()
                {
                    id = item.Id,
                    code = item.PCode,
                    name = item.PName,
                    barcode = item.Barcode,
                    isForcus = item.Forcus,
                    groupId = item.GroupId,
                    groupName = item.GroupName,
                    image = HaiUtil.HostName + item.Thumbnail,
                    isNew = item.New,
                    price = item.Price == null? 0:item.Price,
                    quantity_box = item.QuantityBox == null ? 0:item.QuantityBox,
                    short_describe = item.ShortDescibe,
                    unit = item.Unit,
                    vat = item.PVat == null ? 0:item.PVat
                });
            }

            return productCodes;
        }

        protected List<GroupInfo> GetGroupProduct()
        {
            List<GroupInfo> groups = new List<GroupInfo>();
            var data = db.ProductGroups.Where(p => p.HasChild == 0).OrderByDescending(p => p.Parent).ToList();

            foreach (var item in data)
            {
                groups.Add(new GroupInfo()
                {
                    id = item.Id,
                    name = item.Name,
                    childs = new List<GroupInfo>()
                });
            }

            return groups;
        }

        protected List<AgencyInfoC2> GetListC2(HaiStaff staff)
        {
            List<AgencyInfoC2> agencyResult = new List<AgencyInfoC2>();

            List<C2Info> c2List = new List<C2Info>();

            c2List = staff.StaffWithC2.Where(p => p.C2Info.IsActive == 1).OrderByDescending(p => p.GroupChoose).Select(p => p.C2Info).ToList();
            
            foreach (var item in c2List)
            {
                var staffC2 = staff.StaffWithC2.Where(p => p.C2Id == item.Id).FirstOrDefault();
                int? group = 0;
                if (staffC2 != null)
                    group = staffC2.GroupChoose;

                agencyResult.Add(new AgencyInfoC2()
                {
                    code = item.Code,
                    name = item.StoreName,
                    type = "CII",
                    deputy = item.Deputy,
                    address = item.CInfoCommon.AddressInfo,
                    lat = item.CInfoCommon.Lat == null ? 0 : item.CInfoCommon.Lat,
                    lng = item.CInfoCommon.Lng == null ? 0 : item.CInfoCommon.Lng,
                    phone = item.CInfoCommon.Phone,
                    id = item.Id,
                    rank = item.CInfoCommon.CRank,
                    group = group,
                    identityCard = item.CInfoCommon.IdentityCard,
                    businessLicense = item.CInfoCommon.BusinessLicense,
                    province = item.CInfoCommon.ProvinceName,
                    district = item.CInfoCommon.DistrictName,
                    taxCode = item.CInfoCommon.TaxCode,
                    c1Id = item.C1Info.Code,
                    haibranch = item.CInfoCommon.BranchCode
                });
            }

            return agencyResult;

        }


        protected List<AgencyInfo> GetListC1(HaiStaff staff)
        {
            List<AgencyInfo> agencyResult = new List<AgencyInfo>();

            List<C1Info> c1List = new List<C1Info>();

            var roleCheck = CheckRoleShowInfo(staff.UserLogin);
            
            if (roleCheck == 1)
            {
                c1List = db.C1Info.Where(p => p.IsActive == 1).OrderByDescending(p => p.CInfoCommon.CGroup).ToList();

            } else
            {
                c1List = db.C1Info.Where(p => p.IsActive == 1 && p.HaiBrandId == staff.BranchId).OrderByDescending(p => p.CInfoCommon.CGroup).ToList();

            }


            foreach (var item in c1List)
            {
                agencyResult.Add(new AgencyInfo()
                {
                    code = item.Code,
                    name = item.StoreName,
                    type = "CI",
                    deputy = item.Deputy
                });
            }

            return agencyResult;

        }
        protected void saveHistoryProcess(string task, string calendarId)
        {

            var check = db.ProcessHistories.Where(p => p.ProcessId == task && p.CalendarId == calendarId).FirstOrDefault();

            if (check == null)
            {
                // add quy trinh
                var historyProcess = new ProcessHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    CalendarId = calendarId,
                    ProcessId = task,
                    CreateTime = DateTime.Now
                };
                db.ProcessHistories.Add(historyProcess);
                db.SaveChanges();
            }
        }
        protected List<string> GetUserTopics(string user)
        {
            var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();

            List<string> topics = new List<string>();
            if (cInfo != null)
            {
                topics.Add(cInfo.CType + "ALL");
                topics.Add(cInfo.CType + cInfo.BranchCode);
                topics.Add(cInfo.CType + cInfo.HaiArea.Code);
            }
            else
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                if (staff != null)
                {
                    topics.Add("HAIALL");
                    topics.Add("HAI" + staff.HaiBranch.Code);
                    topics.Add("HAI" + staff.HaiBranch.HaiArea.Code);
                }

            }

            return topics;

        }

        protected List<string> GetUserFunction(string user, string screen)
        {
            var checkUser = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

            if (checkUser != null)
            {

                var role = checkUser.AspNetRoles.FirstOrDefault();

                if (role != null)
                {
                    var function = role.MobileFunctions.Where(p => p.ScreenType == screen).OrderBy(p => p.Lvl).Select(p => p.Code).ToList();

                    return function;
                }

            }
            return new List<string>();
        }

        protected int CheckRoleShowInfo(string userName)
        {
            // 1: xem toan bo
            // 2: xem theo chi nhanh
            // 0: xem tung ca nhan

            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return 0;

            var role = user.AspNetRoles.First();
            if (role == null)
                return 0;

            return Convert.ToInt32(role.ShowInfoRole);
        }


    }
}
