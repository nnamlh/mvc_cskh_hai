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

                if (paser.isUpdate == 1 && role.GroupRole == "HAI")
                {
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    result.c2 = GetListC2(staff);

                    result.c1 = GetListC1(staff);

                    result.products = GetProductCodeInfo();
                }
                else
                {
                    result.c2 = new List<AgencyInfoC2>();
                    result.c1 = new List<AgencyInfo>();
                }

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

        protected List<ProductCodeInfo> GetProductCodeInfo()
        {
            var product = db.ProductInfoes.ToList();
            List<ProductCodeInfo> productCodes = new List<ProductCodeInfo>();
            foreach (var item in product)
            {
                productCodes.Add(new ProductCodeInfo()
                {
                    code = item.Barcode,
                    name = item.PName
                });
            }

            return productCodes;
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

        private List<string> GetUserTopics(string user)
        {
            var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();


            List<string> topics = new List<string>();
            if (cInfo != null)
            {
                string cusall = "cus";
                topics.Add(cusall);

                // loai khach hàng
                string cus = "cus" + cInfo.CType.ToLower();
                topics.Add(cus);

                // khu vu
                topics.Add(cus + cInfo.HaiArea.Code);

                topics.Add(cusall + cInfo.HaiArea.Code);


            }
            else
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                if (staff != null)
                {
                    // nhan vien
                    var staffTopic = "staff";
                    topics.Add(staffTopic);

                    // theo chi nhanh
                    topics.Add(staffTopic + staff.HaiBranch.Code);
                    // khu vu
                    topics.Add(staffTopic + staff.HaiBranch.HaiArea.Code);

                }

            }

            return topics;

        }

        private List<string> GetUserFunction(string user, string screen)
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
