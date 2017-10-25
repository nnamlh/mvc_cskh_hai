﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{

    public class OrderConfirmRequest : RequestInfo
    {
        public string agency { get; set; }
        public List<OrderProductInfo> product { get; set; }

    }

    public class OrderInfoRequest : RequestInfo
    {
        public string code { get; set; }

        public string shipType { get; set; }

        public string payType { get; set; }

        public string phone { get; set; }

        public string address { get; set; }

        public List<OrderProductInfo> product { get; set; }

        public string timeSuggest { get; set; }

        public string notes { get; set; }

        public string orderType { get; set; }


        public int inCheckIn { get; set; }
        // 1 : dat hang trong checkin
        // 0: dat hang ngoai
    }

    public class OrderProductInfo
    {
        public string code { get; set; }

        public int quantity { get; set; }

        public string c1 { get; set; }
    }


    public class OrderConfirm : ResultInfo
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