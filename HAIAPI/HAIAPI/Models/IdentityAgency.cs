using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class AgencyCreateRequest : RequestInfo
    {
        public string name { get; set; }
        public string deputy { get; set; }
        public string phone { get; set; }
        public string identityCard { get; set; }
        public string businessLicense { get; set; }
        public string address { get; set; }
        public string province { get; set; }
        public string district { get; set; }

        public string country { get; set; }

        public string ward { get; set; }

        public string c1Id { get; set; }

        public string rank { get; set; }

        public int group { get; set; }

        public string taxCode { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set; }

        public string image { get; set; }

    }

    public class AgencyModifyRequest : AgencyCreateRequest
    {
        public string id { get; set; }

        public string code { get; set; }
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

    public class AgencyC2C1
    {
        public string code { get; set; }
        public string name { get; set; }
        public string store { get; set; }

        public int? priority { get; set; }
    }

    public class AgencyInfoC2 : AgencyInfo
    {
        public string c1Id { get; set; }

        public string rank { get; set; }

        public int? group { get; set; }

        public List<AgencyC2C1> c1 { get; set; }
    }


    public class ResultAgency : ResultInfo
    {
        public List<AgencyInfo> agences { get; set; }
    }


    public class AgencyUpdateLocationRequest : RequestInfo
    {
        public double? lat { get; set; }

        public double? lng { get; set; }

        public string address { get; set; }

        public string province { get; set; }

        public string district { get; set; }

        public string ward { get; set; }

        public string country { get; set; }

        public string id { get; set; }

        public string image { get; set; }
    }

}