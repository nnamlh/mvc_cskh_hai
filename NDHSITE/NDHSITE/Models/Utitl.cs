using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NDHSITE.Models
{
    public class Utitl
    {

        public static bool CheckRoleAdmin(NDHDBEntities db, string userName)
        {
            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return false;

            var role = user.AspNetRoles.First();
            if (role == null)
                return false;

            if (role.Name == "Administrator")
                return true;

            return false;
        }

        public static int CheckRoleShowInfo(NDHDBEntities db, string userName)
        {
            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return 0;

            var role = user.AspNetRoles.First();
            if (role == null)
                return 0;

            return Convert.ToInt32(role.ShowInfoRole);
        }

        public static bool CheckUser(NDHDBEntities db, string userName, string func, int isAll)
        {

            var user = db.AspNetUsers.Where(p => p.UserName == userName).FirstOrDefault();

            if (user == null)
                return false;

            var role = user.AspNetRoles.First();
            if (role == null)
                return false;

            if (role.Name == "Administrator")
                return true;

            var funcCheck = db.FuncRoles.Where(p => p.FuncInfo.Code == func && p.RoleId == role.Id).FirstOrDefault();

            // khong co quyen
            if (funcCheck == null)
                return false;

            // dc full quyen
            if (funcCheck.IsAll == 1)
                return true;

            // chi cho phep xem: isall = 0
            if (isAll == funcCheck.IsAll)
                return true;


            return false;
        }
    }
}