using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityNotification
    {

    }

    public class NotificationInfoResult : ResultInfo
    {
        public List<NotificationInfo> data { get; set; }

        public int page { get; set; }
    }

    public class NotificationInfo
    {
        public string id { get; set; }

        public string time { get; set; }

        public string title { get; set; }

        public string messenger { get; set; }

        public int isRead { get; set; }

        public string content { get; set; }
    }
}