using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class AgencyController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        public ActionResult ManageCI(int? page, string areaId = "-1", string search = "", int? type = 1)
        {
            // type 

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.Provinces = db.Provinces.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.AreaId = areaId;
            ViewBag.AllArea = db.HaiAreas.ToList();

            ViewBag.SearchText = search;
            ViewBag.SType = type;

            if (areaId != "-1")
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C1Info.Where(p => p.HaiBranch.HaiArea.Id == areaId && p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C1Info.Where(p => p.HaiBranch.HaiArea.Id == areaId && p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(db.C1Info.Where(p => p.HaiBranch.HaiArea.Id == areaId && p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.C1Info.Where(p => p.HaiBranch.HaiArea.Id == areaId && p.HaiBranch.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(db.C1Info.Where(p => p.HaiBranch.HaiArea.Id == areaId).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                }

            }

            switch (type)
            {
                case 1:
                    ViewBag.STypeName = "Mã đại lý";
                    return View(db.C1Info.Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 2:
                    ViewBag.STypeName = "Số điện thoại";
                    return View(db.C1Info.Where(p => p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 3:
                    ViewBag.STypeName = "Tên cửa hàng";
                    return View(db.C1Info.Where(p => p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 4:
                    ViewBag.STypeName = "Mã chi nhánh";
                    return View(db.C1Info.Where(p => p.HaiBranch.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                default:
                    return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult BranchJson()
        {
            var arr = db.HaiBranches.Select(p => new { Code = p.Code, Name = p.Name }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }


        public ActionResult C1Json()
        {
            var arr = db.C1Info.Select(p => new { Code = p.Code, Name = p.StoreName }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StaffJson()
        {
            var arr = db.HaiStaffs.Where(p => p.IsDelete != 1).Select(p => new { Code = p.Code, Name = p.FullName }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }


        public ActionResult C2Json()
        {
            var arr = db.C2Info.Select(p => new { Code = p.Code, Name = p.StoreName }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ManageCI(CInfoCommon info, C1Info c1, string birthday, string BranchCode)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");


            var checkBranch = db.HaiBranches.Where(p => p.Code == BranchCode).FirstOrDefault();

            if (checkBranch == null)
                return RedirectToAction("error", "home");

            // them moi

            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                info.ProvinceName = wardInfo.District.Province.Name;
                info.DistrictName = wardInfo.District.Name;
            }

            info.CName = c1.StoreName;


            if (birthday != null)
            {
                var birthTemp = birthday.Split('/');
                if (birthTemp.Length == 3)
                {
                    try
                    {
                        info.BirthDay = Convert.ToInt32(birthTemp[1]);
                        info.BirthMonth = Convert.ToInt32(birthTemp[0]);
                        info.BirthYear = Convert.ToInt32(birthTemp[2]);
                    }
                    catch
                    {

                    }
                }
            }

            info.CreateTime = DateTime.Now;
            info.Id = Guid.NewGuid().ToString();
            info.CType = "CI";

            c1.HaiBrandId = checkBranch.Id;
            info.AreaId = checkBranch.AreaId;
            info.CCode = c1.Code;

            db.CInfoCommons.Add(info);
            db.SaveChanges();

            c1.InfoId = info.Id;
            c1.IsActive = 1;
            c1.IsLock = 0;
            c1.Id = Guid.NewGuid().ToString();



            db.C1Info.Add(c1);
            db.SaveChanges();

            return RedirectToAction("manageci", "agency", new { areaId = checkBranch.AreaId });
        }

        public ActionResult ModifyCI(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");
            var agency = db.C1Info.Find(id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }

            ViewBag.Provinces = db.Provinces.ToList();

            // danh sach quan
            var provinceId = agency.CInfoCommon.Ward.District.Provinceid;
            ViewBag.ProvinceId = provinceId;

            ViewBag.District = db.Districts.Where(p => p.Provinceid == provinceId).ToList();

            ViewBag.DistrictId = agency.CInfoCommon.Ward.Districtid;

            ViewBag.Ward = db.Wards.Where(p => p.Districtid == agency.CInfoCommon.Ward.Districtid).ToList();

            ViewBag.AGENCY = agency;
            ViewBag.RoleList = db.AspNetRoles.Where(p => p.GroupRole == "CUS").ToList();
            return View();
        }

        [HttpPost]
        public ActionResult ModifyCI(CInfoCommon info, C1Info c1, string birthday, string BranchCode)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var agency = db.C1Info.Find(info.Id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }


            var branch = db.HaiBranches.Where(p => p.Code == BranchCode).FirstOrDefault();

            if (branch == null)
            {
                return RedirectToAction("error", "home");
            }

            var infoCommon = agency.CInfoCommon;

            agency.Deputy = c1.Deputy;
            agency.StoreName = c1.StoreName;
            agency.HaiBrandId = branch.Id;
            agency.Code = c1.Code;

            if (birthday != null)
            {
                var birthTemp = birthday.Split('/');
                if (birthTemp.Length == 3)
                {
                    try
                    {
                        infoCommon.BirthDay = Convert.ToInt32(birthTemp[1]);
                        infoCommon.BirthMonth = Convert.ToInt32(birthTemp[0]);
                        infoCommon.BirthYear = Convert.ToInt32(birthTemp[2]);
                    }
                    catch
                    {

                    }
                }
            }
            infoCommon.CName = c1.StoreName;
            infoCommon.ModifyDate = DateTime.Now;
            infoCommon.WardId = info.WardId;
            infoCommon.Notes = info.Notes;
            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;
            infoCommon.Notes = info.Notes;
            infoCommon.CCode = agency.Code;
            infoCommon.Lat = info.Lat;
            infoCommon.Lng = info.Lng;
            infoCommon.IdentityCard = info.IdentityCard;


            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                infoCommon.ProvinceName = wardInfo.District.Province.Name;
                infoCommon.DistrictName = wardInfo.District.Name;
            }

            infoCommon.AreaId = branch.AreaId;

            db.Entry(agency).State = System.Data.Entity.EntityState.Modified;
            db.Entry(infoCommon).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("modifyci", "agency", new { id = agency.Id });
        }

        public ActionResult ManageCII(int? page, string areaId = "-1", string search = "", int? type = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.Provinces = db.Provinces.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.AreaId = areaId;
            ViewBag.AllArea = db.HaiAreas.ToList();

            ViewBag.SearchText = search;
            ViewBag.SType = type;

            ViewBag.BranchAll = db.HaiBranches.ToList();

            if (areaId != "-1")
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId && p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId && p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId && p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã cấp 1";
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId && p.C1Info.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 5:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId && p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(db.C2Info.Where(p => p.CInfoCommon.AreaId == areaId).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));

                }
            }

            switch (type)
            {
                case 1:
                    ViewBag.STypeName = "Mã đại lý";
                    return View(db.C2Info.Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 2:
                    ViewBag.STypeName = "Số điện thoại";
                    return View(db.C2Info.Where(p => p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 3:
                    ViewBag.STypeName = "Tên cửa hàng";
                    return View(db.C2Info.Where(p => p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 4:
                    ViewBag.STypeName = "Mã cấp 1";
                    return View(db.C2Info.Where(p => p.C1Info.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 5:
                    ViewBag.STypeName = "Mã chi nhánh";
                    return View(db.C2Info.Where(p => p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                default:
                    return View(db.C2Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));

            }
        }

        public ActionResult JsonCI(string areaId)
        {
            var listCI = db.C1Info.Where(p => p.HaiBranch.AreaId == areaId).Select(p => new { Name = p.StoreName + " - " + p.Deputy, Id = p.Id }).ToList();

            return Json(listCI, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ManageCII(CInfoCommon info, C2Info c2, string birthday, string C1Code)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            info.CName = c2.StoreName;

            if (birthday != null)
            {
                var birthTemp = birthday.Split('/');
                if (birthTemp.Length == 3)
                {
                    try
                    {
                        info.BirthDay = Convert.ToInt32(birthTemp[1]);
                        info.BirthMonth = Convert.ToInt32(birthTemp[0]);
                        info.BirthYear = Convert.ToInt32(birthTemp[2]);
                    }
                    catch
                    {

                    }
                }
            }

            info.CreateTime = DateTime.Now;
            info.Id = Guid.NewGuid().ToString();
            info.CType = "CII";

            // get area
            var c1Check = db.C1Info.Where(p => p.Code == C1Code).FirstOrDefault();

            if (c1Check == null)
                c1Check = db.C1Info.Where(p => p.Code == "0000000000").FirstOrDefault();


            var branch = db.HaiBranches.Where(p => p.Code == info.BranchCode).FirstOrDefault();

            if (branch != null)
                info.AreaId = branch.AreaId;
            else
                return RedirectToAction("error", "home");


            c2.C1Id = c1Check.Id;

            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                info.ProvinceName = wardInfo.District.Province.Name;
                info.DistrictName = wardInfo.District.Name;
            }
            info.CCode = c2.Code;
            db.CInfoCommons.Add(info);
            db.SaveChanges();

            c2.InfoId = info.Id;
            c2.IsActive = 1;

            c2.Id = Guid.NewGuid().ToString();

            db.C2Info.Add(c2);
            db.SaveChanges();

            return RedirectToAction("managecii", "agency");
        }

        /*
        // excel dai ly ci
        public ActionResult excelAgencyci(HttpPostedFileBase files)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");


            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "ci_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string code = Convert.ToString(sheet.Cells[i, 1].Value);

                        var checkCI = db.C1Info.Where(p => p.Code == code).FirstOrDefault();

                        if (checkCI == null && !String.IsNullOrEmpty(code))
                        {
                            string brandCode = Convert.ToString(sheet.Cells[i, 2].Value).Trim();

                            var branchInfo = db.HaiBranches.Where(p => p.Code == brandCode).FirstOrDefault();

                            if (branchInfo != null)
                            {
                                string storeName = Convert.ToString(sheet.Cells[i, 5].Value);

                                string deputy = Convert.ToString(sheet.Cells[i, 3].Value);

                                string position = Convert.ToString(sheet.Cells[i, 4].Value);

                                string identityCard = Convert.ToString(sheet.Cells[i, 25].Value);

                                string addressInfo = Convert.ToString(sheet.Cells[i, 6].Value);

                                string phone = Convert.ToString(sheet.Cells[i, 9].Value);

                                string fax = Convert.ToString(sheet.Cells[i, 21].Value);

                                string email = Convert.ToString(sheet.Cells[i, 19].Value);

                                string birth = Convert.ToString(sheet.Cells[i, 11].Value);

                                string mobile = Convert.ToString(sheet.Cells[i, 15].Value);

                                var cInfo = new CInfoCommon()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CName = storeName,
                                    IdentityCard = identityCard,
                                    AddressInfo = addressInfo,
                                    Phone = phone,
                                    Fax = fax,
                                    Email = email,
                                    CreateTime = DateTime.Now,
                                    CType = "CI",
                                    AreaId = branchInfo.AreaId,
                                    WardId = "11111",
                                    BranchCode = brandCode,
                                    CCode = code,
                                    CDeputy = deputy
                                };


                                var c1Info = new C1Info()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    InfoId = cInfo.Id,
                                    HaiBrandId = branchInfo.Id,
                                    Code = code,
                                    IsLock = 0,
                                    IsActive = 1,
                                    StoreName = storeName,
                                    Deputy = deputy

                                };

                                db.CInfoCommons.Add(cInfo);
                                db.SaveChanges();


                                db.C1Info.Add(c1Info);
                                db.SaveChanges();
                            }
                        }

                    }
                }
            }

            return RedirectToAction("manageci", "agency");
        }
        */

        // excel dai ly cii
        /*
        public ActionResult excelAgency(HttpPostedFileBase files)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "cii_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string code = Convert.ToString(sheet.Cells[i, 2].Value);

                        var check = db.C2Info.Where(p => p.Code == code).FirstOrDefault();

                        if (check == null && !String.IsNullOrEmpty(code))
                        {
                            string branchCode = Convert.ToString(sheet.Cells[i, 1].Value).Trim();

                            var branch = db.HaiBranches.Where(p => p.Code == branchCode).FirstOrDefault();

                            if (branch != null)
                            {

                                string storeName = Convert.ToString(sheet.Cells[i, 3].Value);

                                string deputy = Convert.ToString(sheet.Cells[i, 4].Value);

                                string identityCard = Convert.ToString(sheet.Cells[i, 5].Value);

                                string addressInfo = Convert.ToString(sheet.Cells[i, 7].Value);

                                string province = Convert.ToString(sheet.Cells[i, 9].Value);
                                string district = Convert.ToString(sheet.Cells[i, 8].Value);
                                string phone = Convert.ToString(sheet.Cells[i, 11].Value);

                                string fax = Convert.ToString(sheet.Cells[i, 12].Value);

                                string email = Convert.ToString(sheet.Cells[i, 13].Value);
                                string c1Code = Convert.ToString(sheet.Cells[i, 26].Value).Trim();

                                var c1Check = db.C1Info.Where(p => p.Code == c1Code).FirstOrDefault();

                                if (c1Check == null)
                                    c1Check = db.C1Info.Where(p => p.Code == "0000000000").FirstOrDefault();

                                var cInfo = new CInfoCommon()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CName = storeName,
                                    IdentityCard = identityCard,
                                    AddressInfo = addressInfo,
                                    ProvinceName = province,
                                    DistrictName = district,
                                    Phone = phone,
                                    Fax = fax,
                                    Email = email,
                                    CreateTime = DateTime.Now,
                                    CType = "CII",
                                    AreaId = branch.AreaId,
                                    WardId = "11111",
                                    BranchCode = branch.Code,
                                    CCode = code,
                                    CDeputy = deputy

                                };


                                var c2 = new C2Info()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    InfoId = cInfo.Id,
                                    C1Id = c1Check.Id,
                                    Code = code,
                                    IsLock = 0,
                                    IsActive = 1,
                                    StoreName = storeName,
                                    Deputy = deputy
                                };

                                db.CInfoCommons.Add(cInfo);
                                db.SaveChanges();


                                db.C2Info.Add(c2);
                                db.SaveChanges();
                            }
                        }

                    }
                }
            }

            return RedirectToAction("managecii", "agency");
        }
        */
        /*
        // excel nong dan
        public ActionResult excelfarmer(HttpPostedFileBase files)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");


            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "farmer_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;

                    string path = Server.MapPath("~/temp/" + fileSave);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {

                        string branch = Convert.ToString(sheet.Cells[i, 8].Value).Trim();

                        var branchCheck = db.HaiBranches.Where(p => p.Code == branch).FirstOrDefault();

                        if (branchCheck != null)
                        {
                            string code = Convert.ToString(sheet.Cells[i, 1].Value);

                            string name = Convert.ToString(sheet.Cells[i, 3].Value);

                            string identityCard = Convert.ToString(sheet.Cells[i, 9].Value);

                            string addressInfo = Convert.ToString(sheet.Cells[i, 4].Value);

                            string province = Convert.ToString(sheet.Cells[i, 7].Value);
                            string district = Convert.ToString(sheet.Cells[i, 6].Value);
                            string ward = Convert.ToString(sheet.Cells[i, 5].Value);

                            string phone = Convert.ToString(sheet.Cells[i, 10].Value);

                            string treeType = Convert.ToString(sheet.Cells[i, 11].Value);
                            string tree = Convert.ToString(sheet.Cells[i, 12].Value);
                            string acreage = Convert.ToString(sheet.Cells[i, 13].Value);

                            var cInfo = new CInfoCommon()
                            {
                                Id = Guid.NewGuid().ToString(),
                                CName = name,
                                IdentityCard = identityCard,
                                AddressInfo = addressInfo,
                                ProvinceName = province,
                                DistrictName = district,
                                Phone = phone,
                                CreateTime = DateTime.Now,
                                CType = "FARMER",
                                AreaId = branchCheck.AreaId,
                                WardId = "11111",
                                CCode = code,
                                BranchCode = branchCheck.Code

                            };

                            var farmer = new FarmerInfo()
                            {
                                Id = Guid.NewGuid().ToString(),
                                InfoId = cInfo.Id,
                                Code = code,
                                IsLock = 0,
                                IsActive = 1,
                                FarmerName = name,
                                TreeType = treeType,
                                Tree = tree,
                                Acreage = acreage

                            };

                            db.CInfoCommons.Add(cInfo);
                            db.SaveChanges();


                            db.FarmerInfoes.Add(farmer);
                            db.SaveChanges();
                        }


                    }


                }
            }

            return RedirectToAction("managefarmer", "agency");
        }
        */
        public ActionResult ModifyCII(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var agency = db.C2Info.Find(id);
            ViewBag.BranchAll = db.HaiBranches.ToList();
            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }

            ViewBag.CIAgency = db.C1Info.OrderBy(p => p.HaiBrandId).ToList();
            ViewBag.Provinces = db.Provinces.ToList();

            // danh sach quan
            var wardId = agency.CInfoCommon.WardId;
            if (agency.CInfoCommon.WardId != null)
            {
                var provinceId = agency.CInfoCommon.Ward.District.Provinceid;
                ViewBag.ProvinceId = provinceId;

                ViewBag.District = db.Districts.Where(p => p.Provinceid == provinceId).ToList();

                ViewBag.DistrictId = agency.CInfoCommon.Ward.Districtid;

                ViewBag.Ward = db.Wards.Where(p => p.Districtid == agency.CInfoCommon.Ward.Districtid).ToList();

            }
            else
            {

            }
            ViewBag.RoleList = db.AspNetRoles.Where(p => p.GroupRole == "CUS").ToList();
            ViewBag.AGENCY = agency;

            return View();
        }

        [HttpPost]
        public ActionResult ModifyCII(CInfoCommon info, C2Info c2, string birthday, string C1Code)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var agency = db.C2Info.Find(info.Id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }

            var infoCommon = agency.CInfoCommon;

            agency.Deputy = c2.Deputy;
            agency.StoreName = c2.StoreName;
            agency.C1Id = c2.C1Id;
            agency.Code = c2.Code;

            if (birthday != null)
            {
                var birthTemp = birthday.Split('/');
                if (birthTemp.Length == 3)
                {
                    try
                    {
                        infoCommon.BirthDay = Convert.ToInt32(birthTemp[1]);
                        infoCommon.BirthMonth = Convert.ToInt32(birthTemp[0]);
                        infoCommon.BirthYear = Convert.ToInt32(birthTemp[2]);
                    }
                    catch
                    {

                    }
                }
            }

            infoCommon.CName = c2.StoreName;
            infoCommon.ModifyDate = DateTime.Now;
            infoCommon.WardId = info.WardId;
            infoCommon.Notes = info.Notes;
            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;
            infoCommon.Notes = info.Notes;
            infoCommon.BackName = info.BackName;
            infoCommon.BranchCode = info.BranchCode;
            infoCommon.BankAccountHolder = info.BankAccountHolder;
            infoCommon.PlaceOfBirth = info.PlaceOfBirth;
            infoCommon.AddressInfo = info.AddressInfo;
            infoCommon.Lat = info.Lat;
            infoCommon.Lng = info.Lng;
            infoCommon.CCode = agency.Code;
            infoCommon.BranchCode = info.BranchCode;
            infoCommon.IdentityCard = info.IdentityCard;

            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                infoCommon.ProvinceName = wardInfo.District.Province.Name;
                infoCommon.DistrictName = wardInfo.District.Name;
            }

            // get area
            var c1Check = db.C1Info.Where(p => p.Code == C1Code).FirstOrDefault();

            if (c1Check == null)
                c1Check = db.C1Info.Where(p => p.Code == "0000000000").FirstOrDefault();


            var branch = db.HaiBranches.Where(p => p.Code == info.BranchCode).FirstOrDefault();
            if (branch != null)
                infoCommon.AreaId = branch.AreaId;
            else
                return RedirectToAction("error", "home");

            agency.C1Id = c1Check.Id;


            db.Entry(agency).State = System.Data.Entity.EntityState.Modified;
            db.Entry(infoCommon).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("modifycii", "agency", new { id = agency.Id });
        }

        // nông dân

        public ActionResult ManageFarmer(int? page, string areaId = "-1", string search = "", int? type = 2)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 0))
                return RedirectToAction("relogin", "home");

            ViewBag.Provinces = db.Provinces.ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.BranchAll = db.HaiBranches.ToList();
            ViewBag.AreaId = areaId;
            ViewBag.AllArea = db.HaiAreas.ToList();

            ViewBag.SearchText = search;
            ViewBag.SType = type;
            if (areaId != "-1")
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã khách hàng";
                        return View(db.FarmerInfoes.Where(p => p.CInfoCommon.AreaId == areaId && p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.FarmerInfoes.Where(p => p.CInfoCommon.AreaId == areaId && p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên khách hàng";
                        return View(db.FarmerInfoes.Where(p => p.CInfoCommon.AreaId == areaId && p.FarmerName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.FarmerInfoes.Where(p => p.CInfoCommon.AreaId == areaId && p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(db.FarmerInfoes.Where(p => p.CInfoCommon.AreaId == areaId).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));

                }
            }


            switch (type)
            {
                case 1:
                    ViewBag.STypeName = "Mã khách hàng";
                    return View(db.FarmerInfoes.Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 2:
                    ViewBag.STypeName = "Số điện thoại";
                    return View(db.FarmerInfoes.Where(p => p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 3:
                    ViewBag.STypeName = "Tên khách hàng";
                    return View(db.FarmerInfoes.Where(p => p.FarmerName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                case 4:
                    ViewBag.STypeName = "Mã chi nhánh";
                    return View(db.FarmerInfoes.Where(p => p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                default:
                    return View(db.FarmerInfoes.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));

            }
        }

        [HttpPost]
        public ActionResult ManageFarmer(CInfoCommon info, FarmerInfo farmer, string birthday)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");
            info.CName = farmer.FarmerName;

            info.CreateTime = DateTime.Now;

            DateTime dt = DateTime.ParseExact(birthday, "MM/dd/yyyy HH:mm", null);
            info.CreateTime = DateTime.Now;
            info.Id = Guid.NewGuid().ToString();
            info.CType = "FARMER";
            info.CCode = farmer.Code;

            var branch = db.HaiBranches.Where(p => p.Code == info.BranchCode).FirstOrDefault();
            if (branch != null)
                info.AreaId = branch.AreaId;
            else
                return RedirectToAction("error", "home");


            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                info.ProvinceName = wardInfo.District.Province.Name;
                info.DistrictName = wardInfo.District.Name;
            }

            db.CInfoCommons.Add(info);
            db.SaveChanges();

            farmer.InfoId = info.Id;
            farmer.IsActive = 1;
            farmer.IsLock = 0;
            farmer.Id = Guid.NewGuid().ToString();

            db.FarmerInfoes.Add(farmer);
            db.SaveChanges();

            return RedirectToAction("managefarmer", "agency");
        }

        public ActionResult ModifyFarmer(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");
            var agency = db.FarmerInfoes.Find(id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }
            ViewBag.BranchAll = db.HaiBranches.ToList();
            ViewBag.Provinces = db.Provinces.ToList();
            // ViewBag.AllArea = db.HaiAreas.ToList();
            // danh sach quan
            var provinceId = agency.CInfoCommon.Ward.District.Provinceid;
            ViewBag.ProvinceId = provinceId;

            ViewBag.District = db.Districts.Where(p => p.Provinceid == provinceId).ToList();

            ViewBag.DistrictId = agency.CInfoCommon.Ward.Districtid;

            ViewBag.Ward = db.Wards.Where(p => p.Districtid == agency.CInfoCommon.Ward.Districtid).ToList();
            ViewBag.RoleList = db.AspNetRoles.Where(p => p.GroupRole == "CUS").ToList();
            ViewBag.AGENCY = agency;

            return View();
        }

        [HttpPost]
        public ActionResult ModifyFarmer(CInfoCommon info, FarmerInfo farmer, string birthday)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");
            var agency = db.FarmerInfoes.Find(info.Id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }

            var infoCommon = agency.CInfoCommon;

            agency.FarmerName = farmer.FarmerName;
            agency.Code = farmer.Code;


            var branch = db.HaiBranches.Where(p => p.Code == info.BranchCode).FirstOrDefault();
            if (branch != null)
                infoCommon.AreaId = branch.AreaId;
            else
                return RedirectToAction("error", "home");


            //  DateTime dt = DateTime.ParseExact(birthday, "MM/dd/yyyy HH:mm", null);
            // infoCommon.BirthDay = dt;
            infoCommon.CName = farmer.FarmerName;
            infoCommon.ModifyDate = DateTime.Now;
            infoCommon.WardId = info.WardId;
            infoCommon.Notes = info.Notes;
            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;
            infoCommon.Notes = info.Notes;
            infoCommon.AreaId = info.AreaId;
            infoCommon.CCode = agency.Code;
            infoCommon.IdentityCard = info.IdentityCard;
            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                infoCommon.ProvinceName = wardInfo.District.Province.Name;
                infoCommon.DistrictName = wardInfo.District.Name;
            }
            db.Entry(agency).State = System.Data.Entity.EntityState.Modified;
            db.Entry(infoCommon).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("modifyfarmer", "agency", new { id = agency.Id });
        }


        public ActionResult JsonDistict(string Provinceid)
        {
            var district = db.Districts.Where(p => p.Provinceid == Provinceid).Select(p => new { Id = p.Id, Name = p.Name }).ToList();

            return Json(district, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JsonWard(string Id)
        {
            var district = db.Wards.Where(p => p.Districtid == Id).Select(p => new { Id = p.Id, Name = p.Name }).ToList();

            return Json(district, JsonRequestBehavior.AllowGet);
        }

        /*
        public ActionResult LockUser(string Id, string Type, int Status, string Agency)
        {


            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var check = db.CInfoCommons.Find(Id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }

            if (Status == 1)
            {
                check.IsClock = 1;
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else if (Status == 2)
            {
                // mo khoa
                if (check.IsDelete != 1)
                {
                    check.IsClock = 0;
                    db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else if (Status == 0)
            {
                if (check.IsClock == 1)
                {
                    check.IsDelete = 1;
                    db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (Type == "CI")
            {
                return RedirectToAction("modifyci", "agency", new { id = Agency });
            }
            else if (Type == "CII")
            {
                return RedirectToAction("modifycii", "agency", new { id = Agency });
            }
            else if (Type == "Farmer")
            {
                return RedirectToAction("modifyfarmer", "agency", new { id = Agency });
            }
            else
            {
                return RedirectToAction("error", "home");
            }

        }
        */

        [HttpPost]
        public ActionResult ActiveUserC2(string Id, int Status)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var check = db.C2Info.Find(Id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }

            if (Status == 1)
            {
                // khoa
                check.IsActive = 0;
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else if (Status == 2)
            {
                // mo khoa
                check.IsActive = 1;
                db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }


            return RedirectToAction("modifycii", "agency", new { id = Id });

        }


        #region tao khach hang c2 va tai khoan
        /***
         * 
         * file excel update-ds-khach-hang-c2-v2.xlsx
         * 
         */

        public ActionResult UpdateAgencyC2V2()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult UpdateAgencyC2V2(HttpPostedFileBase files)
        {
            List<string> listFail = new List<string>();
            if (files != null && files.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx"))
                {

                    string fileSave = "cii_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string code = Convert.ToString(sheet.Cells[i, 1].Value);

                        var check = db.C2Info.Where(p => p.Code == code).FirstOrDefault();

                        if (check == null && !String.IsNullOrEmpty(code))
                        {
                            string branchCode = Convert.ToString(sheet.Cells[i, 3].Value).Trim();

                            var branch = db.HaiBranches.Where(p => p.Code == branchCode).FirstOrDefault();

                            if (branch != null)
                            {

                                string storeName = Convert.ToString(sheet.Cells[i, 4].Value);

                                string deputy = Convert.ToString(sheet.Cells[i, 5].Value);

                                string identityCard = Convert.ToString(sheet.Cells[i, 7].Value);
                                string phone = Convert.ToString(sheet.Cells[i, 6].Value);

                                string bussinessLicence = Convert.ToString(sheet.Cells[i, 8]);

                                string addressInfo = Convert.ToString(sheet.Cells[i, 9].Value);

                                string c1Code = Convert.ToString(sheet.Cells[i, 2].Value);

                                var c1Check = db.C1Info.Where(p => p.Code == c1Code).FirstOrDefault();

                                if (c1Check == null)
                                    c1Check = db.C1Info.Where(p => p.Code == "0000000000").FirstOrDefault();



                                var cInfo = new CInfoCommon()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CName = storeName,
                                    IdentityCard = identityCard,
                                    AddressInfo = addressInfo,
                                    Phone = phone,
                                    CreateTime = DateTime.Now,
                                    CType = "CII",
                                    AreaId = branch.AreaId,
                                    BranchCode = branch.Code,
                                    CCode = code,
                                    WardId = "11111",
                                    CDeputy = deputy
                                };

                                string ward = Convert.ToString(sheet.Cells[i, 10].Value);

                                var wardInfo = db.Wards.Find(ward);

                                if (wardInfo != null)
                                {
                                    cInfo.WardId = ward;
                                }


                                var c2 = new C2Info()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    InfoId = cInfo.Id,
                                    C1Id = c1Check.Id,
                                    Code = code,
                                    IsActive = 1,
                                    StoreName = storeName,
                                    Deputy = deputy
                                };

                                db.CInfoCommons.Add(cInfo);
                                db.SaveChanges();


                                db.C2Info.Add(c2);
                                db.SaveChanges();


                            }
                        }
                        else
                        {
                            listFail.Add(code);
                        }

                    }
                }
            }

            return View(listFail);
        }
        #endregion

        ///
        /// remove c2
        ///

        public ActionResult RemoveC2(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var check = db.C2Info.Find(Id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }


            var staffc2 = check.StaffWithC2.ToList();
            staffc2.Clear();
            db.SaveChanges();

            CInfoCommon cinfo = check.CInfoCommon;
            db.C2Info.Remove(check);

            db.SaveChanges();

            db.CInfoCommons.Remove(cinfo);
            db.SaveChanges();

            return RedirectToAction("managecii", "agency");

        }

    }
}