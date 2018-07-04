using System;
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


        public int max { get; set; }


        public bool requireCheck { get; set; }

    }

    public class CheckCalendarUpdateResult : ResultInfo
    {
        public List<CalendarType> status { get; set; }

    }


    public class CalendarUpdateRequest : RequestInfo
    {
        public int month { get; set; }

        public int year { get; set; }

        public CalendarCreateInfo item { get; set; }


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

        public string ctypename { get; set; }

        public int? inPlan { get; set; }

        public int? perform { get; set; }

        public string content { get; set; }

        public string address { get; set; }


    }

    public class AgencyCheckIn
    {
        public string deputy { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public int? inPlan { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set; }

        public string ctype { get; set; }

        public string cname { get; set; }

        public List<SubOwner> c1 { get; set; }

        public string agencyType { get; set; }

        public string content { get; set; }

        public string checkInId { get; set; }

    }

    public class CalendarShowRequest : RequestInfo
    {
        public int month { get; set; }

        public int year { get; set; }
    }

    //
    public class CheckInGetPlanResult : ResultInfo
    {
        public List<AgencyCheckIn> checkin { get; set; }
        //   public List<string> outplan { get; set; }

        public List<CalendarType> status { get; set; }

        public bool checkFlexible { get; set; }

    }
    public class CheckInGetPlanRequest : RequestInfo
    {
        public int month { get; set; }

        public int year { get; set; }

        public int day { get; set; }

    }

    ///
    public class CheckInTaskRequest : RequestInfo
    {
        public string code { get; set; }

        public string checkInId { get; set; }
    }

    public class CheckInOutPlanRequest : RequestInfo
    {
        public string code { get; set; }

        public string ctype { get; set; }

        public double? lat { get; set; }

        public double? lng { get; set; }
    }

    public class CheckInFlexibleRequest : RequestInfo
    {
        public double? lat { get; set; }

        public double? lng { get; set; }

        public string country { get; set; }

        public string province { get; set; }

        public string district { get; set; }

        public string ward { get; set; }

        public string content { get; set; }

        public string address { get; set; }
    }

    public class CheckInTaskResult : ResultInfo
    {
        public int timeRemain { get; set; }

        public List<TaskInfo> tasks { get; set; }

        public string agencyCode { get; set; }

        public string agencyName { get; set; }

        public string agencyDeputy { get; set; }

        public int inPlan { get; set; }
    }

    public class TaskInfo
    {
        public string code { get; set; }
        public string name { get; set; }
        public int time { get; set; }
    }

    public class CheckInRequest : RequestInfo
    {
        public string agency { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }

        public double? distance { get; set; }

        public string notes { get; set; }

        public string noteCode { get; set; }

    }


}