using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHSITE.Models
{
    public class IdentityCommon
    {
    }

    public class ImportC2Result
    {
        public string old { get; set; }

        public string newCode { get; set; }

        public string staff { get; set; }

        public string phone { get; set; }

        public string name { get; set; }

        public string deputy { get; set; }

        public string success { get; set; }
    }

    public class CommonInfo
    {
        public string code { get; set; }
        public string name { get; set; }
    }

}