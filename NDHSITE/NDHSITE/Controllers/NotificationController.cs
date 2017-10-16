using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using System.Net;
using PagedList;
using System.IO;
using SMSUtl;
using NDHSITE.Util;

namespace NDHSITE.Controllers
{ 
    
    public class NotificationController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        MongoHelper mongoHelp = new MongoHelper();
        //
        // GET: /Notification/
        [Authorize]
        public ActionResult Send(string msg = null)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.MSG = msg;

            ViewBag.Areas = db.HaiAreas.ToList();
            ViewBag.Branches = db.HaiBranches.ToList();

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Send(string title, string messenge, string content, string user, string type, string area, string branch, string group)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");

            string sendTo = "";
            string NType = "";
            string NCode = "";
            string Id = Guid.NewGuid().ToString();

            if (type == "ID")
            {
                NCode = user;
                NType = "ID";
                var firebaseInfo = db.RegFirebases.Where(p => p.UserLogin == user).FirstOrDefault();

                if (firebaseInfo == null)
                    return RedirectToAction("send", "notification", new { msg = "Nhân viên chưa sử dụng APP." });

                sendTo = firebaseInfo.RegId;
            }
            else
            {
                NType = "TOPIC";
                sendTo = "/topics/";
                if (group == "2")
                {
                    // theo khu vuc
                    sendTo += type + area;
                    NCode = type + area;
                }
                else if (group == "3")
                {
                    // theo chi nhanh
                    sendTo += type + branch;
                    NCode = type + branch;
                }
                else
                {
                    sendTo += type + "ALL";
                    NCode = type + "ALL";
                }
            }
            title = title.ToUpper();
            string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + title + "\",\"body\": \"" + messenge + "\"},\"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"" + sendTo + "\"}";

            var responseString = sendRequestFirebase(json);

            MongoNotificationHistory notification = new MongoNotificationHistory()
            {
                GuiId = Id,
                Title = title,
                Messenge = messenge,
                NType = NType,
                CreateTime = DateTime.Now,
                MessengeResult = responseString,
                Content = content,
                Success = 1,
                NCode = new List<string>(),
                UserRead = new List<string>()
            };

            notification.NCode.Add(NCode);
            mongoHelp.saveNotificationHistory(ref notification);

            return RedirectToAction("send", "notification", new { msg = "Đã gửi " + notification.GuiId });
        }
        [Authorize]
        private string sendRequestFirebase(string json)
        {
            string url = @"https://fcm.googleapis.com/fcm/send";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.Headers["Authorization"] = "key=AAAAIr_-D4Q:APA91bHvZRkw_Y0UHBoPa3-HUq1iN41Y5Bhtta0MuSxMBEazsvxLPZM4kIgufKkmvp-3yWMKy6QK9l4wTJd-eymKdJtOFjVQEwJmswqp4YseGq-ylFPvkwOsE3NzpbV6kJEQubWBBUeP";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(json);

            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            long length = 0;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    length = response.ContentLength;
                    var stream = response.GetResponseStream();
                    var reader = new StreamReader(stream, encoding);
                    var responseString = reader.ReadToEnd();

                    return responseString;
                }
            }
            catch
            {
                return null;
            }

        }

        [HttpGet]
        public ActionResult Show(string id)
        {
            return View(mongoHelp.getNotification(id));
        }

        /*
        [HttpPost]
        public ActionResult SendAgency(HttpPostedFileBase files ,string title, string messenge, string userlogin)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");

            var checkUser = db.CInfoCommons.Where(p => p.UserLogin == userlogin).FirstOrDefault();


            if (checkUser == null)
            {
                return RedirectToAction("send", "notification", new { msg = "User phải là của khách hàng." });
            }

            var regId = db.RegFirebases.Where(p => p.UserLogin == userlogin).FirstOrDefault();

            if (regId == null)
            {
                return RedirectToAction("send", "notification", new { msg = "Khách hàng chưa sử dụng APP." });
            }


            var msgSend = messenge;

            if (messenge.Length > 100)
            {
                msgSend = messenge.Substring(0, 100) + "...";
            }

            var titleSend = title.ToUpper();

            string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + titleSend + "\",\"body\": \"" + msgSend + "\"},\"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"" + regId.RegId + "\"}";

            var responseString = sendRequestFirebase(json);

            BasicNotification notification = new BasicNotification()
            {
                Id = Guid.NewGuid().ToString(),
                Title = titleSend,
                Messenge = messenge,
                UserSend = User.Identity.Name,
                CreateDate = DateTime.Now,
                MessengeResult = responseString,
                UserAccept = userlogin,
                TocpicCode = "cus",
                ImageAttach = updateAttach(files)
            };

            db.BasicNotifications.Add(notification);
            db.SaveChanges();
            return RedirectToAction("send", "notification", new { msg = "Đã gửi" });
        }


        [HttpPost]
        public ActionResult SendStaff(HttpPostedFileBase files, string title, string messenge, string userlogin)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");

            var checkUser = db.HaiStaffs.Where(p => p.UserLogin == userlogin).FirstOrDefault();


            if (checkUser == null)
            {
                return RedirectToAction("send", "notification", new { msg = "User phải là của nhân viên." });
            }

            var regId = db.RegFirebases.Where(p => p.UserLogin == userlogin).FirstOrDefault();

            if (regId == null)
            {
                return RedirectToAction("send", "notification", new { msg = "Nhân viên chưa sử dụng APP." });
            }

            var msgSend = messenge;

            if (messenge.Length > 100)
            {
                msgSend = messenge.Substring(0, 100) + "...";
            }

            var titleSend = title.ToUpper();

            string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + titleSend + "\",\"body\": \"" + msgSend + "\"},\"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"" + regId.RegId + "\"}";

            var responseString = sendRequestFirebase(json);

            BasicNotification notification = new BasicNotification()
            {
                Id = Guid.NewGuid().ToString(),
                Title = titleSend,
                Messenge = messenge,
                UserSend = User.Identity.Name,
                CreateDate = DateTime.Now,
                MessengeResult = responseString,
                UserAccept = userlogin,
                TocpicCode = "staff",
                ImageAttach = updateAttach(files)
            };

            db.BasicNotifications.Add(notification);
            db.SaveChanges();
            return RedirectToAction("send", "notification", new { msg = "Đã gửi" });
        }
        */

        public ActionResult SendSMS(int? page, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (String.IsNullOrEmpty(search))
                search = "";

            ViewBag.SearchText = search;

            return View(db.SendSmsHistories.Where(p => p.Phone.Contains(search)).OrderByDescending(p => p.CreateTime).ToPagedList(pageNumber, pageSize));
        }


        [HttpPost]
        public ActionResult SendSMS(string phone, string messenge)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");

            var account = db.SmsAccounts.Find(1);
            string Msg = string.Empty;
            if (account != null)
            {

                SMScore _smsCore = new SMScore(account.BrandName, account.UserName, account.Pass);
                _smsCore.IPserver = account.AddressSend;
                _smsCore.Port = Convert.ToInt32(account.PortSend);
                _smsCore.SendMethod = account.Method;

                var listPhone = phone.Split(';');


                if (listPhone.Count() == 1)
                {
                    var result = _smsCore.SendSMS(messenge, listPhone[0], ref Msg);
                    if (result)
                    {
                        var history = new SendSmsHistory()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Phone = listPhone[0],
                            Messenge = messenge,
                            UserSend = User.Identity.Name,
                            CreateTime = DateTime.Now,
                            StatusSend = "Đã gửi thành công"
                        };
                        db.SendSmsHistories.Add(history);
                        db.SaveChanges();
                    }
                    else
                    {
                        var history = new SendSmsHistory()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Phone = listPhone[0],
                            Messenge = messenge,
                            UserSend = User.Identity.Name,
                            CreateTime = DateTime.Now,
                            StatusSend = Msg
                        };
                        db.SendSmsHistories.Add(history);
                        db.SaveChanges();
                    }

                }
                else if (listPhone.Count() > 1)
                {
                    List<SMSUtl.SendMessageResult> SMSMessageResult = null;
                    List<SMSUtl.Message> SMSMessages = new List<SMSUtl.Message>();
                    foreach (var item in listPhone)
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            SMSUtl.Message msg = new SMSUtl.Message();
                            msg.Phone = item;
                            msg.Content = messenge;
                            SMSMessages.Add(msg);
                        }
                    }

                    SMSMessageResult = _smsCore.SendMultiSMS(SMSMessages, ref Msg);

                    for (var i = 0; i < SMSMessageResult.Count(); i++)
                    {
                        if (SMSMessageResult[i].Status == 1)
                        {
                            var history = new SendSmsHistory()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Phone = listPhone[i],
                                Messenge = messenge,
                                UserSend = User.Identity.Name,
                                CreateTime = DateTime.Now,
                                StatusSend = "Đã gửi thành công"
                            };
                            db.SendSmsHistories.Add(history);
                        }
                        else
                        {
                            var history = new SendSmsHistory()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Phone = listPhone[i],
                                Messenge = messenge,
                                UserSend = User.Identity.Name,
                                CreateTime = DateTime.Now,
                                StatusSend = SMSMessageResult[i].Message
                            };
                            db.SendSmsHistories.Add(history);
                        }
                    }
                    db.SaveChanges();
                }


            }

            return RedirectToAction("sendsms", "notification");
        }
    }
}