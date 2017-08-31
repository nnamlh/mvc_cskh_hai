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


}