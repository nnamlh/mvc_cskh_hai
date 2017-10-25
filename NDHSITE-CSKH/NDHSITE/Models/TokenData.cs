using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHSITE.Models
{
    public class TokenData
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Token { get; set; }
    }

    public class FuncShow
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }


    public class MenuGroup
    {
        public String Name;
        public List<MenuInfo> Menus { get; set; }
    }

    public class MenuInfo
    {
        public String Url { get; set; }

        public String Icon { get; set; }

        public String Name { get; set; }
        public int? Position { get; set; }

    }


    public class EventTokenAgency
    {
        public string Name { get; set; }

        public List<EventAgency> Agencies { get; set; }
    }

    public class EventAgency
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public string Token { get; set; }
    }

    public class UserInfoData
    {
        public string type { get; set; }

        public string fullname { get; set; }

        public string address { get; set; }

        public string phone { get; set; }

        public string birthday { get; set; }

        public string user { get; set; }

        public string area { get; set; }

        public string branch { get; set; }
        public string code { get; set; }
    }


    public class PTrackingInfo
    {
        public string name { get; set; }

        public string status { get; set; }

        public string importTime { get; set; }
        public string exportTime { get; set; }
    }

    public class AccountAgencyResult
    {
        public string code { get; set; }

        public string user { get; set; }

        public string status { get; set; }
    }
}