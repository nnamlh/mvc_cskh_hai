using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NDHAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using NDHAPI.Utils;
using System.Net.Http.Headers;
using System.Collections;
using System.Data.Entity;
using System.Device.Location;
using SMSUtl;

namespace NDHAPI.Controllers
{
    public class RestController : RestParentController
    {

      

        #region Login
        /// <summary>
        /// danh cho login logout
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public LoginInfo Login(string imei = "")
        {
            // login
            // /api/rest/login
            // method: get
            HttpRequestHeaders headers = Request.Headers;

            var check = Auth(headers, imei);

            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/login",
                ReturnInfo = new JavaScriptSerializer().Serialize(check.Result)
            };

            if (check.Result.id == "0")
            {
                history.Sucess = 0;
            }
            else history.Sucess = 1;

            db.APIHistories.Add(history);
            db.SaveChanges();

            return check.Result;
        }

        [HttpGet]
        public CheckUserLoginReponse CheckUserLogin()
        {
            // login
            // /api/rest/checkuserlogin
            // method: get

            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/checkuserlogin"
            };

            HttpRequestHeaders headers = Request.Headers;
            if (!headers.Contains("Authorization"))
            {
                throw new Exception("Nead authorization info");
            }

            string token;

            try
            {
                string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                token = XString.FromBase64(base64Auth);
            }
            catch
            {
                throw new Exception("Wrong authorization info");
            }

            var arrtok = token.Split(':');

            if (arrtok.Length != 2)
                throw new Exception("Wrong authorization format");

            string user = arrtok[0];
            string phone = arrtok[1];

            var result = new CheckUserLoginReponse()
            {
                id = "1",
                msg = "success",
                user = user
            };

            try
            {
                var checkUser = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (checkUser != null)
                {
                    if (checkUser.IsLock == 1)
                    {
                        throw new Exception("Tài khoản đang tạm khóa");
                    }

                    result.id = "1";
                }
                else
                {
                    var check = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();
                    if (check != null)
                    {
              
                        result.id = "2";
                        result.name = check.CDeputy;
                        result.store = check.CName;
                        result.code = check.CCode;
                        result.phone = check.Phone;

                        if (check.Phone != null)
                        {
                            // kiem tra phone
                            var phoneOrige = check.Phone;
                            if (check.Phone.Substring(0, 2) == "84")
                            {
                                phoneOrige = "0" + check.Phone.Substring(2, check.Phone.Length - 2);
                            }
                            if (phone == phoneOrige)
                            {
                                result.id = "3";
                                // cho dang nhap luon

                                bool isActive = false;

                                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                                if (staff != null)
                                {
                                    if (staff.IsLock != 1)
                                        isActive = true;
                                }
                                else
                                {
                                    var agency = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();
                                    if (agency != null)
                                    {
                                        if (check.CType == "CII")
                                        {
                                            var checkC2 = check.C2Info.FirstOrDefault();
                                            if (checkC2 != null)
                                            {
                                                if (checkC2.IsActive == 0)
                                                {
                                                    isActive = false;
                                                }
                                            }
                                        }
                                    }
                                }


                                if (!isActive)
                                    throw new Exception("Tài khoản bị khóa");

                                var userData = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();
                                var userRole = userData.AspNetRoles.FirstOrDefault();

                                var authToke = Guid.NewGuid().ToString();

                                var lastLogin = db.APIAuthHistories.Where(p => p.UserLogin == user && p.IsExpired == 0).FirstOrDefault();

                                if (lastLogin != null)
                                {
                                    lastLogin.IsExpired = 1;
                                    db.Entry(lastLogin).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }

                                var loginHistory = new APIAuthHistory()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IsExpired = 0,
                                    LoginTime = DateTime.Now,
                                    UserLogin = user,
                                    UserRole = userRole.Name,
                                    Token = authToke
                                };

                                db.APIAuthHistories.Add(loginHistory);
                                db.SaveChanges();

                                result.role = userRole.Name;
                                result.token = authToke;

                            }
                            else
                            {
                                string Msg = string.Empty;
                                var account = db.SmsAccounts.Find(1);
                                Random random = new Random();
                                var otp = random.Next(100000, 999999);

                                // update otp old
                                var allOtp = db.SMSCodes.Where(p => p.UserLogin == user && p.CStatus == 0).ToList();
                                foreach (var item in allOtp)
                                {
                                    item.CStatus = 1;
                                    db.Entry(item).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                SMSCode smsCode = new SMSCode()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Code = Convert.ToString(otp),
                                    CreateAt = DateTime.Now,
                                    CStatus = 0,
                                    UserLogin = user
                                };

                                db.SMSCodes.Add(smsCode);
                                db.SaveChanges();

                                // send sms
                                SMScore _smsCore = new SMScore(account.BrandName, account.UserName, account.Pass);
                                _smsCore.IPserver = account.AddressSend;
                                _smsCore.Port = Convert.ToInt32(account.PortSend);
                                _smsCore.SendMethod = account.Method;

                                _smsCore.SendSMS("Cam on quy khach da dang ky, ma kich hoat cua quy khach la : " + otp, check.Phone, ref Msg);
                            }

                        }

                        else
                        {
                            throw new Exception("Quý khách chưa đăng kí số điện thoại với HAI để nhận mà kích hoặt");
                        }
                    }
                    else
                    {
                        throw new Exception("Tài khoản không hợp lệ");
                    }
                }

            }
            catch (Exception e)
            {

                result.id = "0";
                result.msg = e.Message;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }

        [HttpGet]
        public LoginInfo LoginActivaton()
        {
            // login
            // /api/rest/loginactivaton
            // method: get

            HttpRequestHeaders headers = Request.Headers;
            if (!headers.Contains("Authorization"))
            {
                throw new Exception("Nead authorization info");
            }

            string token;

            try
            {
                string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                token = XString.FromBase64(base64Auth);
            }
            catch
            {
                throw new Exception("Wrong authorization info");
            }

            var arrtok = token.Split(':');

            if (arrtok.Length != 2)
                throw new Exception("Wrong authorization format");

            string user = arrtok[0];
            string otp = arrtok[1];


            var result = new LoginInfo()
            {
                id = "1",
                msg = "success",
                user = user
            };


            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/loginactivaton"
            };

            try
            {

                var check = db.SMSCodes.Where(p => p.UserLogin == user && p.Code == otp && p.CStatus == 0).FirstOrDefault();

                if (check == null)
                    throw new Exception("Không thể đăng nhập vui lòng thử lại");

                check.CStatus = 1;
                db.Entry(check).State = EntityState.Modified;
                db.SaveChanges();


                var userData = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();
                var userRole = userData.AspNetRoles.FirstOrDefault();

                var authToke = Guid.NewGuid().ToString();

                var lastLogin = db.APIAuthHistories.Where(p => p.UserLogin == user && p.IsExpired == 0).FirstOrDefault();

                if (lastLogin != null)
                {
                    lastLogin.IsExpired = 1;
                    db.Entry(lastLogin).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                var loginHistory = new APIAuthHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsExpired = 0,
                    LoginTime = DateTime.Now,
                    UserLogin = user,
                    UserRole = userRole.Name,
                    Token = authToke
                };

                db.APIAuthHistories.Add(loginHistory);
                db.SaveChanges();

                result.Role = userRole.Name;
                result.token = authToke;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message; ;
            }

            return result;
        }


        #endregion

        #region Logout
        /// <summary>
        /// --------------------------------// logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultInfo Logout()
        {
            // logout
            // /api/rest/logout
            // method: post
            var log = new APIHistory()
           {
               Id = Guid.NewGuid().ToString(),
               APIUrl = "/api/rest/logout",
               CreateTime = DateTime.Now,
               Sucess = 1
           };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<LogoutInfo>(requestContent);

                // xoa firebase id
                var regFirebase = db.RegFirebases.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (regFirebase != null)
                {
                    regFirebase.RegId = "";
                    regFirebase.ModifyDate = DateTime.Now;
                    db.Entry(regFirebase).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }


                APILogoutHistory logoutHistory = new APILogoutHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    LogoutTime = DateTime.Now,
                    Token = paser.token,
                    UserLogin = paser.user
                };

                db.APILogoutHistories.Add(logoutHistory);
                db.SaveChanges();

            }
            catch
            {

            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(log);
            db.SaveChanges();

            return result;
        }
        #endregion

        #region Auth
        private async Task<LoginInfo> Auth(HttpRequestHeaders headers, string imei)
        {

            if (!headers.Contains("Authorization"))
            {
                return new LoginInfo() { id = "0", msg = "Nead authorization info" };
            }

            string token;

            try
            {
                string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                token = XString.FromBase64(base64Auth);
            }
            catch
            {
                return new LoginInfo { id = "0", msg = "Wrong authorization info" };
            }

            var arrtok = token.Split(':');

            if (arrtok.Length != 2)
                return new LoginInfo { id = "0", msg = "Wrong authorization format" };

            string UserName = arrtok[0];
            string PassWord = arrtok[1];

            var user = await UserManager.FindAsync(UserName, PassWord);

            if (user != null)
            {

                bool isActive = false;

                var staff = db.HaiStaffs.Where(p => p.UserLogin == user.UserName).FirstOrDefault();
                if (staff != null)
                {
                    if (staff.IsLock != 1)
                        isActive = true;
                }
                else
                {
                    var agency = db.CInfoCommons.Where(p => p.UserLogin == user.UserName).FirstOrDefault();
                    if (agency != null)
                    {
                        if (agency.CType == "CII")
                        {
                            var checkC2 = agency.C2Info.FirstOrDefault();
                            if (checkC2 != null)
                            {
                                if (checkC2.IsActive == 0)
                                {
                                    isActive = false;
                                }
                            }
                        }
                    }
                }


                if (!isActive)
                    return new LoginInfo { id = "0", msg = "Tài khoản bị khóa.", user = UserName };


                var userData = db.AspNetUsers.Find(user.Id);
                var userRole = userData.AspNetRoles.FirstOrDefault();

                if (userRole != null)
                {


                    // check imei

                    var roleCheckImei = db.RoleCheckImeis.Where(p => p.RoleName == userRole.Name).FirstOrDefault();
                    if (roleCheckImei != null)
                    {
                        // phai check imei
                        var userImei = db.ImeiUsers.Where(p => p.UserName == UserName).FirstOrDefault();
                        if (userImei == null)
                        {
                            // dc phep
                            userImei = new ImeiUser()
                            {
                                Id = Guid.NewGuid().ToString(),
                                CreateDate = DateTime.Now,
                                IsUpdate = 0,
                                UserName = UserName,
                                Imei = imei
                            };

                            db.ImeiUsers.Add(userImei);
                            db.SaveChanges();
                        }
                        else if (userImei.IsUpdate == 1)
                        {
                            // xet lai cho tai khoan duoc phep dang nhap lai
                            userImei.Imei = imei;
                            userImei.ModifyDate = DateTime.Now;
                            userImei.IsUpdate = 0;
                            db.Entry(userImei).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            // kiem tra imei
                            if (userImei.Imei != imei)
                                return new LoginInfo { id = "0", msg = "Ứng dụng đã sử dụng trên thiết bị khác, liên hệ HAI nếu bạn muốn đăng nhập lại", user = UserName };
                        }
                    }



                    //string[] mobil = userRole.MobileFunctions.Where(p => p.ScreenType == "main").Select(p => p.Code).ToArray();

                    var authToke = Guid.NewGuid().ToString();

                    var lastLogin = db.APIAuthHistories.Where(p => p.UserLogin == UserName && p.IsExpired == 0).FirstOrDefault();

                    if (lastLogin != null)
                    {
                        lastLogin.IsExpired = 1;
                        db.Entry(lastLogin).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    var loginHistory = new APIAuthHistory()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsExpired = 0,
                        LoginTime = DateTime.Now,
                        UserLogin = UserName,
                        UserRole = userRole.Name,
                        Token = authToke
                    };

                    db.APIAuthHistories.Add(loginHistory);
                    db.SaveChanges();

                    // return new LoginInfo { id = "1", msg = "login success", token = authToke, user = UserName, function = mobil, Role = userRole.Name };
                    return new LoginInfo { id = "1", msg = "login success", token = authToke, user = UserName, Role = userRole.Name };
                }
                return new LoginInfo { id = "0", msg = "User is not permission.", user = UserName };


            }
            else
                return new LoginInfo { id = "0", msg = "User or Password not correct", user = UserName };

        }
        #endregion

        #region updateReg - topic - getfunction
        /// <summary>
        /// --------------------------------
        /// </summary>
        /// <returns></returns>
        // get regid firebase 
        [HttpPost]
        public ResultFirebaseInfo updateReg()
        {
            // update regid firebase
            // /api/rest/updatereg
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/updatereg",
                Sucess = 1
            };

            var result = new ResultFirebaseInfo()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<UpdateRegFirebase>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");


                // get topic
                result.topics = getTopics(paser.user);
                //  result.ecount = countEvent(paser.user);
                result.function = getFunction(paser.user, "main");


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

            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }


        [HttpPost]
        public ResultMainInfo getMainInfo()
        {
            // update regid firebase
            // /api/rest/getmaininfo
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/getmaininfo",
                Sucess = 1
            };

            var result = new ResultMainInfo()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<UpdateRegFirebase>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var checkUser = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Lỗi");

                var role = checkUser.AspNetRoles.FirstOrDefault();

                // get topic
                result.topics = getTopics(paser.user);

                result.function = getFunction(paser.user, "main");

                if (paser.isUpdate == 1 && role.GroupRole == "HAI")
                {
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    result.agencies = getListAgency(staff);

                    result.recivers = getReceiver(getRoleType(role.Name, paser.user), paser.user);

                    result.products = getProductCodeInfo();

                    result.agencyc1 = getListAgencyC1(staff);
                }
                else
                {
                    result.agencies = new List<AgencyInfoC2Result>();
                    result.recivers = new List<ReceiverInfo>();
                    result.products = new List<ProductCodeInfo>();
                    result.agencyc1 = new List<AgencyInfo>();
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

            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }

        private List<string> getFunction(string user, string screen)
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

        private List<ProductCodeInfo> getProductCodeInfo()
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
        private string getRole(string user)
        {
            var checkUser = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

            if (checkUser != null)
            {
                var role = checkUser.AspNetRoles.FirstOrDefault();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                if (staff != null)
                {
                    var userStaff = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

                    if (role.Name == "Warehouse")
                        return "W";
                    else
                        return "B";
                }
                else
                {
                    var agency = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();
                    return agency.CType;
                }
            }

            return "";
        }

        private string getRoleType(string role, string user)
        {
            var checkUser = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

            if (staff != null)
            {
                var userStaff = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

                if (role == "Warehouse")
                    return "W";
                else
                    return "B";
            }
            else
            {
                var agency = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();
                return agency.CType;
            }

        }

        private List<string> getTopics(string user)
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


        private int countEvent(string user)
        {
            var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();

            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

            if (cInfo != null)
            {
                // lay danh sach sự kiện phù hợp với user (theo khu vực)
                List<EventInfo> listEventUser = new List<EventInfo>();
                if (cInfo.CType == "CII")
                {
                    var eventArea = db.EventAreas.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            listEventUser.Add(item.EventInfo);
                        }
                    }

                }
                else if (cInfo.CType == "FARMER")
                {
                    var eventArea = db.EventAreaFarmers.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            listEventUser.Add(item.EventInfo);
                        }
                    }
                }

                return listEventUser.Count();

            }
            else if (staff != null)
            {

                var listEvent = db.EventAreas.Where(p => p.EventInfo.ESTT == 1 && p.AreaId == staff.HaiBranch.AreaId).ToList();
                return listEvent.Count();
            }
            return 0;
        }
        #endregion

        #region functionProduct
        /// <summary>
        /// -------------------------------- man hinh chon chuc nang san pham
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ResultFunctionProduct functionProduct()
        {
            // update regid firebase
            // /api/rest/functionproduct
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/functionproduct",
                Sucess = 1
            };

            var result = new ResultFunctionProduct()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                //if (!checkLoginSession(paser.user, paser.token))
                //  throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");


                result.function = getFunction(paser.user, "product");

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }
        #endregion

        #region LoginSession
        /// <summary>
        /// --------------------------------
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResultInfo LoginSession(string user, string token)
        {

            // check sesion for login
            // /api/rest/loginsession
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/loginsession",
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1"
            };

            if (!checkLoginSession(user, token))
            {
                result.id = "0";
                result.msg = "Tài khoản bạn đã đăng nhập ở thiết bị khác.";
                history.Sucess = 0;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;

        }

        #endregion

        #region CheckIn
        /// <summary>
        /// check in cho ứng dụng
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultInfo CheckIn()
        {
            // staff checkin
            // url: /api/rest/checkin
            // method: put

            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/checkin",
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
                var paser = jsonserializer.Deserialize<CheckinResponse>(requestContent);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                // save log
                log.Content = new JavaScriptSerializer().Serialize(paser);

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff != null)
                {
                    DateTime? dt = DateTime.Now;

                    try
                    {
                        dt = DateTime.ParseExact(paser.date, "MM/dd/yyyy HH:mm", null);
                    }
                    catch
                    {

                    }

                    StaffCheckIn info = new StaffCheckIn()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Comment = paser.comment,
                        Latitude = paser.latitude,
                        Longitude = paser.longitude,
                        StaffId = staff.Id,
                        AcceptTime = dt,
                        CreateDate = DateTime.Now,
                        ImageUrl = paser.image,
                        StaffUser = paser.user,
                        Agency = paser.agency
                    };

                    var c1Info = db.C1Info.Where(p => p.Code == paser.agency).FirstOrDefault();



                    if (c1Info != null)
                    {
                        info.AgencyName = c1Info.StoreName;
                        info.AgencyAddress = c1Info.CInfoCommon.AddressInfo;
                        info.AgencyType = "CI";
                        info.AgencyTypeName = "Đại lý cấp 1";
                    }
                    else
                    {
                        var c2Info = db.C2Info.Where(p => p.Code == paser.agency).FirstOrDefault();
                        if (c2Info != null)
                        {
                            info.AgencyName = c2Info.StoreName;
                            info.AgencyAddress = c2Info.CInfoCommon.AddressInfo;
                            info.AgencyType = "CII";
                            info.AgencyTypeName = "Đại lý cấp 2";
                        }

                    }

                    db.StaffCheckIns.Add(info);
                    db.SaveChanges();
                }
                else throw new Exception("wrong user login !");


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

        #region FindAgency
        /// <summary>
        /// tìm đại lý cấp 1, cấp 2
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultAgency FindAgency()
        {
            // url: /api/rest/findagency
            // method: post

            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/findagency",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultAgency()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestAgency>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var checkUser = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Lỗi");

                var role = checkUser.AspNetRoles.FirstOrDefault();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chức năng này không cho phép bạn sử dụng");

                List<AgencyInfo> agencyResult = new List<AgencyInfo>();
                // type agency: 1: ci. 2: cii
                if (paser.type == "1")
                {
                    List<C1Info> agences = new List<C1Info>();
                    if (role.ShowInfoRole == 1)
                    {
                        agences = db.C1Info.Where(p => p.Code.Contains(paser.search)).ToList();
                    }
                    else if (role.ShowInfoRole == 2)
                    {
                        agences = db.C1Info.Where(p => p.Code.Contains(paser.search) && p.HaiBrandId == staff.BranchId).ToList();
                    }
                    else
                    {
                        agences = staff.C1Info.Where(p => p.Code.Contains(paser.search)).ToList();
                    }


                    foreach (var item in agences)
                    {
                        agencyResult.Add(new AgencyInfo()
                        {
                            code = item.Code,
                            name = item.StoreName
                        });
                    }

                }
                else if (paser.type == "2")
                {
                    List<C2Info> agences = new List<C2Info>();
                    if (role.ShowInfoRole == 1)
                    {
                        agences = db.C2Info.Where(p => p.Code.Contains(paser.search)).ToList();
                    }
                    else if (role.ShowInfoRole == 2)
                    {
                        agences = db.C2Info.Where(p => p.Code.Contains(paser.search) && p.CInfoCommon.BranchCode == staff.HaiBranch.Code).ToList();
                    }
                    else
                    {
                        agences = staff.C2Info.Where(p => p.Code.Contains(paser.search)).ToList();
                    }

                    foreach (var item in agences)
                    {
                        agencyResult.Add(new AgencyInfo()
                        {
                            code = item.Code,
                            name = item.StoreName
                        });
                    }
                }


                result.agences = agencyResult;


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

        #region FindReceiver
        /// <summary>
        /// Lấy danh sách nơi nhận
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ReceiverResult FindReceiver()
        {
            // url: /api/rest/findreceiver
            // method: post

            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/findreceiver",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ReceiverResult()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<ReceiverRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");


                var role = getRole(paser.user);

                if (String.IsNullOrEmpty(role))
                    throw new Exception("Tài khoản không có quyền để truy xuất");

                List<ReceiverInfo> receivers = new List<ReceiverInfo>();

                if (role == "W")
                {
                    // day la kho, thi lay danh sach cac chi nhanh
                    var brand = db.HaiBranches.Where(p => p.Code.Contains(paser.search)).ToList();
                    foreach (var item in brand)
                    {
                        receivers.Add(new ReceiverInfo()
                        {
                            name = item.Name,
                            code = item.Code
                        });
                    }
                }
                else if (role == "B")
                {
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    // day la chi nhanh, lay danh sach cap 1

                    var agencyLevel1 = db.C1Info.Where(p => p.Code.Contains(paser.search) && p.HaiBrandId == staff.BranchId).ToList();
                    foreach (var item in agencyLevel1)
                    {
                        receivers.Add(new ReceiverInfo()
                        {
                            name = item.StoreName,
                            code = item.Code
                        });
                    }
                }
                else if (role == "CI")
                {
                    var c1 = db.C1Info.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();
                    // day la dai ly cap 1, lay danh sách cap 2
                    var agencyLevel2 = db.C2Info.Where(p => p.Code.Contains(paser.search) && p.C1Id == c1.Id).ToList();
                    foreach (var item in agencyLevel2)
                    {
                        receivers.Add(new ReceiverInfo()
                        {
                            name = item.StoreName,
                            code = item.Code
                        });
                    }
                }

                result.receivers = receivers;

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

        private List<ReceiverInfo> getReceiver(string role, string user)
        {
            List<ReceiverInfo> receivers = new List<ReceiverInfo>();

            if (role == "W")
            {
                // day la kho, thi lay danh sach cac chi nhanh
                var brand = db.HaiBranches.ToList();
                foreach (var item in brand)
                {
                    receivers.Add(new ReceiverInfo()
                    {
                        name = item.Name,
                        code = item.Code,
                        type = "Chi nhánh"
                    });
                }
            }
            else if (role == "B")
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                // day la chi nhanh, lay danh sach cap 1

                var agencyLevel1 = db.C1Info.Where(p => p.HaiBrandId == staff.BranchId).ToList();
                foreach (var item in agencyLevel1)
                {
                    receivers.Add(new ReceiverInfo()
                    {
                        name = item.StoreName,
                        code = item.Code,
                        type = "Cấp 1"
                    });
                }
            }
            else if (role == "CI")
            {
                var c1 = db.C1Info.Where(p => p.CInfoCommon.UserLogin == user).FirstOrDefault();
                // day la dai ly cap 1, lay danh sách cap 2
                var agencyLevel2 = db.C2Info.Where(p => p.C1Id == c1.Id).ToList();
                foreach (var item in agencyLevel2)
                {
                    receivers.Add(new ReceiverInfo()
                    {
                        name = item.StoreName,
                        code = item.Code,
                        type = "Cấp 2"
                    });
                }

            }

            return receivers;
        }

        #endregion

        #region updateProduct
        /// <summary>
        /// san pham
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ResultProduct updateProduct()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/updateproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultProduct()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestProduct>(requestContent);
                //  log.Content = new JavaScriptSerializer().Serialize(paser);

                var check = db.APIAuthHistories.Where(p => p.UserLogin == paser.user && p.Token == paser.token && p.IsExpired == 0).FirstOrDefault();
                if (check == null)
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");

                WavehouseInfo wareActionInfo = new WavehouseInfo();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff != null)
                {
                    wareActionInfo.wCode = staff.HaiBranch.Code;
                    wareActionInfo.wName = staff.HaiBranch.Name;

                    var userStaff = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                    var role = userStaff.AspNetRoles.FirstOrDefault();

                    if (role.Name == "Warehouse")
                        wareActionInfo.wType = "W";
                    else
                        wareActionInfo.wType = "B";
                }
                else
                {
                    var agency = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    if (agency != null)
                    {
                        wareActionInfo.wCode = agency.CCode;
                        wareActionInfo.wName = agency.CName;
                        wareActionInfo.wType = agency.CType;
                    }
                }

                // check user receiver
                WavehouseInfo wareReceiInfo = null;
                // lay thong tin noi nhan
                if (!String.IsNullOrEmpty(paser.receiver.Trim()) && paser.status == "XK")
                {
                    wareReceiInfo = new WavehouseInfo();
                    var branchReceiver = db.HaiBranches.Where(p => p.Code == paser.receiver).FirstOrDefault();
                    if (branchReceiver != null)
                    {
                        wareReceiInfo.wType = "B";
                        wareReceiInfo.wCode = branchReceiver.Code;
                        wareReceiInfo.wName = branchReceiver.Name;
                    }
                    else
                    {
                        var agencyReceiver = db.CInfoCommons.Where(p => p.CCode == paser.receiver).FirstOrDefault();
                        if (agencyReceiver != null)
                        {
                            wareReceiInfo.wType = agencyReceiver.CType;
                            wareReceiInfo.wCode = agencyReceiver.CCode;
                            wareReceiInfo.wName = agencyReceiver.CName;
                        }

                    }

                    if (String.IsNullOrEmpty(wareReceiInfo.wCode) || wareActionInfo.wCode == wareReceiInfo.wCode)
                        throw new Exception("Sai thông tin nơi nhập hàng");
                }

                ProductScanAction wareAction = null;

                if (paser.status == "NK")
                {
                    wareAction = new ImportAction(db, wareActionInfo, paser.user, null);
                }
                else if (paser.status == "XK")
                {
                    wareAction = new ExportAction(db, wareActionInfo, paser.user, wareReceiInfo);
                }

                if (wareAction == null)
                    throw new Exception("Sai thông tin quét");

                string msg = wareAction.checkRole();
                if (msg != null)
                    throw new Exception(msg);

                // kiem tra nhap kho
                List<GeneralInfo> productsReturn = new List<GeneralInfo>();
                foreach (string barcode in paser.products)
                {
                    BarcodeHistory resultSave = wareAction.Handle(barcode);
                    db.BarcodeHistories.Add(resultSave);
                    db.SaveChanges();
                    productsReturn.Add(new GeneralInfo()
                    {
                        code = resultSave.Barcode,
                        name = resultSave.ProductName,
                        status = resultSave.Messenge,
                        success = Convert.ToInt32(resultSave.IsSuccess)
                    });

                }

                result.products = productsReturn;
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

        #region Staff import for agency

        [HttpPost]
        public ResultInfo checkLocationDistance()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/checklocationdistance",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };
            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestCheckLocationDistance>(requestContent);
                //  log.Content = new JavaScriptSerializer().Serialize(paser);

                var check = db.APIAuthHistories.Where(p => p.UserLogin == paser.user && p.Token == paser.token && p.IsExpired == 0).FirstOrDefault();

                if (check == null)
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");


                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                if (staff == null)
                {
                    throw new Exception("Nhân viên mới được dùng chức năng này");
                }

                var agency = db.CInfoCommons.Where(p => p.CCode == paser.agency).FirstOrDefault();

                if (agency == null)
                {
                    throw new Exception("Sai mã đại lý");
                }

                var distance = getDistanceTwoLocation(Convert.ToDouble(paser.latitude), Convert.ToDouble(paser.longitude), Convert.ToDouble(agency.Lat), Convert.ToDouble(agency.Lng));

                if (distance > 200)
                {
                    result.id = "2";
                    result.msg = "Ngoài đại lý";
                }
                else
                {
                    result.id = "1";
                    result.msg = "Tại đại lý";
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


        [HttpPost]
        public ResultProduct helpAgencyImport()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                APIUrl = "/api/rest/helpagencyimport",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultProduct()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestProductHelp>(requestContent);
                //  log.Content = new JavaScriptSerializer().Serialize(paser);
                /*
                var check = db.APIAuthHistories.Where(p => p.UserLogin == paser.user && p.Token == paser.token && p.IsExpired == 0).FirstOrDefault();

                if (check == null)
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");
                */
                WavehouseInfo wareActionInfo = new WavehouseInfo();
                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                if (staff == null)
                {
                    throw new Exception("Chỉ nhân viên công ty mới sử dụng chức năng này");
                }

                var agency = db.CInfoCommons.Where(p => p.CCode == paser.agency).FirstOrDefault();

                if (agency != null)
                {
                    wareActionInfo.wCode = agency.CCode;
                    wareActionInfo.wName = agency.CName;
                    wareActionInfo.wType = agency.CType;
                }
                else
                {
                    throw new Exception("Sai mã khách hàng");
                }

                // kiem tra toa do
                ProductScanAction wareAction = new ImportAction(db, wareActionInfo, paser.user, null);

                string msg = wareAction.checkRole();
                if (msg != null)
                    throw new Exception(msg);

                // kiem tra nhap kho
                List<GeneralInfo> productsReturn = new List<GeneralInfo>();
                foreach (string barcode in paser.products)
                {
                    BarcodeHistory resultSave = wareAction.Handle(barcode);
                    resultSave.StaffHelp = staff.Code;
                    if (paser.nearAgency == 1)
                    {
                        resultSave.StaffHelpIssue = "locationin";
                    }
                    else
                    {
                        resultSave.StaffHelpIssue = "locationout";
                    }
                    resultSave.LocationStaff = paser.latitude + "|" + paser.longitude;
                    db.BarcodeHistories.Add(resultSave);
                    db.SaveChanges();

                    productsReturn.Add(new GeneralInfo()
                    {
                        code = resultSave.Barcode,
                        name = resultSave.ProductName,
                        status = resultSave.Messenge,
                        success = Convert.ToInt32(resultSave.IsSuccess)
                    });
                }

                result.products = productsReturn;
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

        private double getDistanceTwoLocation(double sLatitude, double sLongitude, double eLatitude, double eLongitude)
        {
            var sCoord = new GeoCoordinate(sLatitude, sLongitude);
            var eCoord = new GeoCoordinate(eLatitude, eLongitude);

            return sCoord.GetDistanceTo(eCoord);
        }

        #endregion

        #region Tracking
        /// <summary>
        /// --------------------------------
        /// </summary>
        /// <returns></returns>
        // tracking san pham
        [HttpPost]
        public ResultTracking Tracking()
        {
            var log = new APIHistory()
           {
               Id = Guid.NewGuid().ToString(),
               APIUrl = "/api/rest/tracking",
               CreateTime = DateTime.Now,
               Sucess = 1
           };

            var result = new ResultTracking()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestTracking>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                var check = db.APIAuthHistories.Where(p => p.UserLogin == paser.user && p.Token == paser.token && p.IsExpired == 0).FirstOrDefault();

                // if (check == null)
                //  throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");

                result.code = paser.code;

                var checkProduct = GetProduct(paser.code);

                if (checkProduct == null)
                    throw new Exception("Mã không hợp lệ");


                result.name = checkProduct.PName;

                var boxCode = paser.code.Substring(0, 15);

                List<PTrackingInfo> productTracking = new List<PTrackingInfo>();

                var pHistoryTracking = db.PTrackings.Where(p => p.CaseCode == boxCode).OrderBy(p => p.ImportTime).ToList();

                foreach (var item in pHistoryTracking)
                {
                    PTrackingInfo pTracking = new PTrackingInfo();

                    if (item.WType == "CI")
                        pTracking.name = "CẤP 1: " + item.WName;
                    else if (item.WType == "CII")
                        pTracking.name = "CẤP 2: " + item.WName;
                    else if (item.WType == "B")
                        pTracking.name = "CHI NHÁNH: " + item.WName;
                    else if (item.WType == "FARMER")
                        pTracking.name = "NÔNG DÂN: " + item.WName;
                    else if (item.WType == "W")
                        pTracking.name = "TỔNG KHO: " + item.WName;

                    if (item.ImportTime != null)
                        pTracking.importTime = "Nhập kho lúc " + item.ImportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.importTime = "Chưa nhập kho";

                    if (item.ExportTime != null)
                        pTracking.exportTime = "Xuất kho lúc " + item.ExportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.exportTime = "Chưa xuất kho";


                    if (item.Quantity == 1 || item.Quantity == 0)
                    {
                        pTracking.status = Convert.ToInt32(item.Quantity) + " thùng";
                    }
                    else
                    {
                        int quantity = Convert.ToInt32(item.Quantity * item.ProductInfo.QuantityBox);
                        pTracking.status = quantity + " hộp";
                    }

                    productTracking.Add(pTracking);
                }

                result.tracking = productTracking;

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

        private ProductInfo GetProduct(string barcode)
        {

            if (String.IsNullOrEmpty(barcode))
                return null;

            if (barcode.Length < 17)
                return null;

            string countryCode = barcode.Substring(0, 3);
            if (countryCode != "893")
                return null;

            string companyCode = barcode.Substring(3, 5);
            if (companyCode != "52433")
                return null;

            string productCode = barcode.Substring(8, 2);

            var product = db.ProductInfoes.Where(p => p.Barcode == productCode).FirstOrDefault();

            return product;

        }
        #endregion

        #region Methoh checkStaff
        /// <summary>
        /// kiem tra nhan vien
        /// </summary>
        /// <param name="code"></param>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [HttpGet]
        public ResultCheckStaff checkStaff(string code, string user, string token)
        {
            // check sesion for login
            // /api/rest/loginsession
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/checkstaff",
                Sucess = 1,
                Content = "code : " + code + " ; user : " + user + " token : " + token
            };

            var result = new ResultCheckStaff()
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
                    var serverInfo = db.ServerInfoes.Where(p => p.Code == "staffimage").FirstOrDefault();
                    if (serverInfo != null)
                    {
                        result.avatar = serverInfo.Content + staff.AvatarUrl;
                        result.signature = serverInfo.Content + staff.SignatureUrl;
                    }
                    else
                    {
                        result.avatar = HaiUtil.HostName + staff.AvatarUrl;
                        result.signature = HaiUtil.HostName + staff.SignatureUrl;
                    }

                    result.id = "1";
                    result.msg = "success";
                    result.status = staff.Notes;

                }
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            db.APIHistories.Add(history);
            db.SaveChanges();

            return result;
        }
        #endregion

        #region Methob msgToHai
        /// <summary>
        /// gui messenage toi HAI
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ResultInfo msgToHai()
        {
            // update regid firebase
            // /api/rest/updatereg
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/msgtohai",
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestMSGToHai>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var msgInfo = new MessegeToHai()
                {
                    Id = Guid.NewGuid().ToString(),
                    Messenge = paser.content,
                    MsgType = paser.type,
                    UserLogin = paser.user,
                    CreateTime = DateTime.Now
                };

                db.MessegeToHais.Add(msgInfo);
                db.SaveChanges();

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;
        }
        #endregion

        #region Methob loyaltyEvent
        /// <summary>
        /// khuyen mai
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultEvent loyaltyEvent()
        {
            // get all event
            // /api/rest/loyaltyevent
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/loyaltyevent",
                Sucess = 1
            };

            var result = new ResultEvent()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                var dateNow = DateTime.Now;
                if (cInfo != null)
                {
                    // lay danh sach sự kiện phù hợp với user (theo khu vực)
                    List<EventInfo> listEventUser = new List<EventInfo>();
                    if (cInfo.CType == "CII")
                    {
                        var eventArea = (from log in db.EventAreas
                                         where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                       <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                       >= DbFunctions.TruncateTime(dateNow))
                                         select log).ToList();
                        foreach (var item in eventArea)
                        {
                            var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                            if (cusJoin.Count() > 0)
                            {
                                var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                                if (cJoin != null)
                                    listEventUser.Add(item.EventInfo);
                            }
                            else
                            {
                                listEventUser.Add(item.EventInfo);
                            }
                        }

                    }
                    else if (cInfo.CType == "FARMER")
                    {
                        var eventArea = (from log in db.EventAreaFarmers
                                         where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                       <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                       >= DbFunctions.TruncateTime(dateNow))
                                         select log).ToList();

                        foreach (var item in eventArea)
                        {
                            var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                            if (cusJoin.Count() > 0)
                            {
                                var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                                if (cJoin != null)
                                    listEventUser.Add(item.EventInfo);
                            }
                            else
                            {
                                listEventUser.Add(item.EventInfo);
                            }
                        }
                    }


                    List<Event> listEventInfo = new List<Event>();

                    foreach (var item in listEventUser)
                    {
                        listEventInfo.Add(new Event()
                        {
                            eid = item.Id,
                            ename = item.Name,
                            etime = item.BeginTime.Value.ToShortDateString() + " - " + item.EndTime.Value.ToShortDateString(),
                            eimage = HaiUtil.HostName + item.Thumbnail
                        });
                    }

                    result.events = listEventInfo;
                }
                else if (staff != null)
                {

                    var listEvent = db.EventAreas.Where(p => p.EventInfo.ESTT == 1 && p.AreaId == staff.HaiBranch.AreaId).ToList().OrderByDescending(p => p.EventInfo.BeginTime);

                    List<Event> listEventInfo = new List<Event>();

                    foreach (var item in listEvent)
                    {
                        listEventInfo.Add(new Event()
                        {
                            eid = item.EventInfo.Id,
                            ename = item.EventInfo.Name,
                            etime = item.EventInfo.BeginTime.Value.ToShortDateString() + " - " + item.EventInfo.EndTime.Value.ToShortDateString(),
                            eimage = HaiUtil.HostName + item.EventInfo.Thumbnail
                        });
                    }

                    result.events = listEventInfo;

                }
                else throw new Exception("Tài khoản không thuộc của HAI.");
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;
        }
        #endregion

        #region Methob eventDetail
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public EventDetail eventDetail()
        {
            // get event detail
            // /api/rest/eventDetail
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/eventdetail",
                Sucess = 1
            };

            var result = new EventDetail()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<EventDetailRequest>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var eventInfo = db.EventInfoes.Find(paser.eventId);

                if (eventInfo == null)
                    throw new Exception("Thông tin gửi lên không chính xác.");


                List<ResultEventAward> awards = new List<ResultEventAward>();

                List<string> areas = new List<string>();

                List<ResultEventProduct> products = new List<ResultEventProduct>();

                var listProduct = db.EventProducts.Where(p => p.EventId == eventInfo.Id).ToList();

                foreach (var item in listProduct)
                {
                    products.Add(new ResultEventProduct()
                    {
                        name = item.ProductInfo.PName,
                        point = item.Point + ""
                    });
                }

                var listArea = db.EventAreas.Where(p => p.EventId == eventInfo.Id).ToList();
                foreach (var item in listArea)
                {
                    areas.Add(item.HaiArea.Name);
                }

                var listAward = eventInfo.AwardInfoes.ToList();
                foreach (var item in listAward)
                {
                    awards.Add(new ResultEventAward()
                    {
                        name = item.Name,
                        point = item.Point + "",
                        image = HaiUtil.HostName + item.Thumbnail
                    });
                }


                result.eid = eventInfo.Id;
                result.eimage = HaiUtil.HostName + eventInfo.Thumbnail;
                result.ename = eventInfo.Name;
                result.etime = eventInfo.BeginTime.Value.ToShortDateString() + " - " + eventInfo.EndTime.Value.ToShortDateString();
                result.edescribe = eventInfo.Descibe;
                result.areas = areas;
                result.awards = awards;
                result.products = products;
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }



            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;
        }
        #endregion

        #region Methob sendCodeEvent
        /// <summary>
        /// ------------------------------------------------------
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultCodeEvent sendCodeEvent()
        {
            // send code 
            // /api/rest/sendcodeevent

            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/sendcodeevent",
                Sucess = 1
            };

            var result = new ResultCodeEvent()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestCodeEvent>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (cInfo == null)
                    throw new Exception("Chương trình chỉ dành cho khách hàng của HAI.");

                if (cInfo.CType == "CI")
                    throw new Exception("Chương trình chỉ dành cho khách hàng là đại lý cấp 2, nông dân của HAI.");


                var dateNow = DateTime.Now;
                // lay danh sach sự kiện phù hợp với user (theo khu vực)
                List<EventInfo> listEventUser = new List<EventInfo>();
                if (cInfo.CType == "CII")
                {
                    // var eventArea = db.EventAreas.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                    var eventArea = (from log in db.EventAreas
                                     where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                   <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                   >= DbFunctions.TruncateTime(dateNow))
                                     select log).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            //var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id && p.IsJoint == 0).FirstOrDefault();
                            //  if (cJoin == null)
                            listEventUser.Add(item.EventInfo);
                        }
                    }

                }
                else if (cInfo.CType == "FARMER")
                {
                    // var eventArea = db.EventAreaFarmers.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                    var eventArea = (from log in db.EventAreaFarmers
                                     where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                   <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                   >= DbFunctions.TruncateTime(dateNow))
                                     select log).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            listEventUser.Add(item.EventInfo);
                        }
                    }

                }


                if (listEventUser.Count() == 0)
                    throw new Exception("Không tìm thấy chương trình khuyến mãi nào phù hợp.");


                List<GeneralInfo> listCodeReturn = new List<GeneralInfo>();

                Hashtable map = new Hashtable();

                foreach (var item in paser.codes)
                {
                    try
                    {
                        // lây mã seri // eventCode or seri
                        var product = GetProduct(item);

                        if (product == null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = "Không phải sản phẩm HAI",
                                code = item,
                                status = "Mã không hợp lệ"
                            });
                            throw new Exception();
                        }

                        var caseCode = item.Substring(0, 15);
                        var boxCode = item.Substring(0, 16);

                        // kiem tra la da nhap kho chua
                        PHistory pHis = null;
                        if (product.IsBox == 1)
                            pHis = db.PHistories.Where(p => p.BoxCode == boxCode && p.WCode == cInfo.CCode && p.PStatus == "NK").FirstOrDefault();
                        else
                            pHis = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode == cInfo.CCode && p.PStatus == "NK").FirstOrDefault();

                        if (pHis == null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = product.PName,
                                code = item,
                                status = "Mã chưa nhập kho"
                            });
                            throw new Exception();
                        }

                        // kiem tra la da su dung chua

                        var checkUse = db.MSGPoints.Where(p => p.Barcode == item).FirstOrDefault();
                        if (checkUse != null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = product.PName,
                                code = item,
                                status = "Mã đã được sử dụng"
                            });
                            throw new Exception();
                        }

                        // cap nhat lịch su
                        var hEvent = new MSGPoint()
                        {
                            Id = Guid.NewGuid().ToString(),
                            AcceptTime = DateTime.Now,
                            CInfoId = cInfo.Id,
                            UserLogin = paser.user,
                            ProductId = product.Id,
                            Barcode = item,
                            MSGType = "APP"
                        };


                        List<MSGPointEvent> listPointEvent = new List<MSGPointEvent>();
                        foreach (var userEvent in listEventUser)
                        {

                            // kiem tra san pham co trong su kien nay ko
                            var productEvent = userEvent.EventProducts.Where(p => p.ProductId == product.Id).FirstOrDefault();
                            if (productEvent != null)
                            {

                                var pointEvemt = new MSGPointEvent()
                                {
                                    EventId = userEvent.Id,
                                    MSGPointId = hEvent.Id,
                                    Point = productEvent.Point
                                };
                                listPointEvent.Add(pointEvemt);

                                // tra thong tin ve client
                                listCodeReturn.Add(new GeneralInfo()
                                {
                                    code = item,
                                    name = product.PName,
                                    status = productEvent.EventInfo.Name + " + " + productEvent.Point + "***"
                                });

                            }
                        }
                        //

                        if (listPointEvent.Count() > 0)
                        {

                            db.MSGPoints.Add(hEvent);

                            db.SaveChanges();

                            foreach (var pevent in listPointEvent)
                            {

                                if (map.ContainsKey(pevent.EventId))
                                {
                                    var oldPoint = Convert.ToInt32(map[pevent.EventId]);
                                    map[pevent.EventId] = oldPoint + pevent.Point;
                                }
                                else
                                {
                                    map.Add(pevent.EventId, Convert.ToInt32(pevent.Point));
                                }

                                db.MSGPointEvents.Add(pevent);

                                // luu diem cho khach hang
                                var agencyPoint = db.AgencySavePoints.Where(p => p.EventId == pevent.EventId && p.CInfoId == cInfo.Id).FirstOrDefault();

                                if (agencyPoint == null)
                                {
                                    var newAgencyPoint = new AgencySavePoint()
                                    {
                                        EventId = pevent.EventId,
                                        CInfoId = cInfo.Id,
                                        PointSave = pevent.Point,
                                        CreateTime = DateTime.Now
                                    };
                                    db.AgencySavePoints.Add(newAgencyPoint);
                                }
                                else
                                {
                                    var newPoint = agencyPoint.PointSave + pevent.Point;
                                    agencyPoint.PointSave = newPoint;
                                    agencyPoint.ModifyTime = DateTime.Now;
                                    db.Entry(agencyPoint).State = System.Data.Entity.EntityState.Modified;
                                }

                                db.SaveChanges();
                            }
                        }



                    }
                    catch
                    {

                    }
                }

                string pointEvent = "";
                int countPoint = 0;
                foreach (var item in map.Keys)
                {
                    var eventInfo = db.EventInfoes.Find(item);

                    var savePoint = eventInfo.AgencySavePoints.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();
                    int? point = 0;
                    if (savePoint != null)
                        point = savePoint.PointSave;

                    string aWard = "";
                    var listAward = eventInfo.AwardInfoes.ToList();
                    foreach (var aWardItem in listAward)
                    {
                        if (aWardItem.Point <= point)
                        {
                            aWard += ", " + aWardItem.Name + "(" + aWardItem.Point + "điểm )";
                        }
                    }

                    var nameEvent = eventInfo.Name;

                    pointEvent = pointEvent + ";" + nameEvent + " (" + map[item] + " điểm)";

                    if (!String.IsNullOrEmpty(aWard))
                    {
                        aWard = aWard.Remove(0, 2);
                        pointEvent = pointEvent + " - phần thưởng đổi: " + aWard;
                    }
                    else
                    {
                        pointEvent = pointEvent + "-chưa đủ điểm đổi quà";
                    }

                    countPoint += Convert.ToInt32(map[item]);
                }
                string msgReturn = "Cộng " + countPoint;

                msgReturn = msgReturn + pointEvent;

                result.msg = msgReturn;

                result.codes = listCodeReturn;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;

        }
        #endregion

        #region Methob UserInfo
        /// <summary>
        /// thong tin tai khoan
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultUserInfo UserInfo()
        {

            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/userinfo",
                Sucess = 1
            };

            var result = new ResultUserInfo()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();


                if (cInfo != null)
                {
                    if (cInfo.CType == "CI")
                        result.type = "Đại lý cấp 1";
                    else if (cInfo.CType == "CII")
                        result.type = "Đại lý cấp 2";
                    else if (cInfo.CType == "FARMER")
                        result.type = "Nông dân";

                    result.fullname = cInfo.CName;
                    result.phone = cInfo.Phone;
                    result.address = cInfo.AddressInfo;
                    result.user = paser.user;
                    result.area = cInfo.HaiArea.Name;
                }
                else if (staff != null)
                {
                    result.type = "Nhân viên";
                    result.fullname = staff.FullName;
                    result.phone = staff.Phone;
                    result.address = staff.HaiBranch.Name;
                    result.area = staff.HaiBranch.HaiArea.Name;
                    result.user = paser.user;
                }
                else
                    throw new Exception("Tài khoản không hợp lệ");


            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;

        }
        #endregion

        #region Methob notification
        // notification
        [HttpPost]
        public NotificationResult GetNotification()
        {
            // send code 
            // /api/rest/sendcodeevent

            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/rest/getnotification",
                Sucess = 1
            };

            var result = new NotificationResult()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<NotificationRequest>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                List<NotificationInfo> notificationInfoes = new List<NotificationInfo>();

                int pageSize = 10;

                var listTopics = getTopics(paser.user);

                var listNotification = db.BasicNotifications.Where(p => p.UserAccept == paser.user || listTopics.Contains(p.TocpicCode)).OrderByDescending(p => p.CreateDate).ToList();

                var pageCount = Math.Ceiling(1.0 * listNotification.Count / pageSize);

                if (paser.pageno < pageCount)
                {

                    string urlServer = "";
                    var serverInfo = db.ServerInfoes.Where(p => p.Code == "upload").FirstOrDefault();
                    if (serverInfo != null)
                        urlServer = serverInfo.Content;


                    var listLoad = listNotification.Skip(paser.pageno * pageSize).Take(pageSize);

                    foreach (var item in listLoad)
                    {
                        notificationInfoes.Add(new NotificationInfo()
                        {
                            title = item.Title,
                            content = item.Messenge,
                            id = item.Id,
                            time = item.CreateDate.Value.ToString("dd/MM/yyyy HH:mm"),
                            image = urlServer + item.ImageAttach
                        });
                    }

                }

                result.pageno = paser.pageno;
                result.pagemax = Convert.ToInt32(pageCount);
                result.notifications = notificationInfoes;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }


            db.APIHistories.Add(history);
            db.SaveChanges();
            return result;
        }
    }
        #endregion

}
