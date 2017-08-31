using HAIAPI.Models;
using HAIAPI.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HAIAPI.Controllers
{
    public class RestMainController : ApiController
    {
        protected NDHDBEntities db;
        protected MongoHelper mongoHelper;
        public RestMainController()
        {
            db = new NDHDBEntities();
            mongoHelper = new MongoHelper();

        }

        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        /*
        protected bool checkLoginSession(string user, string token)
        {
            var check = db.APIAuthHistories.Where(p => p.UserLogin == user && p.Token == token && p.IsExpired == 0).FirstOrDefault();

            return check != null ? true : false;
        }
        */
    }
}
