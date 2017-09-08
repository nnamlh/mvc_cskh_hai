using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHSITE.Models
{
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

}