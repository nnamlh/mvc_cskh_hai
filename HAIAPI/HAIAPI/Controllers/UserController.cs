﻿using HAIAPI.Models;
using HAIAPI.Util;
using SMSUtl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class UserController : RestMainController
    {
        #region Login
        /// <summary>
        /// danh cho login logout
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<LoginResult> Login(string imei = "")
        {
            // login
            // /api/rest/login
            // method: get
            HttpRequestHeaders headers = Request.Headers;

            LoginResult check = await Auth(headers, imei);

            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/user/login",
                ReturnInfo = new JavaScriptSerializer().Serialize(check)
            };

            if (check.id == "0")
            {
                history.Sucess = 0;
            }
            else history.Sucess = 1;

            mongoHelper.createHistoryAPI(history);

            return check;
        }

        [HttpGet]
        public CheckUserLoginResult CheckUserLogin()
        {
            // login
            // /api/rest/checkuserlogin
            // method: get

            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/user/checkuserlogin"
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

            var result = new CheckUserLoginResult()
            {
                id = "1",
                msg = "success",
                user = user
            };

            try
            {
                var checkUser = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Tài khoản không hợp lệ");

                if (checkUser.AccountType == "STAFF")
                {
                    var haiStaff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

                    if (haiStaff != null)
                    {
                        if (haiStaff.IsLock == 1)
                        {
                            throw new Exception("Tài khoản đang tạm khóa");
                        }

                        result.id = "1";
                    }
                    else
                        throw new Exception("Tài khoản không hợp lệ");
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

                                var info = updateAuth(user);

                                result.role = info.Role;
                                result.token = info.token;
                                result.type = info.type;
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
            mongoHelper.createHistoryAPI(history);

            return result;
        }

        [HttpGet]
        public LoginResult LoginActivaton()
        {
            // login
            // /api/rest/loginactivaton
            // method: get

            var result = new LoginResult()
            {
                id = "1",
                msg = "success"
            };


            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/user/loginactivaton"
            };

            try
            {
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


                var check = db.SMSCodes.Where(p => p.UserLogin == user && p.Code == otp && p.CStatus == 0).FirstOrDefault();

                if (check == null)
                    throw new Exception("Không thể đăng nhập vui lòng thử lại");

                check.CStatus = 1;
                db.Entry(check).State = EntityState.Modified;
                db.SaveChanges();


                var info = updateAuth(user);

                result.Role = info.Role;
                result.token = info.token;
                result.type = info.type;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message; ;
            }

            return result;
        }


        private UserLoginInfo updateAuth(string user)
        {
            var userData = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

            var userRole = userData.AspNetRoles.FirstOrDefault();

            var authToke = Guid.NewGuid().ToString();

            mongoHelper.checkAndCreateAutHistory(user, authToke, userRole.Name, "", "", "");

            return new UserLoginInfo()
            {
                Role = userRole.Name,
                token = authToke,
                user = user,
                type = userData.AccountType
            };
        }

        #endregion
        #region Logout
        /// <summary>
        /// --------------------------------// logout
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResultInfo Logout()
        {
            // logout
            // /api/rest/logout
            // method: post
            var log = new MongoHistoryAPI()
            {
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
                HttpRequestHeaders headers = Request.Headers;
                if (!headers.Contains("Authorization"))
                {
                    throw new Exception("Nead authorization info");
                }

                string content;

                try
                {
                    string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                    content = XString.FromBase64(base64Auth);
                }
                catch
                {
                    throw new Exception("Wrong authorization info");
                }

                var arrtok = content.Split(':');

                if (arrtok.Length != 2)
                    throw new Exception("Wrong authorization format");

                string user = arrtok[0];
                string token = arrtok[1];

                // check user
                var checkUser = db.AspNetUsers.Where(p => p.UserName == user).FirstOrDefault();

                if (checkUser == null)
                    throw new Exception("Không thể đăng xuất");

                //
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản đang đăng nhập trên thiết bị khác");

                // xoa firebase id
                var regFirebase = db.RegFirebases.Where(p => p.UserLogin == user).FirstOrDefault();

                if (regFirebase != null)
                {
                    regFirebase.RegId = "";
                    regFirebase.ModifyDate = DateTime.Now;
                    db.Entry(regFirebase).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                mongoHelper.updateStateAuthToken(user);

                mongoHelper.saveLogout(user, token);

            }
            catch
            {

            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }
        #endregion

        #region LoginSession
        /// <summary>
        /// --------------------------------
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResultInfo LoginSession(string user, string token, string version)
        {

            // check sesion for login
            // /api/rest/loginsession
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/user/loginsession",
                Content = user + "|" + token,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1"
            };

            string versionCurrent = ConfigurationManager.AppSettings["VersionApp"];

            if (version != versionCurrent)
            {
                result.id = "2";
                result.msg = "Cập nhật phiên bản mới";
                history.Sucess = 0;
            }
            else if (!mongoHelper.checkLoginSession(user, token))
            {
                result.id = "0";
                result.msg = "Tài khoản bạn đã đăng nhập ở thiết bị khác.";
                history.Sucess = 0;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(history);

            return result;

        }
        #endregion


        #region Auth
        private async Task<LoginResult> Auth(HttpRequestHeaders headers, string imei)
        {

            if (!headers.Contains("Authorization"))
            {
                return new LoginResult() { id = "0", msg = "Nead authorization info" };
            }

            string token;

            try
            {
                string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                token = XString.FromBase64(base64Auth);
            }
            catch
            {
                return new LoginResult { id = "0", msg = "Wrong authorization info" };
            }

            var arrtok = token.Split(':');

            if (arrtok.Length != 2)
                return new LoginResult { id = "0", msg = "Wrong authorization format" };

            string UserName = arrtok[0];
            string PassWord = arrtok[1];

            var user = await UserManager.FindAsync(UserName, PassWord);

            if (user == null)
            {
                return new LoginResult { id = "0", msg = "User or Password not correct", user = UserName };
            }

            var userData = db.AspNetUsers.Find(user.Id);
            bool isActive = false;

            if (userData.AccountType == "STAFF")
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user.UserName).FirstOrDefault();
                if (staff.IsLock != 1)
                    isActive = true;
            }
            else
            {
                if (userData.AccountType == "CII")
                {
                    var checkC2 = db.C2Info.Where(p => p.CInfoCommon.UserLogin == user.UserName).FirstOrDefault();
                    if (checkC2 != null)
                    {
                        if (checkC2.IsActive == 0)
                        {
                            isActive = false;
                        }
                    }
                }
                else if (userData.AccountType == "CI")
                {
                    var checkC1 = db.C1Info.Where(p => p.CInfoCommon.UserLogin == user.UserName).FirstOrDefault();
                    if (checkC1 != null)
                    {
                        if (checkC1.IsActive == 0)
                        {
                            isActive = false;
                        }
                    }
                }
            }
            if (!isActive)
                return new LoginResult { id = "0", msg = "Tài khoản bị khóa.", user = UserName };

            var userRole = userData.AspNetRoles.FirstOrDefault();

            if (userRole == null)
            {
                return new LoginResult { id = "0", msg = "User is not permission.", user = UserName };
            }
            var authToke = Guid.NewGuid().ToString();

            mongoHelper.checkAndCreateAutHistory(user.UserName, authToke, userRole.Name, "", "", "");

            return new LoginResult { id = "1", msg = "login success", token = authToke, user = UserName, Role = userRole.Name, type = userData.AccountType };
        }
        #endregion
    }
}
