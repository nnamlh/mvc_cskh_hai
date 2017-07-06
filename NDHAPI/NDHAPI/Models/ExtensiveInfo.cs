using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHAPI.Models
{
    public class ExtensiveInfo
    {

    }

    public class WavehouseInfo
    {
        public string wCode { get; set; }
        public string wType { get; set; }
        // BRANCH : chi nhanh
        // WAREHOURSE : kho
        // CI : dai ly1
        // CII : dai ly 2
        public string wName { get; set; }
    }
}