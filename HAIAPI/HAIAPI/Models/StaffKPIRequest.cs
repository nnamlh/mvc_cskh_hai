using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class StaffKPIRequest : RequestInfo
    {
        public string type { get; set; }
    }

    public class StaffKPIDetailRequest : RequestInfo
    {
        public string kpiId { get; set; }
    }

    public class StaffKPIResult : ResultInfo
    {
        public List<StaffKPIInfo> data { get; set;  }
    }

    public class StaffKPIInfo
    {
        public string title { get; set; }

        public string id { get; set; }

        public string createTime { get; set; }
    }

    public class StaffKPIDetail
    {
        public string title { get; set; }
        public string plan { get; set; }

        public string perform { get; set; }

        public string point { get; set; }

        public string percent { get; set; }
    }

    public class StaffKPIDetailResult : ResultInfo
    {

        public List<StaffKPIDetail> data { get; set; }
    }
}