using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HAIAPI.Models;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class DecorController : RestMainController
    {

        [HttpGet]
        public List<DecorFolderResult> GetDecorFolder (string user, string token)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/decor/getdecorfolder",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new List<DecorFolderResult>();

            try
            {
            
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Wrong token and user login!");

                var data = db.DecorGroups.ToList();

                foreach(var item in data)
                {
                    result.Add(new DecorFolderResult()
                    {
                        code = item.Id,
                        name = item.Name
                    });
                }
            }
            catch (Exception e)
            {
                log.Error = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }

        [HttpPost]
        public List<DecorImageResult> GetDecorImages()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/decor/getdecorimages",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new List<DecorImageResult>();

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<DecorImageRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền");

                var data = staff.DecorImages.Where(p => p.AgencyCode == paser.agency && p.DecorGroup == paser.group && p.DaySend == paser.day && p.MonthSend == paser.month && p.YearSend == paser.year).ToList();

                foreach(var item in data)
                {
                    result.Add(new DecorImageResult()
                    {
                        id = item.Id,
                        url = HaiUtil.HostName + item.ImageUrl
                    });
                }


            } catch(Exception e)
            {
                log.Error = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }
    }
}
