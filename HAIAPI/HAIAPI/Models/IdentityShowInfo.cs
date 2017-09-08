using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityShowInfo
    {
    }

    public class GroupInfo
    {
        public string id { get; set; }
        public string name { get; set; }

        public List<GroupInfo> childs { get; set; }
    }

    public class ProductInfoResult
    {
        public string id { get; set; }
        public string code { get; set; }
        public string barcode { get; set; }
        public string name { get; set; }
        public string producer { get; set; }
        public string commerceName { get; set; }
        public string activce { get; set; }
        public Nullable<int> isNew { get; set; }
        public Nullable<int> isForcus { get; set; }
        public string groupId { get; set; }
        public string groupName { get; set; }
        public string image { get; set; }
    }

    public class ProductDetailResult : ProductInfoResult
    {
        public string unit { get; set; }
        public string describe { get; set; }

        public string uses { get; set; }

        public string introduce { get; set; }

        public string register { get; set; }

        public string notes { get; set; }

        public string other { get; set; }

        public List<string> images { get; set; }
    }
}