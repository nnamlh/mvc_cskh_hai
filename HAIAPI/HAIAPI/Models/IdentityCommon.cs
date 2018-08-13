using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityCommon
    {
        public string code { get; set; }
        public string name { get; set; }
    }


    public class ResultInfo
    {
        public string id { get; set; }
        public string msg { get; set; }
    }


    public class ResultWithData : ResultInfo
    {
        public Object data { get; set; }
    }


    public class MainInfoResult : ResultInfo
    {

        public string name { get; set; }

        public string code { get; set; }

        public string type { get; set; }

        public List<string> topics { get; set; }

        public List<string> function { get; set; }
    }


    public class BranchInfo
    {
        public string code { get; set; }
        public string phone { get; set; }

        public string name { get; set; }

        public double? lat { get; set; }
        public double? lng { get; set; }

        public string address { get; set; }
    }
    public class MainAgencyInfoResult : ResultInfo
    {

        public string name { get; set; }

        public string code { get; set; }

        public string type { get; set; }

        public List<string> topics { get; set; }

        public List<string> function { get; set; }

        public List<GroupInfo> productGroups { get; set; }

        public List<ProductInfoResult> products { get; set; }

        public List<AgencyInfo> c2 { get; set; }

    }

    public class RequestInfo
    {
        public string token { get; set; }
        public string user { get; set; }
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


    public class ResultCommonType : ResultInfo
    {
        public List<IdentityCommon> data { get; set; }
    }

}