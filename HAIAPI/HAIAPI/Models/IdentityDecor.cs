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
        public string checkInId { get; set; }

        public string group { get; set; }
    }
}