using NDHSITE.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NDHSITE.Models
{
    public class Utitl
    {

        public static bool CheckRoleAdmin(NDHDBEntities db, string userName)
        {
            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return false;

            var role = user.AspNetRoles.First();
            if (role == null)
                return false;

            if (role.Name == "Administrator")
                return true;

            return false;
        }

        public static int CheckRoleShowInfo(NDHDBEntities db, string userName)
        {
            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return 0;

            var role = user.AspNetRoles.First();
            if (role == null)
                return 0;

            return Convert.ToInt32(role.ShowInfoRole);
        }

        public static void Send(string title, string messenge, string user, NDHDBEntities db, MongoHelper mongoHelp)
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

                var responseString = Utitl.sendRequestFirebase(json);

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
                mongoHelp.saveNotificationHistory(ref notification);
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

        public static List<string> GetBranchesPermiss(NDHDBEntities db, string userName, bool isAll)
        {

            if (!isAll)
            {
                // lay ds chi nhanh co the xem
                var branchPermiss = db.UserBranchPermisses.Where(p => p.UserName == userName).Select(p => p.BranchCode).ToList();

                if (branchPermiss.Count() == 0)
                {
                    List<string> branches = new List<string>();
                    var checkStaff = db.HaiStaffs.Where(p => p.UserLogin == userName).FirstOrDefault();

                    if (checkStaff != null)
                    {
                        branches.Add(checkStaff.HaiBranch.Code);
                    }

                    return branches;
                }

                return branchPermiss;
            }

            // lay toan bo
            var listBranch = db.HaiBranches.Select(p => p.Code).ToList();

            return listBranch;

        }

        public static bool CheckUser(NDHDBEntities db, string userName, string func, int isAll)
        {

            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return false;

            var role = user.AspNetRoles.First();
            if (role == null)
                return false;

            if (role.Name == "Administrator")
                return true;

            var funcCheck = db.FuncRoles.Where(p => p.FuncInfo.Code == func && p.RoleId == role.Id).FirstOrDefault();

            // khong co quyen
            if (funcCheck == null)
                return false;

            // dc full quyen
            if (funcCheck.IsAll == 1)
                return true;

            // chi cho phep xem: isall = 0
            if (isAll == funcCheck.IsAll)
                return true;


            return false;
        }
    }
}