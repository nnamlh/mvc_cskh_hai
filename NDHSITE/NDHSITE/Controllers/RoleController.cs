using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using PagedList;

namespace NDHSITE.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class RoleController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();


        private ApplicationDbContext sdb = new ApplicationDbContext();

        public RoleManager<IdentityRole> RoleManager { get; private set; }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        public RoleController()
        {
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(sdb));
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(sdb));
        }


        public ActionResult Manager(int? page, string searchString)
        {
  
            ViewBag.CurrentFilter = searchString;
            ViewBag.Roles = sdb.Roles.ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);


            if (!String.IsNullOrEmpty(searchString))
            {
                return View(sdb.Users.Where(p => p.UserName.Contains(searchString) || p.FullName.Contains(searchString)).OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize));
            }


            return View(sdb.Users.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult Update(String userId, String roleName)
        {
            var user = UserManager.FindById(userId);

            var userRole = user.Roles.ToList();

            foreach (var role in userRole)
            {
                UserManager.RemoveFromRole(user.Id, role.Role.Name);
            }


            if (roleName != "none")
            {
                UserManager.AddToRole(user.Id, roleName);
            }
            else
            {
                return Content("The role was removed !");
            }

            return Content("The role was added !");
        }

        // phan quyen
        public ActionResult Permission(string roleName)
        {
            ViewBag.RoleName = roleName;

            if (roleName != null && roleName != "")
                ViewBag.FuncList = db.FuncInfoes.OrderBy(p => p.GroupId).ToList();

            
            ViewBag.Roles = sdb.Roles.ToList();
            return View(db.FuncRoles.Where(p=> p.AspNetRole.Name == roleName).ToList());
        }

        public ActionResult AddFuncToRole(int FuncId, String RoleName, bool isAdd)
        {

            var func = db.FuncInfoes.Find(FuncId);
            if (func == null)
                return Content("Chức năng sai.");

            var role = db.AspNetRoles.Where(p => p.Name == RoleName).FirstOrDefault();

            if (role == null)
                return Content("Quyền sai.");

            var funcRole = db.FuncRoles.Where(p => p.FuncId == func.Id && p.RoleId == role.Id).FirstOrDefault();

            if (isAdd && funcRole == null)
            {
                var newFuncRole = new FuncRole()
                {
                    FuncId = func.Id,
                    RoleId = role.Id,
                    IsAll = 0
                };

                db.FuncRoles.Add(newFuncRole);
                db.SaveChanges();
                return Content("Đã add");

            }


            if (!isAdd && funcRole != null)
            {
                db.FuncRoles.Remove(funcRole);
                db.SaveChanges();

                return Content("Đã xóa");
            }


            return Content("Không thành công.");
        }

        public ActionResult AddFuncIsAll(int FuncId, String RoleName, bool isAdd)
        {

            var func = db.FuncInfoes.Find(FuncId);
            if (func == null)
                return Content("Chức năng sai.");

            var role = db.AspNetRoles.Where(p => p.Name == RoleName).FirstOrDefault();

            if (role == null)
                return Content("Quyền sai.");

            var funcRole = db.FuncRoles.Where(p => p.FuncId == func.Id && p.RoleId == role.Id).FirstOrDefault();

            if (funcRole == null)
                return Content("Chua thêm chức năng này.");

            if (isAdd)
                funcRole.IsAll = 1;
            else
                funcRole.IsAll = 0;

            db.Entry(funcRole).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Content("Đã sét toàn quyền.");
        }


        // mobile
        // phan quyen
        public ActionResult MobileFunction(string roleName)
        {
            if (roleName == null)
                roleName = "Administrator";

            ViewBag.RoleName = roleName;

            if (roleName != null && roleName != "")
                ViewBag.FuncList = db.MobileFunctions.OrderBy(p=> p.ScreenType).ToList();


            ViewBag.Roles = sdb.Roles.ToList();

            var roles = db.AspNetRoles.Where(p=> p.Name == roleName).FirstOrDefault();

            return View(roles.MobileFunctions.ToList());
        }

        public ActionResult AddFuncToMobile(string FuncId, String RoleName, bool isAdd)
        {

            var func = db.MobileFunctions.Find(FuncId);
            if (func == null)
                return Content("Chức năng sai.");

            var role = db.AspNetRoles.Where(p => p.Name == RoleName).FirstOrDefault();

            if (role == null)
                return Content("Quyền sai.");

            var funcRole = func.AspNetRoles.Where(p=> p.Id == role.Id).FirstOrDefault();

            if (isAdd && funcRole == null)
            {
                func.AspNetRoles.Add(role);
                db.SaveChanges();
                return Content("Đã add");

            }


            if (!isAdd && funcRole != null)
            {
                func.AspNetRoles.Remove(role);
                db.SaveChanges();

                return Content("Đã xóa");
            }


            return Content("Không thành công.");
        }

    }
}