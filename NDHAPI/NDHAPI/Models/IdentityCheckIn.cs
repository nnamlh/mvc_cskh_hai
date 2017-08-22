using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHAPI.Models
{
    public class IdentityCheckIn
    {

    }


    public class AgencyCreate
    {
        public string store { get; set; }
        public string deputy { get; set; }
        public string phone { get; set; }
        public string identityCard { get; set; }
        public string businessLicense { get; set; }
        public string address { get; set; }
        public string province { get; set; }
        public string district { get; set; }

        public string user { get; set; }

        public string token { get; set; }

        public string c1Id { get; set; }

        public string rank { get; set; }

        public int group { get; set; }

        public string taxCode { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set;  }

    }
    
}