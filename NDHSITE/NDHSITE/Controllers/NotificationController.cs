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

namespace NDHSITE.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        //
        // GET: /Notification/

        public ActionResult Send(string msg = null)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.MSG = msg;

          //  ViewBag.CTopic = db.NotificationTopics.Where(p => p.TopicType == "CUS").ToList();
           // ViewBag.STopic = db.NotificationTopics.Where(p => p.TopicType == "STAFF").ToList();

            ViewBag.Areas = db.HaiAreas.ToList();
            ViewBag.Branches = db.HaiBranches.ToList();

            return View();
        }

        
        [HttpPost]
        public ActionResult SendArea(HttpPostedFileBase files, string title, string messenge, string type, string area)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");

            //string json = "{\"notification\": {\"title\": \"Portugal vs. Denmark\",\"body\": \"5 to 1\"},\"to\": \"ftWtDYSoDhE:APA91bFXZfod7cSDMTuUWuRn6aC63WL_-WBIgshPvC2E0jTXEq7E4BNcid14CC2Kx2a1Ih2T4xR0RtY4j71pcd089eM_PH32va3As4JJzuNreZcxoy5-pkeRLyZ2b6jFAWwoTzbJcx2Z\"}";

            //string json = "{\"data\": {\"title\": \"'Portugal vs. Denmark'\", \"message\": \"'This is a Firebase Cloud Messaging Topic Message!'\"},\"to\": \"/topics/global\"}";

            var topic = type + area;

            var msgSend = messenge;

            if (messenge.Length > 100)
            {
                msgSend = messenge.Substring(0, 100) + "...";
            }

            var titleSend = title.ToUpper();

            string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + titleSend + "\",\"body\": \"" + msgSend + "\"},\"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"/topics/" + topic + "\"}";

            var responseString = sendRequestFirebase(json);

            BasicNotification notification = new BasicNotification()
            {
                Id = Guid.NewGuid().ToString(),
                Title = titleSend,
                Messenge = messenge,
                UserSend = User.Identity.Name,
                CreateDate = DateTime.Now,
                MessengeResult = responseString,
                TocpicCode = topic,
                ImageAttach = updateAttach(files)
            };

            db.BasicNotifications.Add(notification);
            db.SaveChanges();
 
            return RedirectToAction("send", "notification", new { msg = "Đã gửi" });
        }
        

        private string updateAttach(HttpPostedFileBase files)
        {
            string urlThumbnail = "";
            if (files != null)
            {
                try
                {
                    string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                    string fsave = "~/haiupload/notification/" + dfolder;

                    bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                    MemoryStream target = new MemoryStream();
                    files.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Height = 160,
                        isSacle = false,
                        UploadPath = fsave
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, ".png");

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/haiupload/notification/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {
                }
            }

            return urlThumbnail;
        }


        [HttpPost]
        public ActionResult SendStaffArea(HttpPostedFileBase files, string title, string messenge, string type, string area, string branch)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageNotification", 1))
                return RedirectToAction("relogin", "home");


            string topic = "staff";
            if (type == "2")
            {
                // theo khu vuc
                topic += area;
            }
            else if (type == "3")
            {
                // theo chi nhanh
                topic += branch;
            }

            var msgSend = messenge;

            if (messenge.Length > 100)
            {
                msgSend = messenge.Substring(0, 100) + "...";
            }

            var titleSend = title.ToUpper();


            //string json = "{\"notification\": {\"title\": \"Portugal vs. Denmark\",\"body\": \"5 to 1\"},\"to\": \"ftWtDYSoDhE:APA91bFXZfod7cSDMTuUWuRn6aC63WL_-WBIgshPvC2E0jTXEq7E4BNcid14CC2Kx2a1Ih2T4xR0RtY4j71pcd089eM_PH32va3As4JJzuNreZcxoy5-pkeRLyZ2b6jFAWwoTzbJcx2Z\"}";

            //string json = "{\"data\": {\"title\": \"'Portugal vs. Denmark'\", \"message\": \"'This is a Firebase Cloud Messaging Topic Message!'\"},\"to\": \"/topics/global\"}";

            string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + titleSend + "\",\"body\": \"" + msgSend + "\"}, \"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"/topics/" + topic + "\"}";

            var responseString = sendRequestFirebase(json);

            BasicNotification notification = new BasicNotification()
            {
                Id = Guid.NewGuid().ToString(),
                Title = titleSend,
                Messenge = messenge,
                UserSend = User.Identity.Name,
                CreateDate = DateTime.Now,
                MessengeResult = responseString,
                TocpicCode = topic,
                ImageAttach = updateAttach(files)
            };

            db.BasicNotifications.Add(notification);
            db.SaveChanges();

            return RedirectToAction("send", "notification", new { msg = "Đã gửi" });
        }


        private string sendRequestFirebase(string json)
        {
            string url = @"https://fcm.googleapis.com/fcm/send";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.Headers["Authorization"] = "key=AAAAEo0n4Rw:APA91bHJi7Sz884_5yuDyUw2Sf2mPyXG1GK06q4jWmhnLl7gLeJUCPn_SD9q0R0gfUWUWbyujsYn_Wb6LLjtY33LWO1BYVryjeo2RPflMko7NqJaUecn8a1WB8Ng_9OvNDxHoE1wJ7wH6BRWr5ayinritsNlM2ntjQ";

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