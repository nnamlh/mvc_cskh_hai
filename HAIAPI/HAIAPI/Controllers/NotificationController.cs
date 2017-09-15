using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HAIAPI.Models;
using PagedList;
using System.Web.Script.Serialization;
using System.Globalization;

namespace HAIAPI.Controllers
{
    public class NotificationController : RestMainController
    {

        // lay danh sach notification
        [HttpGet]
        public NotificationInfoResult get(string user, string token, int? page)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/notification/get",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new NotificationInfoResult()
            {
                id = "1",
                msg = "success",
                data = new List<NotificationInfo>()
            };

         

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Wrong token and user login!");

                var topics = GetUserTopics(user);

                // last 3 month
                var lastMonth = DateTime.Now.Month - 3;
                var lastYear = DateTime.Now.Year;

                if (lastMonth <= 0)
                {
                    lastMonth = lastMonth + 12;
                    lastYear--;
                }


                var time = DateTime.ParseExact("01/" + lastMonth + "/" + lastYear, "dd/M/yyyy", null);
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                result.page = pageNumber;
                var data = mongoHelper.getListNotification(time.ToShortDateString()).ToPagedList(pageNumber, pageSize);

                List<NotificationInfo> notificstions = new List<NotificationInfo>();

                foreach(var item in data)
                {
                    if (item.NType == "ID")
                    {
                        // kiem tra user trong list
                        if(item.NCode.Contains(user))
                        {
                            var info = new NotificationInfo()
                            {
                                id = item.GuiId,
                                messenger = item.Messenge,
                                title = item.Title,
                                time = item.CreateTime.Value.ToShortDateString(),
                                content = HaiUtil.HostName + "/notification/show/" + item.GuiId
                            };

                            if (item.UserRead.Contains(user))
                                info.isRead = 1;
                            else
                                info.isRead = 0;

                            notificstions.Add(info);
                        }

                    } else
                    {
                        // kiem tra topic
                        foreach(var topic in topics)
                        {
                            if (item.NCode.Contains(topic))
                            {
                                var info = new NotificationInfo()
                                {
                                    id = item.GuiId,
                                    messenger = item.Messenge,
                                    title = item.Title,
                                    time = item.CreateTime.Value.ToShortDateString(),
                                    content = HaiUtil.HostName + "/notification/show/" + item.GuiId
                                };

                                if (item.UserRead.Contains(user))
                                    info.isRead = 1;
                                else
                                    info.isRead = 0;

                                notificstions.Add(info);
                            }
                        }
                    }

                    result.data = notificstions;
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
        public ResultInfo read(string user, string notification)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/notification/read",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };

            mongoHelper.updateNotificationRead(user, notification);

            log.Content = user + "|" + notification;

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }
    }
}
