using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityCommon
    {
    }


    public class HaiUtil
    {
        public static string HostName = "http://cskh.nongduochai.vn/";
    }

    public class ResultInfo
    {
        public string id { get; set; }
        public string msg { get; set; }
    }

    public class RequestInfo
    {
        public string user { get; set; }
        public string token { get; set; }

    }


    public class MainInfoResult : ResultInfo
    {

        public List<string> topics { get; set; }

        public List<string> function { get; set; }

        public List<AgencyInfoC2> c2 { get; set; }

        public List<AgencyInfo> c1 { get; set; }

    }

    public class AgencyInfo
    {
        public string name { get; set; }

        public string deputy { get; set; }

        public string code { get; set; }

        public string type { get; set; }

        public string id { get; set; }

        public double? lat { get; set; }

        public double? lng { get; set; }

        public string address { get; set; }

        public string phone { get; set; }

        public string taxCode { get; set; }

        public string province { get; set; }

        public string district { get; set; }

        public string identityCard { get; set; }

        public string businessLicense { get; set; }

        public string ward { get; set; }

        public string country { get; set; }

        public string haibranch { get; set; }

    }

    public class AgencyInfoC2 : AgencyInfo
    {
        public string c1Id { get; set; }

        public string rank { get; set; }

        public int? group { get; set; }
    }


    public class ResultAgency : ResultInfo
    {
        public List<AgencyInfo> agences { get; set; }
    }

    public class MainInfoRequest : RequestInfo
    {

        public string regId { get; set; }

        public int isUpdate { get; set; }
        // 1: yes
        // 0: no
    }

}