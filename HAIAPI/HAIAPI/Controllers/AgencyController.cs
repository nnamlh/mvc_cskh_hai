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
    public class AgencyController : RestMainController
    {

        #region
        /// <summary>
        /// tao khach hang cap 2
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public ResultInfo CreateAgencyC2()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/createagencyc2",
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
                var paser = jsonserializer.Deserialize<AgencyCreateRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");


                var checkC1 = db.C1Info.Where(p => p.Code == paser.c1Id).FirstOrDefault();

                if (checkC1 == null)
                    throw new Exception("Sai thông tin cấp 1");
                var agencyCode = GetAgencyCodeTemp(staff.HaiBranch.Code);

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
                    CCode = agencyCode,
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
                    Code = agencyCode,
                    InfoId = cInfo.Id
                };
                db.C2Info.Add(c2);
                db.SaveChanges();

                db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var staffC2 = new StaffWithC2()
                {
                    C2Id = c2.Id,
                    StaffId = staff.Id,
                    GroupChoose = paser.group
                };

                db.StaffWithC2.Add(staffC2);
                db.SaveChanges();

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

        private string GetAgencyCodeTemp(string branch)
        {
            int? number = db.StoreAgencyIds.Where(p => p.TypeStore == "C2Temp").Max(p => p.CountNumber);

            if (number == null)
                number = 0;

            number++;

            var count = Convert.ToString(number).Count();
            var temp = "";
            if (count == 1)
                temp = "000" + number;
            else if (count == 2)
                temp = "00" + number;
            else if (count == 3)
                temp = "0" + number;
            else
                temp = number + "";

            string code = "T" + branch + temp;

            var store = new StoreAgencyId()
            {
                Id = code,
                IsUse = 1,
                CountNumber = number,
                TypeStore = "C2Temp"
            };

            try
            {
                db.StoreAgencyIds.Add(store);
                db.SaveChanges();
            }
            catch
            {
                code = GetAgencyCodeTemp(branch);
            }

            return code;
        }

        [HttpPost]
        public ResultInfo ModifyAgencyC2()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/modifyagencyc2",
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
                var paser = jsonserializer.Deserialize<AgencyModifyRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var checkC2 = db.C2Info.Find(paser.id);
                if (checkC2 == null)
                {
                    throw new Exception("Sai thông tin khách hàng");

                }

                checkC2.StoreName = paser.name;
                checkC2.Deputy = paser.deputy;

                CInfoCommon cinfo = checkC2.CInfoCommon;
                cinfo.CDeputy = paser.deputy;
                cinfo.CName = paser.name;

              //  cinfo.Phone = paser.phone;

                //cinfo.IdentityCard = paser.identityCard;
                cinfo.BusinessLicense = paser.businessLicense;
                cinfo.TaxCode = paser.taxCode;
                cinfo.Country = paser.country;
                cinfo.ProvinceName = paser.province;
                cinfo.DistrictName = paser.district;
                cinfo.WardName = paser.ward;
                cinfo.AddressInfo = paser.address;

                if (paser.lat != 0 && paser.lng != 0)
                {
                    // cho cap nhat toa do voi dk toa do duoi server dc reset
                    if (cinfo.Lat == 0 || cinfo.Lng == 0 || cinfo.Lat == null || cinfo.Lng == null)
                    {
                        cinfo.Lat = paser.lat;
                        cinfo.Lng = paser.lng;
                    }
                    else
                    {
                        throw new Exception("Tọa độ đã được cập nhật trước, nếu bạn muốn cập nhật lại, liên hệ ban quản trị");
                    }
                }

                db.Entry(checkC2).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.Entry(cinfo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var staffC2 = checkC2.StaffWithC2.Where(p => p.StaffId == staff.Id).FirstOrDefault();

                if (staffC2 == null)
                {
                    var staffC2Create = new StaffWithC2()
                    {
                        C2Id = checkC2.Id,
                        StaffId = staff.Id,
                        GroupChoose = paser.group
                    };

                    db.StaffWithC2.Add(staffC2Create);
                    db.SaveChanges();
                }
                else
                {
                    staffC2.GroupChoose = paser.group;
                    db.Entry(staffC2).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
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


        [HttpGet]
        public List<AgencyInfoC2> GetAgencyC2(string user, string token)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/getagencyc2",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<AgencyInfoC2>();
         //   var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
             //   var jsonserializer = new JavaScriptSerializer();
               // var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);

                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");


                result = GetListC2(staff);

            }
            catch
            {
                result = new List<AgencyInfoC2>();
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }

        #endregion


        #region
        ///
        /// Danh sach C1
        ///
        ///
        [HttpGet]
        public List<AgencyInfo> GetAgencyC1(string user, string token)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/getagencyc1",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<AgencyInfo>();
            if (!mongoHelper.checkLoginSession(user, token))
                return result;

            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

            if (staff == null)
                return result;

            result = GetListC1(staff);

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }

        #endregion

    }
}
