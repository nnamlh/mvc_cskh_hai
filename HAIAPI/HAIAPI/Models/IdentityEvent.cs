using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
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



}