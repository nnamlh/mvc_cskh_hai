using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class MongoHistoryAPI
    {
        public ObjectId Id { get; set; }
        public string APIUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
        public int Sucess { get; set; }
        public string Error { get; set; }
        public string ReturnInfo { get; set; }
    }

    public class MongoAPIAuthHistory
    {
        public ObjectId Id { get; set; }
        public string UserLogin { get; set; }
        public string UserRole { get; set; }
        public Nullable<System.DateTime> LoginTime { get; set; }
        public Nullable<int> IsExpired { get; set; }
        public string Token { get; set; }

        public string Device { get; set; }

        public string Imei { get; set; }

        public string OS { get; set; }

    }

    public class MongoLogoutHistory
    {
        public ObjectId Id { get; set; }
        public string UserLogin { get; set; }
        public Nullable<System.DateTime> LogoutTime { get; set; }
        public string Token { get; set; }
    }

    public class MongoNotificationHistory
    {

        public ObjectId Id { get; set; }

        public string GuiId { get; set; }

        public string Content { get; set; }
        public string Title { get; set; }
        public string Messenge { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public int Success { get; set; }

        public string MessengeResult { get; set; }

        public string NType { get; set; }

        public List<string> NCode { get; set; }

        public List<string> UserRead { get; set; }

    }

}