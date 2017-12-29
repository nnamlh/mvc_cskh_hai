using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;
using OfficeOpenXml;
using System.IO;
using SMSUtl;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class ShowInfoController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        //
        // GET: /Staff/
        #region Xem thong in check in
        /// <summary>
        /// Xem thong in check in
        /// </summary>
        /// <param name="search"></param>
        /// <param name="brand"></param>
        /// <param name="page"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        public ActionResult ShowCheckIn(string search = "", string brand = "-1", int? page = 1, string DateFrom = null, string DateTo = null)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowCheckIn", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            DateTime dFrom = DateTime.Now;
            DateTime dTo = DateTime.Now;
            if (DateFrom != null && DateTo != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "MM/dd/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "MM/dd/yyyy", null);
            }

            ViewBag.DateFrom = dFrom.ToString("MM/dd/yyyy");
            ViewBag.DateTo = dTo.ToString("MM/dd/yyyy");
            ViewBag.SearchText = search;

            ViewBag.Branchs = db.HaiBranches.OrderByDescending(p => p.AreaId).ToList();
            ViewBag.IsDisable = 0;
            if(!Utitl.CheckRoleAdmin(db, User.Identity.Name))
            {
                ViewBag.IsDisable = 1;
                var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

                if (staff != null)
                {
                    brand = staff.BranchId;
                }
                else
                {
                    return RedirectToAction("error", "home");
                }
            }


            ViewBag.BranchId = brand;

            if (brand == "-1")
            {
                var allCheckin = (from log in db.StaffCheckIns
                                  where DbFunctions.TruncateTime(log.CreateDate)
                                                     >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateDate)
                                                     <= DbFunctions.TruncateTime(dTo) && (log.HaiStaff.Code.Contains(search) || log.HaiStaff.FullName.Contains(search))
                                  select log).OrderByDescending(p => p.AcceptTime).ToPagedList(pageNumber, pageSize);

                return View(allCheckin);
            }
            else
            {
                var allCheckin = (from log in db.StaffCheckIns
                                  where DbFunctions.TruncateTime(log.CreateDate)
                                                     >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateDate)
                                                     <= DbFunctions.TruncateTime(dTo) && log.HaiStaff.BranchId == brand && (log.HaiStaff.Code.Contains(search) || log.HaiStaff.FullName.Contains(search))
                                  select log).OrderByDescending(p => p.AcceptTime).ToPagedList(pageNumber, pageSize);

                return View(allCheckin);
            }

        }

        [HttpGet]
        public ActionResult ExportCheckIn(string search = "", string brand = "-1", string DateFrom = null, string DateTo = null)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowCheckIn", 0))
                return RedirectToAction("relogin", "home");

            DateTime dFrom = DateTime.Now;
            DateTime dTo = DateTime.Now;
            if (DateFrom != null && DateTo != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "MM/dd/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "MM/dd/yyyy", null);
            }

            ViewBag.DateFrom = dFrom.ToString("MM/dd/yyyy");
            ViewBag.DateTo = dTo.ToString("MM/dd/yyyy");
  
            if (!Utitl.CheckRoleAdmin(db, User.Identity.Name))
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();

                if (staff != null)
                {
                    brand = staff.BranchId;
                }
                else
                {
                    return RedirectToAction("error", "home");
                }
            }

            List<StaffCheckIn> allCheckin;

            if (brand == "-1")
            {
                allCheckin = (from log in db.StaffCheckIns
                              where DbFunctions.TruncateTime(log.CreateDate)
                                                 >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateDate)
                                                 <= DbFunctions.TruncateTime(dTo) && (log.HaiStaff.Code.Contains(search) || log.HaiStaff.FullName.Contains(search))
                              select log).OrderByDescending(p => p.AcceptTime).ToList();
            }
            else
            {
                allCheckin = (from log in db.StaffCheckIns
                              where DbFunctions.TruncateTime(log.CreateDate)
                                                 >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateDate)
                                                 <= DbFunctions.TruncateTime(dTo) && log.HaiStaff.BranchId == brand && (log.HaiStaff.Code.Contains(search) || log.HaiStaff.FullName.Contains(search))
                              select log).OrderByDescending(p => p.AcceptTime).ToList();
            }


            string pathTo = Server.MapPath("~/temp/checkin_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xlsx");

            try
            {
                FileInfo newFile = new FileInfo(pathTo);
                if (newFile.Exists)
                {
                    newFile.Delete(); // ensures we create a new workbook
                    newFile = new FileInfo(pathTo);
                }

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("pms");

                    worksheet.Cells[1, 1].Value = "MANV";
                    worksheet.Cells[1, 2].Value = "TEN NHAN VIEN";
                    worksheet.Cells[1, 3].Value = "TAI KHOAN";
                    worksheet.Cells[1, 4].Value = "THOI GIAN";
                    worksheet.Cells[1, 5].Value = "TOA DO";
                    worksheet.Cells[1, 6].Value = "NOI DUNG";
                    worksheet.Cells[1, 7].Value = "DIA CHI";
                    worksheet.Cells[1, 8].Value = "MA DAI LY";
                    worksheet.Cells[1, 9].Value = "TEN DAI LY";
                    worksheet.Cells[1, 10].Value = "LOAI DAI LY";


                    for (int i = 0; i < allCheckin.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = allCheckin[i].HaiStaff.Code;
                            worksheet.Cells[i + 2, 2].Value = allCheckin[i].HaiStaff.FullName;
                            worksheet.Cells[i + 2, 3].Value = allCheckin[i].HaiStaff.UserLogin;
                            if (allCheckin[i].AcceptTime != null)
                                worksheet.Cells[i + 2, 4].Value = allCheckin[i].AcceptTime.Value.ToString("dd/MM/yyyy HH:mm");
                            else
                                worksheet.Cells[i + 2, 4].Value = allCheckin[i].CreateDate.Value.ToString("dd/MM/yyyy HH:mm");


                            worksheet.Cells[i + 2, 5].Value = allCheckin[i].Latitude + "," + allCheckin[i].Longitude;
                            worksheet.Cells[i + 2, 6].Value = allCheckin[i].Comment;
                            worksheet.Cells[i + 2, 7].Value = allCheckin[i].AgencyAddress;
                            worksheet.Cells[i + 2, 8].Value = allCheckin[i].Agency;
                            worksheet.Cells[i + 2, 9].Value = allCheckin[i].AgencyName;
                            worksheet.Cells[i + 2, 10].Value = allCheckin[i].AgencyTypeName;
                        }
                        catch
                        {
                            return RedirectToAction("error", "home");
                        }

                    }

                    package.Save();

                }

            }
            catch
            {
                return RedirectToAction("error", "home");
            }


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("thong-tin-check-in-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));
        }

        #endregion

        #region thac mac tu khach hang
        /// <summary>
        /// thac mac tu khach hang
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerMessenge()
        {
            return View();
        }
        #endregion 


        /*
        #region  xem nhan vien
        ///
        /// xem nhan vien
        ///
        // show nhan vien
        public ActionResult ShowStaff(int? page, string branchId = "-1", string search = "")
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowStaff", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.AllBranch = db.HaiBranches.ToList();

            ViewBag.SearchText = search;
          
            ViewBag.IsDisable = 0;

            if (!Utitl.CheckRoleAdmin(db, User.Identity.Name))
            {
                ViewBag.IsDisable = 1;
                var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name && p.IsDelete != 1).FirstOrDefault();

                if (staff != null)
                {
                    branchId = staff.BranchId;
                }
                else
                {
                    return RedirectToAction("error", "home");
                }
            }

            ViewBag.BranchId = branchId;

            if (branchId == "-1")
            {
                return View(db.HaiStaffs.Where(p => p.IsDelete != 1 && (p.Code.Contains(search) || p.FullName.Contains(search))).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return View(db.HaiStaffs.Where(p => p.BranchId == branchId && p.IsDelete != 1 && (p.Code.Contains(search) || p.FullName.Contains(search))).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));

            }

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
                Name = "Tìm theo tên/mã",
                Value = 5
            });
            return arr;
        }
        #endregion


        // chi nhánh
        public ActionResult ShowBranch(int? page, string areaId, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowBranch", 0))
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

            if (areaId == "-1")
            {
                return View(db.HaiBranches.Where(p => p.Name.Contains(search) || p.Code.Contains(search)).OrderByDescending(p => p.AreaId).ToPagedList(pageNumber, pageSize));
            }

            return View(db.HaiBranches.Where(p => p.AreaId == areaId && (p.Name.Contains(search) || p.Code.Contains(search))).OrderByDescending(p => p.Name).ToPagedList(pageNumber, pageSize));
        }

        // sản phẩm
        public ActionResult ShowProduct(int? page, string search, int? type)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowProduct", 0))
                return RedirectToAction("relogin", "home");

            if (search == null)
                search = "";

            if (type == null)
                type = 1;

            ViewBag.SearchText = search;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.SType = type;

            switch (type)
            {
                case 2:
                    ViewBag.STypeName = "Nhà sản xuất";
                    return View(db.ProductInfoes.Where(p => p.Producer.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
                case 1:
                    ViewBag.STypeName = "Tên sản phẩm";
                    return View(db.ProductInfoes.Where(p => p.PName.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
                case 3:
                    ViewBag.STypeName = "Nhóm sản phẩm";
                    return View(db.ProductInfoes.Where(p => p.GroupId.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
                case 4:
                    ViewBag.STypeName = "Mã sản phẩm";
                    return View(db.ProductInfoes.Where(p => p.PCode.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
            }

            return View(db.ProductInfoes.Where(p => p.PName.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
        }

        #region xem dai ly cap 2
        // xem thông tin đại lý
        public ActionResult ShowCII(int? page, string search = "", int? type = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowAgency", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.SearchText = search;
            ViewBag.SType = type;

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();
            if (staff == null)
                return RedirectToAction("error", "home");


            var roleShow = Utitl.CheckRoleShowInfo(db, User.Identity.Name);

            if (roleShow == 1)
            {
                // xem toàn bộ
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
                        return View(db.C2Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return RedirectToAction("error", "home");
                }
            }
            else if (roleShow == 2)
            {
                // xem toàn bộ
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C2Info.Where(p => p.Code.Contains(search) && p.CInfoCommon.BranchCode == staff.HaiBranch.Code).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C2Info.Where(p => p.CInfoCommon.Phone.Contains(search) && p.CInfoCommon.BranchCode == staff.HaiBranch.Code).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(db.C2Info.Where(p => p.StoreName.Contains(search) && p.CInfoCommon.BranchCode == staff.HaiBranch.Code).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã cấp 1";
                        return View(db.C2Info.Where(p => p.CInfoCommon.BranchCode == staff.HaiBranch.Code).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return RedirectToAction("error", "home");
                }
            }
            else
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(staff.StaffWithC2.Select(p => p.C2Info).Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(staff.StaffWithC2.Select(p => p.C2Info).Where(p => p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(staff.StaffWithC2.Select(p => p.C2Info).Where(p => p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã cấp 1";
                        return View(staff.StaffWithC2.Select(p => p.C2Info).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return RedirectToAction("error", "home");
                }
            }

          

        }
        #endregion


        #region xem cap 1
        // xem thông tin đại lý 1
        public ActionResult ShowCI(int? page, string search = "", int? type = 1)
        {
            // type 

            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowAgency", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.SearchText = search;
            ViewBag.SType = type;

            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();
            if (staff == null)
                return RedirectToAction("error", "home");

            var roleShow = Utitl.CheckRoleShowInfo(db, User.Identity.Name);
            if (roleShow == 1)
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C1Info.Where(p => p.Code.Contains(search) ).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
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
            else if (roleShow == 2)
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(db.C1Info.Where(p => p.Code.Contains(search) ).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
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
            else
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã đại lý";
                        return View(staff.C1Info.Where(p => p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.C1Info.Where(p => p.CInfoCommon.Phone.Contains(search) ).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên cửa hàng";
                        return View(staff.C1Info.Where(p => p.StoreName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    default:
                        return View(staff.C1Info.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                }
            }

           
        }
        #endregion


        #region xem nong dan
        // nông dân
        public ActionResult ShowFarmer(int? page, string areaId = "-1", string search = "", int? type = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowAgency", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.AreaId = areaId;
            ViewBag.AllArea = db.HaiAreas.ToList();

            ViewBag.SearchText = search;
            ViewBag.SType = type;
            var staff = db.HaiStaffs.Where(p => p.UserLogin == User.Identity.Name).FirstOrDefault();
            if (staff == null)
                return RedirectToAction("error", "home");

            if (areaId != "-1")
            {
                switch (type)
                {
                    case 1:
                        ViewBag.STypeName = "Mã khách hàng";
                        return View(db.FarmerInfoes.Where(p =>  p.Code.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 2:
                        ViewBag.STypeName = "Số điện thoại";
                        return View(db.FarmerInfoes.Where(p =>  p.CInfoCommon.Phone.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 3:
                        ViewBag.STypeName = "Tên khách hàng";
                        return View(db.FarmerInfoes.Where(p => p.FarmerName.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
                    case 4:
                        ViewBag.STypeName = "Mã chi nhánh";
                        return View(db.FarmerInfoes.Where(p =>  p.CInfoCommon.BranchCode.Contains(search)).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
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

        #endregion

        private List<FuncShow> prepareFuncShowC2()
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
                Name = "Theo cấp 1",
                Value = 2
            });
            arr.Add(new FuncShow()
            {
                Name = "Tìm theo tên",
                Value = 3
            });
            return arr;
        }

        private List<FuncShow> prepareFuncShowC1()
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
                Name = "Tìm theo tên",
                Value = 3
            });
            return arr;
        }

        private List<FuncShow> prepareFuncShowFarmer()
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
                Name = "Theo đại lý cấp 2",
                Value = 2
            });
            arr.Add(new FuncShow()
            {
                Name = "Tìm theo tên",
                Value = 3
            });
            return arr;
        }


        // xem chuuong trinh khuyen mai
        public ActionResult ShowEvent(int? page, string search = "", int type = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowEvent", 0))
                return RedirectToAction("relogin", "home");

            if (search == null)
                search = "";

            ViewBag.Type = type;


            ViewBag.SearchText = search;
            int pageSize = 10;
            int pageNumber = (page ?? 1);


            switch (type)
            {
                case 1:
                    return View(db.EventAreas.Where(p => p.EventInfo.ESTT == 1 && (p.HaiArea.Code == search || p.HaiArea.Name == search)).Select(p => p.EventInfo).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
                case 2:
                    var cinfo = db.CInfoCommons.Where(p => p.CCode == search || p.Phone == search).FirstOrDefault();
                    if (cinfo == null)
                        return View((new List<EventInfo>()).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
                    else
                    {
                        var allArea = db.EventAreas.Where(p => p.EventInfo.ESTT == 1 ).ToList();
                        List<EventInfo> eventInfoes = new List<EventInfo>();
                        foreach (var item in allArea)
                        {
                            if (cinfo.CType == "CII")
                            {
                                var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId).ToList();

                                if (cusJoin.Count() > 0)
                                {
                                    var cJoin = cusJoin.Where(p => p.CInfoId == cinfo.Id).FirstOrDefault();
                                    if (cJoin != null)
                                        eventInfoes.Add(item.EventInfo);
                                }
                                else
                                    eventInfoes.Add(item.EventInfo);
                            }
                            else if (cinfo.CType == "FARMER")
                            {
                                var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId ).ToList();

                                if (cusJoin.Count() > 0)
                                {
                                    var cJoin = cusJoin.Where(p => p.CInfoId == cinfo.Id).FirstOrDefault();
                                    if (cJoin != null)
                                        eventInfoes.Add(item.EventInfo);
                                }
                                else
                                    eventInfoes.Add(item.EventInfo);
                            }
                        }
                        return View(eventInfoes.OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
                    }
                case 3:
                    return View(db.EventProducts.Where(p => p.EventInfo.ESTT == 1 && (p.ProductInfo.Barcode == search || p.ProductInfo.PCode == search)).Select(p => p.EventInfo).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
                case 4:
                    return View(db.EventInfoes.Where(p => p.ESTT == 1 && p.Name.Contains(search)).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
            }

            return View(db.EventInfoes.Where(p => p.ESTT == 1 && p.Name.Contains(search)).OrderByDescending(p => p.EndTime).ToPagedList(pageNumber, pageSize));
        }


        public ActionResult ShowEventDetail(string id)
        {
            return PartialView(db.EventInfoes.Find(id));
        }



        public ActionResult ShowMsg(int? page, string DateFrom, string DateTo)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowMsg", 0))
                return RedirectToAction("relogin", "home");

            DateTime dFrom = DateTime.Now;
            DateTime dTo = DateTime.Now;
            if (DateFrom != null && DateTo != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }
            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var allMsg = (from log in db.MessegeToHais
                          where DbFunctions.TruncateTime(log.CreateTime)
                                             >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateTime)
                                             <= DbFunctions.TruncateTime(dTo)
                          select log).OrderByDescending(p => p.UserLogin).ToPagedList(pageNumber, pageSize);

            return View(allMsg);
        }
        */
        public ActionResult UserInfo(string user)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "UserInfo", 0))
                return RedirectToAction("relogin", "home");


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
              //  if (cInfo.BirthDay != null)
                    result.birthday = "";
                result.user = user;
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



            return View(result);
        }

        public ActionResult AcceptMsg(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AcceptMsg", 0))
                return RedirectToAction("relogin", "home");

            var check = db.MessegeToHais.Find(id);
            if (check == null)
                return RedirectToAction("error", "home");

            check.IsSeen = 1;
            check.UserSeen = User.Identity.Name;
            db.Entry(check).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("showmsg", "showinfo");

        }


        public ActionResult SMSHistory(string search = "", int? page = 1, string DateFrom = null, string DateTo = null)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "SMSHistory", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            DateTime dFrom = DateTime.Now;
            DateTime dTo = DateTime.Now;
            if (DateFrom != null && DateTo != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "MM/dd/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "MM/dd/yyyy", null);
            }

            ViewBag.DateFrom = dFrom.ToString("MM/dd/yyyy");
            ViewBag.DateTo = dTo.ToString("MM/dd/yyyy");
            ViewBag.SearchText = search;

            var allMsg = (from log in db.SMSHistories
                          where DbFunctions.TruncateTime(log.CreateTime)
                                             >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.CreateTime)
                                             <= DbFunctions.TruncateTime(dTo) && log.PhoneNumber.Contains(search)
                          select log).OrderByDescending(p => p.CreateTime).ToPagedList(pageNumber, pageSize);

            return View(allMsg);
        }

        public ActionResult HappyBirthday(int? page = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "HappyBirthday", 0))
                return RedirectToAction("relogin", "home");

           
            return View();
        }


        [HttpPost]
        public ActionResult SendHappyBirthday(string id, string content)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "HappyBirthday", 1))
                return RedirectToAction("relogin", "home");

            var check = db.CInfoCommons.Find(id);

            var account = db.SmsAccounts.Find(1);
            string Msg = string.Empty;


            if (check != null && account != null)
            {

                SMScore _smsCore = new SMScore(account.BrandName, account.UserName, account.Pass);
                _smsCore.IPserver = account.AddressSend;
                _smsCore.Port = Convert.ToInt32(account.PortSend);
                _smsCore.SendMethod = account.Method;


                var result = _smsCore.SendSMS(content, check.Phone, ref Msg);
                if (result)
                {
                    var history = new SendSmsHistory()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Phone = check.Phone,
                        Messenge = content,
                        UserSend = User.Identity.Name,
                        CreateTime = DateTime.Now,
                        StatusSend = "Đã gửi thành công"
                    };
                    db.SendSmsHistories.Add(history);
                    db.SaveChanges();

                    var save = new HappyBirthday()
                    {

                        Id = Guid.NewGuid().ToString(),
                        Content = content,
                        CInfoId = id,
                        CreateDate = DateTime.Now
                    };

                    db.HappyBirthdays.Add(save);
                    db.SaveChanges();


                }
                else
                {
                    var history = new SendSmsHistory()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Phone = check.Phone,
                        Messenge = content,
                        UserSend = User.Identity.Name,
                        CreateTime = DateTime.Now,
                        StatusSend = Msg
                    };
                    db.SendSmsHistories.Add(history);
                    db.SaveChanges();



                }
            }




            return RedirectToAction("HappyBirthday", "ShowInfo");
        }




        ///
       

    }
}