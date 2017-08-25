using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHAPI.Models
{
    public class IdentityRequest
    {
    }

    public class HaiUtil
    {
        public static string HostName = "http://cskh.nongduochai.vn/";
    }

    public class ResultInfo
    {
        public string id { get; set; }
        public string msg { get; set; }
    }

    public class LoginInfo
    {
        public string id { get; set; }

        public string msg { get; set; }

        public string token { get; set; }

        public string user { get; set; }

        public string[] function { get; set; }

        public string Role { get; set; }

        public string store { get; set; }

        public string type { get; set; }

    }

    public class CheckUserLoginReponse
    {
        public string id { get; set; }
        // 0: fail and msg
        // 1: staff
        // 2: agency
        // 3: go to login
        public string msg { get; set; }

        public string name { get; set; }

        public string store { get; set; }

        public string code { get; set; }

        public string phone { get; set; }

        public string token { get; set; }

        public string role { get; set; }

        public string user { get; set; }
    }


    public class CheckinResponse
    {
        public string comment { get; set; }
        public string image { get; set; }

        public string user { get; set; }

        public string token { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string date { get; set; }

        public string agency { get; set; }
    }

    public class ResultCheckStaff
    {
        public string id { get; set; }
        public string msg { get; set; }

        public string avatar { get; set; }

        public string signature { get; set; }

        public string status { get; set; }
    }


    public class RequestProduct
    {
        public string user { get; set; }
        public string token { get; set; }

        public string receiver { get; set; }

        public List<string> products { get; set; }

        public string status { get; set; }
    }

    public class RequestProductHelp
    {
        public string user { get; set; }
        public string token { get; set; }

        public List<string> products { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string agency { get; set; }

        public int nearAgency { get; set; }
    }

    public class RequestCheckLocationDistance
    {
        public string user { get; set; }
        public string token { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string agency { get; set; }
    }

    public class ResultProduct
    {

        public string id { get; set; }
        public string msg { get; set; }

        public List<GeneralInfo> products { get; set; }

    }

    public class GeneralInfo
    {
        public string name { get; set; }
        public string code { get; set; }
        public string status { get; set; }

        public int success { get; set; }
    }

    // kiem tra tracking
    public class RequestTracking
    {
        public string user { get; set; }
        public string token { get; set; }

        public string code { get; set; }
    }

    public class ResultTracking : ResultInfo
    {
        public string code { get; set; }

        public string name { get; set; }

        public List<PTrackingInfo> tracking { get; set; }
    }
    public class PTrackingInfo
    {
        public string name { get; set; }

        public string status { get; set; }

        public string importTime { get; set; }
        public string exportTime { get; set; }
    }

    public class UpdateRegFirebase
    {
        public string user { get; set; }
        public string token { get; set; }

        public string regId { get; set; }

        public int isUpdate { get; set; }
        // 1: yes
        // 0: no
    }

    // logout
    public class LogoutInfo
    {
        public string user { get; set; }
        public string token { get; set; }
    }

    public class ResultFirebaseInfo
    {
        public string id { get; set; }
        public string msg { get; set; }

        public int ecount { get; set; }

        public List<string> topics { get; set; }

        public List<string> function { get; set; }
    }

    public class ResultMainInfo : ResultInfo
    {
        public int ecount { get; set; }

        public List<string> topics { get; set; }

        public List<string> function { get; set; }

        public List<AgencyInfoC2Result> agencies { get; set; }

        public List<ReceiverInfo> recivers { get; set; }

        public List<ProductCodeInfo> products { get; set; }

        public List<AgencyInfo> agencyc1 { get; set; }

    }

    public class ProductCodeInfo {
        public string code {get; set;}
        public string name {get; set;}
    }

    public class ResultFunctionProduct
    {
        public string id { get; set; }
        public string msg { get; set; }


        public List<string> function { get; set; }

    }

    public class RequestMSGToHai
    {
        public string user { get; set; }
        public string token { get; set; }

        public string type { get; set; }

        public string content { get; set; }
    }


    // phana event
    public class Event
    {
        public string eid { get; set; }
        public string ename { get; set; }

        public string etime { get; set; }

        public string eimage { get; set; }

    }

    public class EventDetail : Event
    {
        public string id { get; set; }
        public string msg { get; set; }

        public string edescribe { get; set; }

        public List<string> areas { get; set; }

        public List<ResultEventAward> awards { get; set; }

        public List<ResultEventProduct> products { get; set; }
    }

    public class ResultEvent : ResultInfo
    {
        public List<Event> events { get; set; }
    }


    public class EventDetailRequest : RequestInfo
    {
        public string eventId { get; set; }
    }

    public class RequestInfo
    {
        public string user { get; set; }
        public string token { get; set; }

    }

    public class ResultEventProduct
    {
        public string name { get; set; }
        public string point { get; set; }

    }

    public class ResultEventAward
    {
        public string name { get; set; }
        public string point { get; set; }

        public string image { get; set; }

    }

    public class RequestCodeEvent : RequestInfo
    {
        public string eventId { get; set; }
        public List<string> codes { get; set; }
    }

    public class ResultCodeEvent : ResultInfo
    {
        public List<GeneralInfo> codes { get; set; }
    }



    public class ResultUserInfo : ResultInfo
    {
        public string type { get; set; }

        public string fullname { get; set; }

        public string address { get; set; }

        public string phone { get; set; }

        public string birthday { get; set; }

        public string user { get; set; }

        public string area { get; set; }
    }

    public class RequestAgency : RequestInfo
    {
        public string type { get; set; }

        public string search { get; set; }
    }


    public class AgencyInfo
    {
        public string name { get; set; }
        public string deputy { get; set; }
        public string code { get; set; }

        public string type { get; set; }

        public string id { get; set; }

    }

    public class AgencyInfoC2Result : AgencyInfo
    {
       
        public double? lat { get; set; }

        public double? lng { get; set; }

        public string address { get; set; }

        public string phone { get; set; }

        public string c1Id { get; set; }

        public string rank { get; set; }

        public int? group { get; set; }

        public string taxCode { get; set; }
        public string province { get; set; }
        public string district { get; set; }
        public string identityCard { get; set; }
        public string businessLicense { get; set; }

        public string ward { get; set; }
        public string country { get; set; }

    }


    public class ResultAgency : ResultInfo
    {
        public List<AgencyInfo> agences { get; set; }
    }

    public class ResultAgencyC2 : ResultInfo
    {
        public List<AgencyInfoC2Result> agences { get; set; }
    }

    /// <summary>
    /// receiver
    /// </summary>

    public class ReceiverRequest : RequestInfo
    {
        public string search { get; set; }
    }

    public class ReceiverInfo
    {
        public string name { get; set; }
        public string code { get; set; }

        public string type { get; set; }
    }


    public class ReceiverResult : ResultInfo
    {
        public List<ReceiverInfo> receivers { get; set; }
    }
    /////////
    public class NotificationRequest : RequestInfo
    {
        public int pageno { get; set; }
    }

    public class NotificationInfo
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string time { get; set; }

        public string image { get; set; }
    }

    public class NotificationResult : ResultInfo
    {
        public int pageno { get; set; }
        public int pagemax { get; set; }
        public List<NotificationInfo> notifications { get; set; }
    }
}