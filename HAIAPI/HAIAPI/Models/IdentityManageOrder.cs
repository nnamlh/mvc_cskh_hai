using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class StaffOrderRequest : RequestInfo
    {
        //
        public string c2Code { get; set; }

        public string status { get; set; }

        public int? page { get; set; }
    }

    public class YourOrderResult : ResultInfo
    {
        public List<YourOrder> orders { get; set; }
    }

}