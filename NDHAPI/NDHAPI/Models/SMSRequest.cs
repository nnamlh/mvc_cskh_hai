
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHAPI.Models
{
    public class SMSRequest
    {

        public string content { get; set; }

        public string phone { get; set; }
    }


    public class SMSContent
    {
        public int status { get; set; }

        public string message { get; set; }

        public bool isAgency { get; set; }

        public string code { get; set; }

        public List<string> products { get; set; }

        public string phone { get; set; }
    }

    public class SMSResult
    {
        public int status { get; set; }

        public string message { get; set; }
    }

}