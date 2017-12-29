﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;
using System.IO;
using OfficeOpenXml;
using System.Text.RegularExpressions;

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
                        return View(db.C1Info.Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C1Info.Where(p => p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(db.C1Info.Where(p => p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
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
                    return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                default:
                    return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
            }
        }

        /*
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
        */

        [HttpPost]
        public ActionResult ManageCI(CInfoCommon info, C1Info c1, string birthday, string BranchCode)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");


            var checkBranch = db.HaiBranches.Where(p => p.Code == BranchCode).FirstOrDefault();

            if (checkBranch == null)
                return RedirectToAction("error", "home");
            /*
            // them moi

            var wardInfo = db.Wards.Find(info.WardId);
            if (wardInfo != null)
            {
                info.ProvinceName = wardInfo.District.Province.Name;
                info.DistrictName = wardInfo.District.Name;
            }*/

            info.CName = c1.StoreName;


            if (birthday != null)
            {

            }

            info.CreateTime = DateTime.Now;
            info.Id = Guid.NewGuid().ToString();
            info.CType = "CI";

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
            /*
            var provinceId = agency.CInfoCommon.Ward.District.Provinceid;
            ViewBag.ProvinceId = provinceId;

            ViewBag.District = db.Districts.Where(p => p.Provinceid == provinceId).ToList();

            ViewBag.DistrictId = agency.CInfoCommon.Ward.Districtid;

            ViewBag.Ward = db.Wards.Where(p => p.Districtid == agency.CInfoCommon.Ward.Districtid).ToList();
            */

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
            agency.Code = c1.Code;

            infoCommon.CName = c1.StoreName;
            infoCommon.ModifyDate = DateTime.Now;
            infoCommon.Notes = info.Notes;
            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;
            infoCommon.Notes = info.Notes;
            infoCommon.CCode = agency.Code;
            infoCommon.Lat = info.Lat;
            infoCommon.Lng = info.Lng;
            infoCommon.IdentityCard = info.IdentityCard;



            db.Entry(agency).State = System.Data.Entity.EntityState.Modified;
            db.Entry(infoCommon).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("modifyci", "agency", new { id = agency.Id });
        }

        public ActionResult ManageCII(int? page, int status = -1, string search = "", int? type = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 30;
            int pageNumber = (page ?? 1);
            ViewBag.Status = status;

            ViewBag.SearchText = search;
            ViewBag.SType = type;

            ViewBag.AgencyType = db.AgencyTypes.ToList();

            ViewBag.BranchAll = db.HaiBranches.ToList();

            ViewBag.Province = db.Provinces.ToList();

            if (status != -1)
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C2Info.Where(p => p.IsActive == status && p.Code.Contains(search) && p.OldData == 0).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C2Info.Where(p => p.IsActive == status && p.CInfoCommon.Phone.Contains(search) && p.OldData == 0).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(db.C2Info.Where(p => p.IsActive == status && p.StoreName.Contains(search) && p.OldData == 0).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.C2Info.Where(p => p.IsActive == status && p.CInfoCommon.BranchCode.Contains(search) && p.OldData == 0).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(new List<C2Info>());

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
                    ViewBag.STypeName = "Mã chi nhánh";
                    return View(db.C2Info.Where(p => p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                default:
                    return View(new List<C2Info>());

            }
        }

        /*
        public ActionResult JsonCI(string areaId)
        {
            var listCI = db.C1Info.Select(p => new { Name = p.StoreName + " - " + p.Deputy, Id = p.Id }).ToList();

            return Json(listCI, JsonRequestBehavior.AllowGet);
        }
        */

        [HttpPost]
        public ActionResult ManageCII(CInfoCommon info, C2Info c2, string Province)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            info.CName = c2.StoreName;

            info.CreateTime = DateTime.Now;
            info.Id = Guid.NewGuid().ToString();
            info.CType = "CII";

            var branch = db.HaiBranches.Where(p => p.Code == info.BranchCode).FirstOrDefault();

            if (branch == null)
                return RedirectToAction("error", "home");

            var checkProvince = db.Provinces.Where(p => p.Code == Province).FirstOrDefault();
            if (checkProvince == null)
                return RedirectToAction("error", "home");

            // check code
            var code = c2.Code;
            if (String.IsNullOrEmpty(c2.Code))
            {
                code = GetAgencyCode(checkProvince.Code);
            }
            else
            {
                var checkCode = db.C2Info.Where(p => p.Code == code).FirstOrDefault();
                if (checkCode != null)
                {
                    // tao code tu dong
                    // checkProvince
                    code = GetAgencyCode(checkProvince.Code);
                }
            }

            info.ProvinceName = checkProvince.Name;
            info.ProvinceCode = checkProvince.Code;

            c2.Code = code;
            info.CCode = code;
            db.CInfoCommons.Add(info);
            db.SaveChanges();

            c2.InfoId = info.Id;
            c2.IsActive = 1;

            c2.Id = Guid.NewGuid().ToString();

            db.C2Info.Add(c2);
            db.SaveChanges();

            //  return RedirectToAction("managecii", "agency");
            return RedirectToAction("modifycii", "agency", new { id = c2.Id });

        }

        public ActionResult ProvinceJson()
        {
            var arr = db.Provinces.Select(p => new { Name = p.Name }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DistrictJson()
        {
            var arr = db.Districts.Select(p => new { Name = p.Name }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WardJson()
        {
            var arr = db.Wards.Select(p => new { Name = p.Name }).ToList();

            return Json(arr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ModifyCII(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var agency = db.C2Info.Find(id);

            if (agency == null)
            {
                return RedirectToAction("error", "home");
            }

            ViewBag.C1C2 = db.c2_get_list_c1(agency.Code).ToList();

            ViewBag.BranchAll = db.HaiBranches.ToList();
            ViewBag.AgencyType = db.AgencyTypes.ToList();
            ViewBag.RoleList = db.AspNetRoles.Where(p => p.GroupRole == "CUS").ToList();
            ViewBag.AGENCY = agency;

            return View();
        }

        [HttpPost]
        public ActionResult AddC1ForC2(string c2, string c1, int action, int priority)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var checkC2 = db.C2Info.Find(c2);
            var checkC1 = db.C1Info.Where(p => p.Code == c1).FirstOrDefault();
            if (checkC2 != null && checkC1 != null)
            {
               var checkC2C1 = db.C2C1.Where(p => p.C1Code == c1 && p.C2Code == checkC2.Code).FirstOrDefault();
               if (action == 1)
                {
                    // them
                    if (checkC2C1 == null)
                    {
                        // db.C2C1.Add(checkC2C1);

                        C2C1 c2C1Add = new C2C1()
                        {
                            Id = Guid.NewGuid().ToString(),
                            C1Code = checkC1.Code,
                            C2Code = checkC2.Code,
                            Priority = priority,
                            ModifyDate = DateTime.Now
                        };

                        db.C2C1.Add(c2C1Add);
                        db.SaveChanges();
                    }
                } else if (action == 2)
                {
                    // xoa
                    if (checkC2C1 != null)
                    {
                        db.C2C1.Remove(checkC2C1);
                        db.SaveChanges();
                    }
                }

            }
            return RedirectToAction("modifycii", "agency", new { id = c2 });
        }

        /*
        [HttpGet]
        public ActionResult DeleteC2C1(string c2Code, string c1Code, string cType, string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageAgency", 1))
                return RedirectToAction("relogin", "home");

            var check = db.C2C1.Where(p => p.C1Code == c1Code && p.C2Code == c2Code).FirstOrDefault();

            if (check != null)
            {
                db.C2C1.Remove(check);
                db.SaveChanges();
            }

            if (cType == "CII")
            {
                return RedirectToAction("modifycii", "agency", new { id = id });
            }
            else if (cType == "CI")
            {
                return RedirectToAction("modifyci", "agency", new { id = id });
            }
            else
            {
                return RedirectToAction("index", "home");
            }
        }
        */

        [HttpPost]
        public ActionResult ModifyCII(CInfoCommon info, C2Info c2)
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

            infoCommon.CName = c2.StoreName;
            infoCommon.ModifyDate = DateTime.Now;

            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;

            infoCommon.BirthDay = info.BirthDay;
            infoCommon.PlaceOfBirth = info.PlaceOfBirth;
            infoCommon.AddressInfo = info.AddressInfo;
            infoCommon.Lat = info.Lat;
            infoCommon.Lng = info.Lng;

            infoCommon.BranchCode = info.BranchCode;
            infoCommon.IdentityCard = info.IdentityCard;
            infoCommon.ProvinceName = info.ProvinceName;
            infoCommon.DistrictName = info.DistrictName;
            infoCommon.WardName = info.WardName;
            infoCommon.Country = info.Country;
            infoCommon.AgencyType = info.AgencyType;

            db.Entry(agency).State = System.Data.Entity.EntityState.Modified;
            db.Entry(infoCommon).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("modifycii", "agency", new { id = agency.Id });
        }


        [HttpPost]
        public ActionResult excelAgencyC2(HttpPostedFileBase files)
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
                    List<ImportC2Result> listError = new List<ImportC2Result>();
                    for (int i = 2; i <= totalRows; i++)
                    {
                        string codeSMS = Convert.ToString(sheet.Cells[i, 2].Value);

                        string branch = Convert.ToString(sheet.Cells[i, 3].Value);

                        string staffCode = Convert.ToString(sheet.Cells[i, 6].Value);

                        string phone = Convert.ToString(sheet.Cells[i, 13].Value);

                        string storeName = Convert.ToString(sheet.Cells[i, 4].Value);

                        string deputy = Convert.ToString(sheet.Cells[i, 5].Value);

                        string identityCard = Convert.ToString(sheet.Cells[i, 7].Value);
                        string bussiness = Convert.ToString(sheet.Cells[i, 8].Value);

                        string addressInfo = Convert.ToString(sheet.Cells[i, 9].Value);

                        string province = Convert.ToString(sheet.Cells[i, 11].Value);
                        string provinceCode = Convert.ToString(sheet.Cells[i, 12].Value);

                        string district = Convert.ToString(sheet.Cells[i, 10].Value);
                        string rank = Convert.ToString(sheet.Cells[i, 17].Value);
                        string group = Convert.ToString(sheet.Cells[i, 1].Value);



                        string c1Code1 = Convert.ToString(sheet.Cells[i, 14].Value).Trim();
                        string c1Code2 = Convert.ToString(sheet.Cells[i, 15].Value).Trim();
                        string type = Convert.ToString(sheet.Cells[i, 18].Value).Trim();
                        if (String.IsNullOrEmpty(type))
                        {
                            type = "normal";
                        }
                        else
                        {
                            type = "close";
                        }

                        var cInfo = new CInfoCommon()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CName = storeName,
                            IdentityCard = identityCard,
                            AddressInfo = addressInfo,
                            ProvinceName = province,
                            DistrictName = district,
                            Phone = phone,
                            CreateTime = DateTime.Now,
                            CType = "CII",
                            BranchCode = branch,
                            CDeputy = deputy,
                            Country = "VN",
                            CRank = rank,
                            BusinessLicense = bussiness,
                            AgencyType = type,
                            OldData = 0
                        };


                        var c2 = new C2Info()
                        {
                            Id = Guid.NewGuid().ToString(),
                            InfoId = cInfo.Id,
                            StoreName = storeName,
                            Deputy = deputy,
                            IsActive = 1,
                            OldData = 0,
                            SMSCode = codeSMS,
                            CStatus = 1
                        };

                        string newCode = GetAgencyCode(provinceCode);
                        c2.Code = newCode;
                        cInfo.CCode = newCode;

                        HaiStaff staffInfo = null;
                        if (!String.IsNullOrEmpty(staffCode))
                        {
                            staffInfo = db.HaiStaffs.Where(p => p.Code.Contains(staffCode.Trim())).FirstOrDefault();
                        }

                        if (staffInfo != null)
                        {

                            db.CInfoCommons.Add(cInfo);
                            db.SaveChanges();

                            db.C2Info.Add(c2);
                            db.SaveChanges();

                            int groupChoose = 0;
                            try
                            {
                                groupChoose = Convert.ToInt32(group);
                            }
                            catch { }


                            var staffC2 = new StaffWithC2()
                            {
                                C2Id = c2.Id,
                                StaffId = staffInfo.Id,
                                GroupChoose = groupChoose
                            };
                            db.StaffWithC2.Add(staffC2);
                            db.SaveChanges();

                            // input c1
                            var checkC11 = db.C1Info.Where(p => p.Code == c1Code1).FirstOrDefault();
                            if (checkC11 != null)
                            {
                                var c2c1 = new C2C1()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    C1Code = checkC11.Code,
                                    C2Code = newCode,
                                    ModifyDate = DateTime.Now,
                                    Priority = 1
                                };
                                db.C2C1.Add(c2c1);
                                db.SaveChanges();
                            }

                            var checkC12 = db.C1Info.Where(p => p.Code == c1Code2).FirstOrDefault();
                            if (checkC12 != null)
                            {
                                var c2c1 = new C2C1()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    C1Code = checkC12.Code,
                                    C2Code = newCode,
                                    ModifyDate = DateTime.Now,
                                    Priority = 0
                                };
                                db.C2C1.Add(c2c1);
                                db.SaveChanges();
                            }


                            listError.Add(new ImportC2Result()
                            {
                                old = codeSMS,
                                newCode = newCode,
                                phone = phone,
                                staff = staffCode,
                                name = storeName,
                                deputy = deputy,
                                success = "hoan thanh"
                            });
                        }
                        else
                        {
                            listError.Add(new ImportC2Result()
                            {
                                old = codeSMS,
                                newCode = newCode,
                                phone = phone,
                                staff = staffCode,
                                name = storeName,
                                deputy = deputy,
                                success = "loi"
                            });
                        }


                    }


                    // return list
                    string name = "report" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    string pathTo = Server.MapPath("~/temp/" + name);
                    FileInfo fileExport = new FileInfo(pathTo);
                    if (fileExport.Exists)
                    {
                        fileExport.Delete(); // ensures we create a new workbook
                        fileExport = new FileInfo(pathTo);
                    }
                    try
                    {

                        using (ExcelPackage pakageExport = new ExcelPackage(fileExport))
                        {
                            ExcelWorksheet worksheet = pakageExport.Workbook.Worksheets.Add("abc");
                            worksheet.Cells[1, 1].Value = "Mã khách hàng";
                            worksheet.Cells[1, 2].Value = "Mã mới";
                            worksheet.Cells[1, 3].Value = "Tên cửa hàng";
                            worksheet.Cells[1, 4].Value = "Tên khách hàng";
                            worksheet.Cells[1, 5].Value = "Nhân viên";
                            worksheet.Cells[1, 6].Value = "Số điện thoại";
                            worksheet.Cells[1, 7].Value = "Kết quả";

                            for (int i = 0; i < listError.Count; i++)
                            {
                                worksheet.Cells[i + 2, 1].Value = listError[i].old;
                                worksheet.Cells[i + 2, 2].Value = listError[i].newCode;
                                worksheet.Cells[i + 2, 3].Value = listError[i].name;
                                worksheet.Cells[i + 2, 4].Value = listError[i].deputy;
                                worksheet.Cells[i + 2, 5].Value = listError[i].staff;
                                worksheet.Cells[i + 2, 6].Value = listError[i].phone;
                                worksheet.Cells[i + 2, 7].Value = listError[i].success;
                            }

                            pakageExport.Save();
                        }

                    }
                    catch
                    {
                        return RedirectToAction("error", "home");
                    }
                    return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("report-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));


                }


            }

            return RedirectToAction("managecii", "agency");
        }
        public bool IsPhoneNumber(string number)
        {
            if (number.Length < 10 || number.Length > 11)
                return false;

            string pattern = @"^-*[0-9,\.?\-?\(?\)?\ ]+$";
            return Regex.IsMatch(number, pattern);
        }


        private string GetAgencyCode(string province)
        {
            string type = "C2" + province;

            int? number = db.StoreAgencyIds.Where(p => p.TypeStore == type).Max(p => p.CountNumber);

            if (number == null)
                number = 0;

            number++;
            /*
            var count = Convert.ToString(number).Count();

            var numberAddMore = 100000;

            if (count > 4)
            {
                numberAddMore = 10;
                for (int i = 0; i < count; i++)
                {
                    numberAddMore = numberAddMore * 10;
                }

            }

            number = numberAddMore + number + 1;

            var temp = Convert.ToString(number);
            temp = temp.Substring(1);
            */
       
            var count = Convert.ToString(number).Count();
            var temp = "";
            if (count == 1)
                temp = "000" + number;
            else if (count == 2)
                temp = "00" + number;
            else if (count == 3)
                temp = "0" + number;
            else
                temp = number + "";
         
            string code = province + temp;

            var store = new StoreAgencyId()
            {
                Id = code,
                IsUse = 1,
                CountNumber = number,
                TypeStore = type
            };

            try
            {
                db.StoreAgencyIds.Add(store);
                db.SaveChanges();
            }
            catch
            {
                code = GetAgencyCode(province);
            }

            return code;
        }

        private string GetAgencyCodeTemp(string province)
        {
            string type = "C2Temp";

            int? number = db.StoreAgencyIds.Where(p => p.TypeStore == type).Max(p => p.CountNumber);

            if (number == null)
                number = 0;

            number++;

            var count = Convert.ToString(number).Count();
            var temp = "";
            if (count == 1)
                temp = "000" + number;
            else if (count == 2)
                temp = "00" + number;
            else if (count == 3)
                temp = "0" + number;
            else
                temp = number + "";

            string code = "T" + province + temp;

            var store = new StoreAgencyId()
            {
                Id = code,
                IsUse = 1,
                CountNumber = number,
                TypeStore = type
            };

            try
            {
                db.StoreAgencyIds.Add(store);
                db.SaveChanges();
            }
            catch
            {
                code = GetAgencyCodeTemp(province);
            }

            return code;
        }


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
            if (branch == null)
                return RedirectToAction("error", "home");

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
            if (branch == null)
                return RedirectToAction("error", "home");


            //  DateTime dt = DateTime.ParseExact(birthday, "MM/dd/yyyy HH:mm", null);
            // infoCommon.BirthDay = dt;
            infoCommon.CName = farmer.FarmerName;
            infoCommon.ModifyDate = DateTime.Now;
            infoCommon.Notes = info.Notes;
            infoCommon.Phone = info.Phone;
            infoCommon.Email = info.Email;
            infoCommon.Notes = info.Notes;
            infoCommon.CCode = agency.Code;
            infoCommon.IdentityCard = info.IdentityCard;

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
                                    BranchCode = branch.Code,
                                    CCode = code,
                                    CDeputy = deputy
                                };

                                string ward = Convert.ToString(sheet.Cells[i, 10].Value);


                                var c2 = new C2Info()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    InfoId = cInfo.Id,
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

            var imageAgency = db.SaveAgencyShopImages.Where(p => p.Cinfo == cinfo.Id).ToList();

            foreach (var item in imageAgency)
            {
                db.SaveAgencyShopImages.Remove(item);
            }

            db.SaveChanges();

            db.CInfoCommons.Remove(cinfo);
            db.SaveChanges();

            return RedirectToAction("managecii", "agency");

        }

    }
}