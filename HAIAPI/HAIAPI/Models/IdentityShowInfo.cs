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
        public string Id { get; set; }
        public string Code { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Producer { get; set; }
        public string CommerceName { get; set; }
        public string Activce { get; set; }
        public Nullable<int> New { get; set; }
        public Nullable<int> Forcus { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Image { get; set; }
    }
}