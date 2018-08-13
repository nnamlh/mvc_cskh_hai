using HAIAPI.Models;
using HAIAPI.Util;
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



                /*
                if (checkC1 == null)
                    checkC1 = db.C1Info.Where(p => p.Code == "0000000000").FirstOrDefault();
                    */
                var agencyCode = GetAgencyCodeTemp(staff.HaiBranch.Code);

                CInfoCommon cInfo = new CInfoCommon()
                {
                    Id = Guid.NewGuid().ToString(),
                    AddressInfo = paser.address,
                    BranchCode = staff.HaiBranch.Code,
                    BusinessLicense = paser.businessLicense,
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
                    StoreName = paser.name,
                    Deputy = paser.deputy,
                    IsActive = 0,
                    Code = agencyCode,
                    InfoId = cInfo.Id,
                    CStatus = 1
                };
                db.C2Info.Add(c2);
                db.SaveChanges();

                //   db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
                // db.SaveChanges();

                var staffC2 = new StaffWithC2()
                {
                    C2Id = c2.Id,
                    StaffId = staff.Id,
                    GroupChoose = paser.group
                };

                db.StaffWithC2.Add(staffC2);
                db.SaveChanges();


                var checkC1 = db.C1Info.Where(p => p.Code == paser.c1Id).FirstOrDefault();

                // import c1
                if (checkC1 != null)
                {
                    var c2C1Add = new C2C1()
                    {
                        C1Code = checkC1.Code,
                        C2Code = agencyCode,
                        Id = Guid.NewGuid().ToString(),
                        Priority = 1,
                        ModifyDate = DateTime.Now
                    };

                    db.C2C1.Add(c2C1Add);
                    db.SaveChanges();
                }

                // save info
                var agencyImage = new SaveAgencyShopImage()
                {
                    Id = Guid.NewGuid().ToString(),
                    AddressFull = paser.address,
                    Cinfo = cInfo.Id,
                    Country = paser.country,
                    District = paser.district,
                    Lat = paser.lat,
                    Province = paser.province,
                    CreateTime = DateTime.Now,
                    StaffId = staff.Id,
                    Lng = paser.lng,
                    Ward = paser.ward,
                    ImagePath = paser.image
                };

                db.SaveAgencyShopImages.Add(agencyImage);

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


        /// <summary>
        /// 
        ///  update location c2
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ResultInfo UpdateLocation()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/updatelocation",
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
                var paser = jsonserializer.Deserialize<AgencyUpdateLocationRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                var cinfo = db.CInfoCommons.Find(paser.id);
                if (cinfo == null)
                {
                    throw new Exception("Sai thông tin khách hàng");
                }

                //  CInfoCommon cinfo = checkC2.CInfoCommon;
                cinfo.Lat = paser.lat;
                cinfo.Lng = paser.lng;
                db.Entry(cinfo).State = System.Data.Entity.EntityState.Modified;


                // save info
                var agencyImage = new SaveAgencyShopImage()
                {
                    Id = Guid.NewGuid().ToString(),
                    AddressFull = paser.address,
                    Cinfo = cinfo.Id,
                    Country = paser.country,
                    District = paser.district,
                    Lat = paser.lat,
                    Province = paser.province,
                    CreateTime = DateTime.Now,
                    StaffId = staff.Id,
                    Lng = paser.lng,
                    Ward = paser.ward,
                    ImagePath = paser.image
                };

                db.SaveAgencyShopImages.Add(agencyImage);

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

        [HttpGet]
        public List<AgencyInfo> GetAgency(string user, string token)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/getagency",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<AgencyInfo>();
            if (!mongoHelper.checkLoginSession(user, token))
                return result;

            try
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền truy cập");

                result.AddRange(GetStaffC2(staff));

                result.AddRange(GetStaffC1(staff));

            }
            catch
            {
                result = new List<AgencyInfo>();
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }

        #endregion
        [HttpGet]
        public List<AgencyInfo> GetC2C1(string user, string token)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/getc2c1",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<AgencyInfo>();

            try
            {

                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Wrong token and user login!");

                var c1Info = db.C1Info.Where(p => p.CInfoCommon.UserLogin == user).FirstOrDefault();

                if (c1Info == null)
                    throw new Exception("Sai thong tin");


                result = GetListC2OfC1(c1Info.Code);

            }
            catch
            {
                result = new List<AgencyInfo>();
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }



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

        ///
        /// c1 of c2
        ///
        [HttpGet]
        public List<SubOwner> GetC1C2(string code)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/agency/getc1c2",
                CreateTime = DateTime.Now,
                Sucess = 1,
                Content = code
            };

            var c2c1 = db.C2C1.Where(p => p.C2Code == code).ToList();

            List<SubOwner> agencyC2C1 = new List<SubOwner>();

            foreach (var item in c2c1)
            {
                var checkC1 = db.C1Info.Where(p => p.Code == item.C1Code).FirstOrDefault();
                if (checkC1 != null)
                {
                    agencyC2C1.Add(new SubOwner()
                    {
                        code = checkC1.Code,
                        name = checkC1.Deputy,
                        store = checkC1.StoreName,
                        priority = item.Priority
                    });
                }
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(agencyC2C1);
            mongoHelper.createHistoryAPI(log);

            return agencyC2C1;
        }


        /// require reset locaion

        [HttpGet]
        public ResultInfo RequireResetLocation(string user, string token, string agency, string address)
        {
            var result = new ResultInfo()
            {
                id = "0",
                msg = ""
            };

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản đã có người đăng nhập");

                var checkAgency = db.CInfoCommons.Where(p => p.CCode == agency).FirstOrDefault();

                if (checkAgency == null)
                    throw new Exception("Sai thông tin");

                var checkStaff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (checkStaff == null)
                    throw new Exception("Sai thông tin");

                var checkExsit = db.ResetLocations.Where(p => p.AgencyCode == agency && p.UserLogin == user && p.IsAppove == 0).FirstOrDefault();

                if (checkExsit != null)
                    throw new Exception("Đã gửi yêu cầu, đang đợi hệ thống xác nhận");

                var locationReset = new ResetLocation()
                {
                    Id = Guid.NewGuid().ToString(),
                    AgencyCode = checkAgency.CCode,
                    UserLogin = user,
                    CreateDate = DateTime.Now,
                    IsAppove = 0,
                    NewLocation = address,
                    StaffId = checkStaff.Id
                };

                var checkLastUpdate = db.ResetLocations.Where(p => p.AgencyCode == agency).OrderByDescending(p => p.CreateDate).FirstOrDefault();

                if (checkLastUpdate != null)
                    locationReset.OldLocation = checkLastUpdate.NewLocation;
                else
                    locationReset.OldLocation = checkAgency.AddressInfo;

                db.ResetLocations.Add(locationReset);

                db.SaveChanges();

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
            }

            result.id = "1";

            return result;

        }

        [HttpGet]
        public ResultInfo GetListRequiredResetLocation(string user, string token)
        {

            var result = new ResultWithData()
            {
                id = "0",
                msg = ""
            };

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản đã có người đăng nhập");

                result.data = db.ResetLocations.Where(p => p.IsAppove == 0).OrderByDescending(p => p.CreateDate).Select(p=> new { StaffName = p.HaiStaff.FullName,
                    OldLocation = p.OldLocation, NewLocation = p.NewLocation, AgencyCode = p.AgencyCode, RequiredId = p.Id}).ToList();

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
            }

            result.id = "1";

            return result;
        }

        [HttpGet]
        public ResultInfo ApproveResetLocation(string user, string token, string requireId, int approve)
        {
            var result = new ResultInfo()
            {
                id = "0",
                msg = ""
            };

            try
            {

                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản đã có người đăng nhập");

                var checkRequire = db.ResetLocations.Find(requireId);

                if (checkRequire == null)
                    throw new Exception("Sai thông tin");

                if (approve == 1)
                {
                    var findAgency = db.CInfoCommons.Where(p => p.CCode == checkRequire.AgencyCode).FirstOrDefault();

                    findAgency.Lat = 0;
                    findAgency.Lng = 0;

                    db.Entry(findAgency).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();

                }

                checkRequire.IsAppove = 1;
                db.Entry(checkRequire).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

            } catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
            }

            result.id = "1";

            return result;
        }

        [HttpGet]
        public ResultInfo UpdateLocation (string user, string token, string agency, double lat, double lng)
        {
            var result = new ResultInfo()
            {
                id = "0",
                msg = ""
            };

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản đã có người đăng nhập");

                var check = db.CInfoCommons.Where(p => p.CCode == agency).FirstOrDefault();

                if (check == null)
                    throw new Exception("Sai thông tin");

                check.Lat = lat;
                check.Lng = lng;

                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            } catch(Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
            }

            result.id = "1";

            return result;
        }

    }
}
