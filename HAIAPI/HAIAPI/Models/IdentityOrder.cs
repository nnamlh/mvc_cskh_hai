using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{


    public class OrderProductInfo
    {
        public string productId { get; set; }

        
    }


    public class OrderInitialize : ResultInfo
    {
        public List<EventOrderInfo> events { get; set; }

        public List<IdentityCommon> payType { get; set; }

        public List<IdentityCommon> shipType { get; set; }

        public string store { get; set; }

        public string deputy { get; set; }

        public string agencyCode { get; set; }

        public string agencyId { get; set; }

        public string phone { get; set; }

        public string address { get; set; }
    }

   
    public class EventGift
    {
        public string name { get; set; }

        public string image { get; set; }

        public string point { get; set; }
    }

    public class EventOrderInfo
    {
        public string id { get; set; }

        public string name { get; set; }

        public string point { get; set; }

        public string time { get; set; }

        public string notes { get; set; }

        public List<EventGift> gifts { get; set; }
    }
}