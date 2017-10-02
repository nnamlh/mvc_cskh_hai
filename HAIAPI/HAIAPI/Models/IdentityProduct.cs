using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityProduct
    {
    }

    public class WavehouseInfo
    {
        public string wCode { get; set; }
        public string wType { get; set; }
        // BRANCH : chi nhanh
        // WAREHOURSE : kho
        // CI : dai ly1
        // CII : dai ly 2
        public string wName { get; set; }
    }

    public class RequestProduct
    {
        public string user { get; set; }
        public string token { get; set; }

        public string receiver { get; set; }

        public List<string> products { get; set; }

        public string status { get; set; }
    }

    public class RequestProductHelp
    {
        public string user { get; set; }
        public string token { get; set; }

        public List<string> products { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string agency { get; set; }

        public int nearAgency { get; set; }
    }

    public class RequestCheckLocationDistance
    {
        public string user { get; set; }
        public string token { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string agency { get; set; }
    }

    public class ResultProduct
    {

        public string id { get; set; }
        public string msg { get; set; }

        public List<GeneralInfo> products { get; set; }

    }

    public class GeneralInfo
    {
        public string name { get; set; }
        public string code { get; set; }
        public string status { get; set; }

        public int success { get; set; }
    }
    // kiem tra tracking
    public class RequestTracking
    {
        public string user { get; set; }
        public string token { get; set; }

        public string code { get; set; }
    }

    public class ResultTracking : ResultInfo
    {
        public string code { get; set; }

        public string name { get; set; }

        public List<PTrackingInfo> tracking { get; set; }
    }
    public class PTrackingInfo
    {
        public string name { get; set; }

        public string status { get; set; }

        public string importTime { get; set; }
        public string exportTime { get; set; }
    }

}