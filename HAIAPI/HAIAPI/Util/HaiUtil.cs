﻿using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace HAIAPI.Util
{
    public class HaiUtil
    {
        public static string HostName = "http://dms.nongduochai.vn/";
        public static void SendNotifi(string title, string messenge, string user, NDHDBEntities db, MongoHelper mongoHelp)
        {

            string sendTo = "";
            string NType = "ID";
            string NCode = user;
            string Id = Guid.NewGuid().ToString();

            var firebaseInfo = db.RegFirebases.Where(p => p.UserLogin == user).FirstOrDefault();

            if (firebaseInfo != null)
            {
                sendTo = firebaseInfo.RegId;


                title = title.ToUpper();
                string json = "{ \"notification\": {\"click_action\": \"OPEN_ACTIVITY_1\" ,\"title\": \"" + title + "\",\"body\": \"" + messenge + "\"},\"data\": {\"title\": \"'" + title + "'\",\"message\": \"'" + messenge + "'\"},\"to\": \"" + sendTo + "\"}";

                var responseString = HaiUtil.sendRequestFirebase(json);

                MongoNotificationHistory notification = new MongoNotificationHistory()
                {
                    GuiId = Id,
                    Title = title,
                    Messenge = messenge,
                    NType = NType,
                    CreateTime = DateTime.Now,
                    MessengeResult = responseString,
                    Content = messenge,
                    Success = 1,
                    NCode = new List<string>(),
                    UserRead = new List<string>()
                };

                notification.NCode.Add(NCode);
                mongoHelp.saveNotificationHistory(notification);
            }
        }



        public static string sendRequestFirebase(string json)
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


    }
}