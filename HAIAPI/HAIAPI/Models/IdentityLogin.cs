using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityLogin
    {
    }

    public class LoginResult : ResultInfo
    {

        public string token { get; set; }

        public string user { get; set; }

        public string Role { get; set; }

        public string store { get; set; }

        public string type { get; set; }

    }

    public class CheckUserLoginResult : ResultInfo
    {
        //   public string id { get; set; }
        // Id
        // 0: fail and msg
        // 1: staff
        // 2: agency
        // 3: go to login
        // public string msg { get; set; }

        public string name { get; set; }

        public string store { get; set; }

        public string code { get; set; }

        public string phone { get; set; }

        public string token { get; set; }

        public string role { get; set; }

        public string user { get; set; }
    }
}