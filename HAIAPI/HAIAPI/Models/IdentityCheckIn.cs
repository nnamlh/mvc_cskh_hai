﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityCheckIn
    {

    }

    public class CheckCalendarResult : ResultInfo
    {
        public List<CalendarType> status { get; set; }

        public List<string> month { get; set; }
    }

    public class CalendarCreateRequest : RequestInfo
    {

        public int month { get; set; }

        public int year { get; set; }

        public List<CalendarCreateInfo> items { get; set; }

    }
    public class CalendarCreateInfo
    {
        public int day { get; set; }
        public List<string> agencies { get; set; }

        public string status { get; set; }

        public string notes { get; set; }
    }


    public class CalendarShowResult : ResultInfo
    {
        public int? hasApprove { get; set; }

        public string notes { get; set; }

        public int? month { get; set; }
        public int? year { get; set; }

        public List<CalendarShowTypeDetail> typeDetail { get; set; }

        public List<CalendarShowItem> items { get; set; } 
    }

    public class CalendarShowTypeDetail
    {
        public string typeId { get; set; }

        public string typeName { get; set; }

        public int number { get; set; }
    }

    public class CalendarShowItem
    {
        public int day { get; set; }

        public string type { get; set; }

        public string typeName { get; set; }

        public string notes { get; set; }

        public List<CalendarShowAgencyItem> agences { get; set; }
    }

    public class CalendarShowAgencyItem
    {
        public string deputy { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public string ctype { get; set; }

        public int? inPlan { get; set; }

        public int? perform { get; set; }
    }

    public class CalendarShowRequest : RequestInfo
    {
        public int month { get; set; }

        public int year { get; set; }


    }

}