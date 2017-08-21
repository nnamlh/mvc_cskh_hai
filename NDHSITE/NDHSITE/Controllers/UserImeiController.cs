using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;

namespace NDHSITE.Controllers
{

    public class UserImeiController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        //
        // GET: /UserImei/
        public ActionResult CheckImei(string user)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckImei", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.User = user;



            var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();

            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
            var result = new UserInfoData();

            if (cInfo != null)
            {
                if (cInfo.CType == "CI")
                    result.type = "Đại lý cấp 1";
                else if (cInfo.CType == "CII")
                    result.type = "Đại lý cấp 2";
                else if (cInfo.CType == "FARMER")
                    result.type = "Nông dân";

                result.fullname = cInfo.CName;
                result.phone = cInfo.Phone;
                result.address = cInfo.AddressInfo;
               // if (cInfo.BirthDay != null)
                    result.birthday = "";
                result.user = user;
                result.area = cInfo.HaiArea.Name;
                result.code = cInfo.CCode;
                result.branch = cInfo.BranchCode;
            }
            else if (staff != null)
            {
                result.type = "Nhân viên";
                result.fullname = staff.FullName;
                result.phone = staff.Phone;
                result.address = staff.HaiBranch.Name;
                result.area = staff.HaiBranch.HaiArea.Name;
                result.user = user;
                if (staff.BirthDay != null)
                    result.birthday = staff.BirthDay.Value.ToShortDateString();
                result.code = staff.Code;
                result.branch = staff.HaiBranch.Code;
            }

            var data = db.ImeiUsers.Where(p => p.UserName == user).FirstOrDefault();
            if (data != null)
            {
                ViewBag.DataImei = data;
            }

            return View(result);
        }

        [HttpPost]
        public ActionResult ResetImei(string user)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "CheckImei", 0))
                return RedirectToAction("relogin", "home");

            var data = db.ImeiUsers.Where(p => p.UserName == user).FirstOrDefault();
            if (data != null)
            {

                data.IsUpdate = 1;
                db.Entry(data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


            }
            return RedirectToAction("CheckImei", "UserImei", new { user = user });
        }
    }
}