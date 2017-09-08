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
        public static string HostName = "http://192.168.2.170:801/";
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

        public List<ProductInfoResult> products { get; set; }

    }

   
    public class MainInfoRequest : RequestInfo
    {

        public string regId { get; set; }

        public int isUpdate { get; set; }
        // 1: yes
        // 0: no
    }

    public class ProductCodeInfo
    {
        public string code { get; set; }
        public string name { get; set; }
    }


}