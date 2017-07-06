using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;

namespace NDHSITE.Controllers
{

    [Authorize]
    public class HaiStaffController : Controller
    {


        NDHDBEntities db = new NDHDBEntities();

        public ActionResult JsonStaff(string branch)
        {
            var model = db.HaiStaffs.Where(p => p.BranchId == branch).Select(p => new { Id = p.Id, Name = p.FullName}).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateStaff(int? page, int? func, string branchId, string areaId, string departmentId, string posId, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 0))
                return RedirectToAction("relogin", "home");


            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.FuncShow = prepareFuncShowStaff();
            if (func == null)
                func = 0;
            ViewBag.AllBranch = db.HaiBranches.ToList();
            ViewBag.AllArea = db.HaiAreas.ToList();
            ViewBag.AllDepartment = db.HaiDepartments.ToList();
            ViewBag.AllPosition = db.HaiPositions.ToList();

            ViewBag.FuncId = func;
            ViewBag.SearchText = search;
            ViewBag.AreaId = areaId;
            ViewBag.BranchId = branchId;
            ViewBag.DepartmentId = departmentId;
            ViewBag.PosId = posId;


            switch (func)
            {
                case 0:
                    return View(db.HaiStaffs.Where(p=> p.IsLock != 1).OrderByDescending(p => p.DepartmentId).ToPagedList(pageNumber, pageSize));
                case 1:
                    return View(db.HaiStaffs.Where(p => p.HaiBranch.AreaId == areaId && p.IsLock != 1).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 2:
                    return View(db.HaiStaffs.Where(p => p.BranchId == branchId && p.IsLock != 1).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 3:
                    return View(db.HaiStaffs.Where(p => p.DepartmentId == departmentId && p.IsLock != 1).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 4:
                    return View(db.HaiStaffs.Where(p => p.PositionId == posId && p.IsLock != 1).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
                case 5:
                    return View(db.HaiStaffs.Where(p => p.FullName.Contains(search)).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));

            }


            return View(db.HaiStaffs.OrderByDescending(p => p.DepartmentId).ToPagedList(pageNumber, pageSize));
        }

        private List<FuncShow> prepareFuncShowStaff()
        {
            List<FuncShow> arr = new List<FuncShow>();
            arr.Add(new FuncShow()
            {
                Name = "--Tất cả--",
                Value = 0
            });
            arr.Add(new FuncShow()
            {
                Name = "Theo khu vực",
                Value = 1
            });
            arr.Add(new FuncShow()
            {
                Name = "Theo chi nhánh",
                Value = 2
            });
            arr.Add(new FuncShow()
            {
                Name = "Theo phòng ban",
                Value = 3
            });
            arr.Add(new FuncShow()
            {
                Name = "Theo chức vụ",
                Value = 4
            });
            arr.Add(new FuncShow()
            {
                Name = "Tìm theo tên",
                Value = 5
            });
            return arr;
        }

        [HttpPost]
        public ActionResult CreateStaff(HaiStaff staff, string birthday, HttpPostedFileBase avatar, HttpPostedFileBase signature)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");

            staff.CreateDate = DateTime.Now;
            staff.IsLock = 0;

            try
            {
                DateTime dt = DateTime.ParseExact(birthday, "MM/dd/yyyy HH:mm", null);
                string urlAvatar = UploadImage(avatar, "/staff/avatar", ".jpg", staff.Code);
                string urlSignature = UploadImage(signature, "/staff/signature", ".png", staff.Code);

                staff.AvatarUrl = urlAvatar;
                staff.SignatureUrl = urlSignature;
                staff.Id = Guid.NewGuid().ToString();
                staff.IsLock = 0;
                db.HaiStaffs.Add(staff);
                db.SaveChanges();

            }
            catch
            {

            }


            return RedirectToAction("createstaff", "haistaff", new { brand = staff.BranchId });
        }

        [HttpPost]
        public ActionResult ActiveAccount(string id, int clock) {


            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var staff = db.HaiStaffs.Find(id);

            if (staff == null)
            {
                return RedirectToAction("error", "home");
            }


            staff.IsLock = clock;
            db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("modifystaff", "haistaff", new {id = id});
        }

        private string UploadImage(HttpPostedFileBase img, string path, string extension, string fileName)
        {
            if (img != null)
            {

                MemoryStream target = new MemoryStream();
                img.InputStream.CopyTo(target);
                byte[] data = target.ToArray();

                ImageUpload imageUpload = new ImageUpload
                {
                    Width = 500,
                    isSacle = false,
                    UploadPath = "~" + path
                };

                ImageResult imageResult = imageUpload.RenameUploadAvatar(data, extension, fileName);

                if (imageResult.Success)
                {
                    return path + "/" + imageResult.ImageName;
                }

            }
            return "";
        }

        public ActionResult ModifyStaff(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var staff = db.HaiStaffs.Find(Id);

            if (staff == null)
            {
                return RedirectToAction("error", "home");
            }

            ViewBag.RoleList = db.AspNetRoles.Where(p => p.GroupRole == "HAI").ToList();
            ViewBag.AllBranch = db.HaiBranches.ToList();
            ViewBag.AllDepartment = db.HaiDepartments.ToList();
            ViewBag.AllPosition = db.HaiPositions.ToList();

            // choose tab active
            ViewBag.TabActive = "1";
            ViewBag.StaffInfo = staff;
            return View();
        }

        [HttpPost]
        public ActionResult ModifyStaff(HaiStaff staff, string birthday, HttpPostedFileBase avatar, HttpPostedFileBase signature)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var staffCheck = db.HaiStaffs.Find(staff.Id);

            if (staffCheck != null)
            {
                if (birthday != "" && birthday != null)
                {
                    DateTime dt = DateTime.ParseExact(birthday, "MM/dd/yyyy HH:mm", null);
                    staffCheck.BirthDay = dt;
                }

                staffCheck.FullName = staff.FullName;
                staffCheck.Code = staff.Code;
                staffCheck.BranchId = staff.BranchId;
                staffCheck.DepartmentId = staff.DepartmentId;
                staffCheck.PositionId = staff.PositionId;
                staffCheck.Notes = staff.Notes;
                staffCheck.Phone = staff.Phone;
                staffCheck.Email = staff.Email;
               
                staffCheck.PlaceOfBirth = staff.PlaceOfBirth;

                if (avatar != null)
                {
                    staffCheck.AvatarUrl = UploadImage(avatar, "/staff/avatar", ".jpg", staff.Code);
                }

                if (signature != null)
                {
                    staffCheck.SignatureUrl = UploadImage(signature, "/staff/signature", ".png", staff.Code);
                }


                db.Entry(staffCheck).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                ViewBag.TabActive = "1";

                return RedirectToAction("modifystaff", "haistaff", new { Id = staffCheck.Id });
            }
            else
                return RedirectToAction("error", "home");

        }

        // quản lý chi nhánh
        public ActionResult CreateBrand(int? page, string areaId, string search, int? tab)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 0))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.AllArea = db.HaiAreas.ToList();
            ViewBag.AreaId = areaId;
            if (search == null)
                search = "";
            ViewBag.SearchText = search;
            if (areaId == null)
                areaId = "-1";

            if (tab == null)
                tab = 0; // tab 0

            ViewBag.Tab = tab;

            if (areaId == "-1")
            {
                return View(db.HaiBranches.Where( p=> p.Name.Contains(search) || p.Code.Contains(search)).OrderByDescending(p => p.AreaId).ToPagedList(pageNumber, pageSize));
            }

            return View(db.HaiBranches.Where(p => p.AreaId == areaId && (p.Name.Contains(search) || p.Code.Contains(search))).OrderByDescending(p => p.Name).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ModifyBranch(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var branch = db.HaiBranches.Find(id);

            if (branch == null)
                return RedirectToAction("error", "home");
            ViewBag.AllArea = db.HaiAreas.ToList();
            return View(branch);
        }

        [HttpPost]
        public ActionResult ModifyBranch(HaiBranch branch)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");

            var branchCheck = db.HaiBranches.Find(branch.Id);

            if (branchCheck == null)
                return RedirectToAction("error", "home");


            branchCheck.Name = branch.Name;
            branchCheck.Code = branch.Code;
            branchCheck.AreaId = branch.AreaId;
            branchCheck.AddressInfo = branch.AddressInfo;
            branchCheck.Notes = branch.Notes;

            db.Entry(branchCheck).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("modifybranch");
        }

        [HttpPost]
        public ActionResult CreateBrand(HaiBranch branch)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            db.HaiBranches.Add(branch);
            db.SaveChanges();

            return RedirectToAction("createbrand", "haistaff");
        }


        // khu vuc
        [HttpPost]
        public ActionResult CreateArea(HaiArea area)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            db.HaiAreas.Add(area);
            db.SaveChanges();

            return RedirectToAction("createbrand", "haistaff", new { search = "", tab = 3});
        }

        public ActionResult ModifyArea(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var area = db.HaiAreas.Find(id);

            if(area == null)
                return RedirectToAction("error", "home");

            return View(area);
        }

        [HttpPost]
        public ActionResult ModifyArea(HaiArea area)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var areaCheck = db.HaiAreas.Find(area.Id);

            if (areaCheck == null)
                return RedirectToAction("error", "home");

            areaCheck.Name = area.Name;
            areaCheck.Notes = area.Notes;

            db.Entry(areaCheck).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("createbrand", "haistaff", new { search = "", tab = 3 });
        }

        [HttpPost]
        public ActionResult DeleteArea(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var area = db.HaiAreas.Find(id);

            if (area == null)
                return RedirectToAction("error", "home");

            db.HaiAreas.Remove(area);
            db.SaveChanges();

            return RedirectToAction("createbrand", "haistaff", new { search = "", tab = 3 });
        }


        // phong ban
        public ActionResult CreateDepartment(int? page, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 0))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (search == null)
                search = "";

            ViewBag.SearchText = search;

            return View(db.HaiDepartments.Where(p=> p.Name.Contains(search)).OrderByDescending(p=> p.Name).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult CreateDepartment(HaiDepartment department)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            db.HaiDepartments.Add(department);
            db.SaveChanges();

            return RedirectToAction("CreateDepartment", "haistaff");
        }

        public ActionResult ModifyDepartment(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");

            var dep = db.HaiDepartments.Find(id);

            if (dep == null)
                return RedirectToAction("error", "home");

            return View(dep);

        }

        [HttpPost]
        public ActionResult ModifyDepartment(HaiDepartment department)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var dep = db.HaiDepartments.Find(department.Id);

            if (dep == null)
                return RedirectToAction("error", "home");

            dep.Name = department.Name;
            dep.Notes = department.Notes;

            db.Entry(dep).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("CreateDepartment", "haistaff");

        }

        [HttpPost]
        public ActionResult DeleteDepartment(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var dep = db.HaiDepartments.Find(id);

            if (dep == null)
                return RedirectToAction("error", "home");

            db.HaiDepartments.Remove(dep);
            db.SaveChanges();

            return RedirectToAction("CreateDepartment", "haistaff");

        }

        // chuc vụ
        public ActionResult CreatePosition(int? page, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 0))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (search == null)
                search = "";

            ViewBag.SearchText = search;


            return View(db.HaiPositions.Where(p => p.Name.Contains(search)).OrderByDescending(p => p.Name).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult CreatePosition(HaiPosition position)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            db.HaiPositions.Add(position);
            db.SaveChanges();

            return RedirectToAction("CreatePosition", "haistaff");
        }

        public ActionResult ModifyPosition(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var pos = db.HaiPositions.Find(id);

            if (pos == null)
                return RedirectToAction("error", "home");

            return View(pos);
        }

        [HttpPost]
        public ActionResult ModifyPosition(HaiPosition position)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var pos = db.HaiPositions.Find(position.Id);

            if (pos == null)
                return RedirectToAction("error", "home");


            pos.Name = position.Name;
            pos.Notes = position.Notes;
            db.Entry(pos).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("CreatePosition", "haistaff");
        }

        [HttpPost]
        public ActionResult DeletePosition(int id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageStaff", 1))
                return RedirectToAction("relogin", "home");
            var pos = db.HaiPositions.Find(id);

            if (pos == null)
                return RedirectToAction("error", "home");

            db.HaiPositions.Remove(pos);
            db.SaveChanges();

            return RedirectToAction("CreatePosition", "haistaff");
        }

        public ActionResult JsonAllBranch(string Id)
        {
            var model = db.HaiBranches.Where(p => p.AreaId == Id).Select(p => new { Name = p.Name, Id = p.Id, Address = p.AddressInfo, Phone = p.Phone, Notes = p.Notes }).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult JsonAllStaff(string Id)
        {
            var model = db.HaiStaffs.Where(p => p.BranchId == Id && p.IsLock == 0).Select(p => new { Name = p.FullName, Id = p.Id, Department = p.HaiDepartment.Name, Position = p.HaiPosition.Name, Notes = p.Notes, UserLogin = p.UserLogin }).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

    
    }
}