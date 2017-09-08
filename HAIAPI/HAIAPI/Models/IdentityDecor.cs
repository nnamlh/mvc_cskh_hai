using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityDecor
    {
    }

    public class DecorFolderResult
    {
        public string name { get; set; }

        public string code { get; set; }
    }

    public class DecorImageResult
    {
        public string url { get; set; }
        public string id { get; set; }
    }

    public class DecorImageRequest:RequestInfo
    {
        public int day { get; set; }

        public int month { get; set; }

        public int year { get; set; }

        public string agency { get; set; }

        public string group { get; set; }
    }
}