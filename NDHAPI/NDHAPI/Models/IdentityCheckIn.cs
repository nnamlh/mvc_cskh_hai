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

        public string user { get; set; }

        public string token { get; set; }

        public string c1Id { get; set; }

        public string rank { get; set; }

        public int group { get; set; }

        public string taxCode { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set; }

    }


    public class CheckInCalendarShowRequest : RequestInfo
    {
        public int day { get; set; }

        public int month { get; set; }

        public int year { get; set; }


    }
    public class CheckInCalendarShow : ResultInfo
    {
        public int? hasApprove { get; set; }

        public string notes { get; set; }

        public int? month { get; set; }
        public int? year { get; set; }

        public List<CheckInCalendarItemShow> items { get; set; }
    }


    public class CheckInCalendarItemShow
    {

        //
        public int day { get; set; }

        public int month { get; set; }

        public int year { get; set; }

        public string status { get; set; }

        public string statusName { get; set; }

        public string notes { get; set; }

        public List<CheckInAgencyCalendar> calendar { get; set; }


    }

    public class CheckInAgencyCalendar
    {
        public string deputy { get; set; }
        public string code { get; set; }

        public string name { get; set; }

        public string ctype { get; set; }

        public int? inPlan { get; set; }

        public int? perform { get; set; }
    }

    public class CheckInStatus
    {
        public string name { get; set; }

        public string code { get; set; }

    }

    public class CalendarCheckCreate : ResultInfo
    {
        public List<CheckInStatus> status { get; set; }
        public List<string> month { get; set; }
    }


}