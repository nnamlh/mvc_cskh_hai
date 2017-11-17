using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using System.Data.Entity;
using PagedList;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{

    [Authorize(Roles = "Administrator, User")]
    public class EventManageController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        public ActionResult ManageEvent(string DateFrom = null, string DateTo = null, int ChooseFunc = 1, int? page = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.ChooseId = ChooseFunc;
            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;
            if (DateFrom != null)
            {
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);
            }

            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;
            ViewBag.EventAward = db.AwardInfoes.ToList();
            ViewBag.EventProduct = db.ProductInfoes.ToList();
            var listArea = db.HaiAreas.ToList();
            ViewBag.EventArea = listArea;
            var dateNow = DateTime.Now;


            // stop cac chuong trinh het han
            var allEventStop = (from log in db.EventInfoes
                            where  DbFunctions.TruncateTime(log.EndTime)
                                               <= DbFunctions.TruncateTime(dateNow) && log.ESTT == 1
                            select log).ToList();

            foreach (var item in allEventStop)
            {
                item.ESTT = 2;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }


            
            if (ChooseFunc != 2)
            {
               
                var bla = (from log in db.EventInfoes
                           where log.ESTT == ChooseFunc
                           select log).ToList();
                return View(bla.OrderByDescending(p => p.CreateTime).ToPagedList(pageNumber, pageSize));
            }
            

            var allEvent = (from log in db.EventInfoes
                            where DbFunctions.TruncateTime(log.BeginTime)
                                               >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.BeginTime)
                                               <= DbFunctions.TruncateTime(dTo) && log.ESTT == 2
                            select log).ToList();

            return View(allEvent.OrderByDescending(p => p.CreateTime).ToPagedList(pageNumber, pageSize));
        }



        public ActionResult jsonChooseCII(string id)
        {
            var listArea = db.C2Info.Select(p => new { Id = p.InfoId, Name = p.StoreName, Deputy = p.Deputy }).ToList();
       
            return Json(listArea, JsonRequestBehavior.AllowGet);
        }

        public ActionResult jsonChooseFarmer(string id)
        {
            var listArea = db.FarmerInfoes.Select(p => new { Id = p.InfoId, Name = p.FarmerName }).ToList();

            return Json(listArea, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateEvent(EventInfo info, string BeginTime, string EndTime, HttpPostedFileBase files, List<string> Awards, List<string> ProductChoose, HttpPostedFileBase products, List<int> EventAreaCII, List<int> EventAreaFarmer, HttpPostedFileBase ciijoin, HttpPostedFileBase farmerjoin)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            info.BeginTime = DateTime.ParseExact(BeginTime, "dd/MM/yyyy", null);
            info.EndTime = DateTime.ParseExact(EndTime, "dd/MM/yyyy", null);

            string urlThumbnail = "";
            if (files != null)
            {
                try
                {
                    string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                    string fsave = "~/haiupload/event/" + dfolder;

                    bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                    MemoryStream target = new MemoryStream();
                    files.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Height = 160,
                        isSacle = true,
                        UploadPath = fsave
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, ".png");

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/haiupload/event/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {
                    urlThumbnail = "/haiupload/defaultevent.png";
                }
            }
            else
            {
                urlThumbnail = "/haiupload/defaultevent.png";
            }

            info.Thumbnail = urlThumbnail;
            info.Id = Guid.NewGuid().ToString();
            info.ESTT = 0;
            info.UserCretea = User.Identity.Name;
            info.CreateTime = DateTime.Now;
            db.EventInfoes.Add(info);
            db.SaveChanges();


            // phan thuong
            if (Awards != null)
            {
                foreach (var award in Awards)
                {
                    var data = db.AwardInfoes.Find(award);
                    if (data != null)
                    {
                        info.AwardInfoes.Add(data);

                    }
                }
                db.SaveChanges();
            }


            // lisst san pham
            if (ProductChoose != null && ProductChoose.Count > 0)
            {
                foreach (var item in ProductChoose)
                {
                    var checkProduct = db.ProductInfoes.Find(item);

                    if (checkProduct != null)
                    {

                        var check = db.EventProducts.Where(p => p.EventId == info.Id && p.ProductId == item).FirstOrDefault();

                        if (check == null)
                        {

                            var eventProduct = new EventProduct()
                            {
                                EventId = info.Id,
                                ProductId = checkProduct.Id

                            };
                            try
                            {
                                eventProduct.Point = Convert.ToInt32(checkProduct.CardPoint);
                            }
                            catch
                            {
                                eventProduct.Point = 0;
                            }



                            db.EventProducts.Add(eventProduct);
                        }
                    }
                }
            }


            if (products != null && products.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(products.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "eventproduct_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    products.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string pCode = Convert.ToString(sheet.Cells[i, 1].Value);

                        string pName = Convert.ToString(sheet.Cells[i, 2].Value);

                        int point = Convert.ToInt32(sheet.Cells[i, 3].Value);

                        var checkProduct = db.ProductInfoes.Where(p => p.PCode == pCode).FirstOrDefault();

                        if (checkProduct != null)
                        {
                            var check = db.EventProducts.Where(p => p.EventId == info.Id && p.ProductId == checkProduct.Id).FirstOrDefault();

                            if (check == null)
                            {
                                var eventProduct = new EventProduct()
                                {
                                    EventId = info.Id,
                                    ProductId = checkProduct.Id,
                                    Point = point
                                };
                                db.EventProducts.Add(eventProduct);
                            }
                        }

                    }
                    db.SaveChanges();
                }
            }


            // phan dai ly
            if (EventAreaCII != null)
            {
                foreach (var item in EventAreaCII)
                {
                    var area = db.HaiAreas.Find(item);
                    if (area != null)
                    {

                        var checkAreaEvent = db.EventAreas.Where(p => p.EventId == info.Id && p.AreaId == area.Id).FirstOrDefault();

                        if (checkAreaEvent == null)
                        {
                            var areaEvent = new EventArea()
                            {
                                EventId = info.Id,
                                AreaId = area.Id,
                                AllAgency = 1
                            };
                            db.EventAreas.Add(areaEvent);
                        }
                    }
                }
                db.SaveChanges();
            }

            if (ciijoin != null && ciijoin.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(ciijoin.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "ciijoin_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    ciijoin.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string cCode = Convert.ToString(sheet.Cells[i, 1].Value);

                        var checkCII = db.C2Info.Where(p => p.Code == cCode).FirstOrDefault();
                        if (checkCII != null)
                        {
                            // kiem tra khu vu cua dai ly nay da add chua, chua thi add
                            var checkAreaEvent = db.EventAreas.Where(p => p.EventId == info.Id).FirstOrDefault();

                            if (checkAreaEvent == null)
                            {
                                var areaEvent = new EventArea()
                                {
                                    EventId = info.Id,
                                    AllAgency = 1
                                };
                                db.EventAreas.Add(areaEvent);
                                db.SaveChanges();
                            }

                            // them dai ly vao danh sach chuong trinh
                            var checkECustomer = db.EventCustomers.Where(p => p.EventId == info.Id && p.CInfoId == checkCII.InfoId).FirstOrDefault();
                            if (checkECustomer == null)
                            {
                                var cEvent = new EventCustomer()
                                {
                                    CInfoId = checkCII.InfoId,
                                    EventId = info.Id,
                                    IsJoint = 1
                                };
                                db.EventCustomers.Add(cEvent);
                                db.SaveChanges();
                            }
                        }

                    }
                    
                }
            }

            // phan nong dan
            if (EventAreaFarmer != null)
            {
                foreach (var item in EventAreaFarmer)
                {
                    var area = db.HaiAreas.Find(item);
                    if (area != null)
                    {

                        var checkAreaFarmer = db.EventAreaFarmers.Where(p => p.EventId == info.Id && p.AreaId == area.Id).FirstOrDefault();

                        if (checkAreaFarmer == null)
                        {
                            var areaEvent = new EventAreaFarmer()
                            {
                                EventId = info.Id,
                                AreaId = area.Id,
                                AllAgency = 1
                            };
                            db.EventAreaFarmers.Add(areaEvent);
                        }
                    }
                }
                db.SaveChanges();
            }

            // nong dan duoc tham gia

            if (farmerjoin != null && farmerjoin.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(farmerjoin.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "farmerjoin_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    farmerjoin.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string phone = Convert.ToString(sheet.Cells[i, 1].Value);

                        var cInfo = db.CInfoCommons.Where(p => p.Phone == phone && p.CType == "FARMER").FirstOrDefault();
                        if (cInfo != null)
                        {

                            var checkAreaFarmer = db.EventAreaFarmers.Where(p => p.EventId == info.Id).FirstOrDefault();

                            if (checkAreaFarmer == null)
                            {
                                var areaEvent = new EventAreaFarmer()
                                {
                                    EventId = info.Id,
                                    AllAgency = 1
                                };
                                db.EventAreaFarmers.Add(areaEvent);
                                db.SaveChanges();
                            }



                            var checkECustomer = db.EventCustomerFarmers.Where(p => p.EventId == info.Id && p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (checkECustomer == null)
                            {
                                var cEvent = new EventCustomerFarmer()
                                {
                                    CInfoId = cInfo.Id,
                                    EventId = info.Id,
                                    IsJoint = 1
                                };

                                db.EventCustomerFarmers.Add(cEvent);
                                db.SaveChanges();
                            }
                        }

                    }
                    
                }
            }

            return RedirectToAction("manageevent", "eventmanage");
        }


        public ActionResult ModifyEvent(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            var checkEvent = db.EventInfoes.Find(id);

            if (checkEvent != null)
            {
                ViewBag.EventArea = db.HaiAreas.ToList();
                ViewBag.EventAward = db.AwardInfoes.ToList();
                ViewBag.EventProduct = db.ProductInfoes.ToList();
                return View(checkEvent);
            }

            return RedirectToAction("error", "home");
        }

        [HttpPost]
        public ActionResult RemoveEvent(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            var checkEvent = db.EventInfoes.Find(id);

            if (checkEvent != null && checkEvent.ESTT == 0)
            {
                // xóa khu vuc
                var eventArea = db.EventAreas.Where(p => p.EventId == id).ToList();

                foreach (var item in eventArea)
                {
                    db.EventAreas.Remove(item);
                }
                db.SaveChanges();


                var eventfarmer = db.EventAreaFarmers.Where(p => p.EventId == id).ToList();
                foreach (var item in eventfarmer)
                {
                    db.EventAreaFarmers.Remove(item);
                }
                db.SaveChanges();


                // xoa khach hang
                var eventCustomer = db.EventCustomers.Where(p => p.EventId == id).ToList();
                foreach (var item in eventCustomer)
                {
                    db.EventCustomers.Remove(item);
                }
                db.SaveChanges();

                var eventCustomerFarmer = db.EventCustomerFarmers.Where(p => p.EventId == id).ToList();
                foreach (var item in eventCustomerFarmer)
                {
                    db.EventCustomerFarmers.Remove(item);
                }
                db.SaveChanges();


                // xoa phan thuong
                var eventAward = checkEvent.AwardInfoes.ToList();

                foreach (var item in eventAward)
                {
                    checkEvent.AwardInfoes.Remove(item);
                }
                db.SaveChanges();

                // san pham
                var eventProduct = db.EventProducts.Where(p => p.EventId == id).ToList();
                foreach (var item in eventProduct)
                {
                    db.EventProducts.Remove(item);
                }
                db.SaveChanges();

                db.EventInfoes.Remove(checkEvent);
                db.SaveChanges();

                return RedirectToAction("manageevent", "eventmanage");

            }

            return RedirectToAction("error", "home");
        }


        public ActionResult StopEvent(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");


            var eventInfo = db.EventInfoes.Find(id);
            if (eventInfo == null)
            {
                return RedirectToAction("error", "home");
            }

            if (eventInfo.ESTT == 1)
            {
                eventInfo.ESTT = 3;
                db.Entry(eventInfo).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("modifyevent", "eventmanage", new { id = id });

        }

        public ActionResult ChangeState(int stt, string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");


            var eventInfo = db.EventInfoes.Find(id);
            if (eventInfo == null)
            {
                return RedirectToAction("error", "home");
            }

            switch (stt)
            {
                case 1:
                    if (eventInfo.ESTT == 0 || eventInfo.ESTT == 3)
                    {
                        eventInfo.ESTT = 1;
                        db.Entry(eventInfo).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;
                case 2:
                    if (eventInfo.ESTT == 1)
                    {
                        eventInfo.ESTT = 2;
                        db.Entry(eventInfo).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;
                case 3:
                    if (eventInfo.ESTT == 1)
                    {
                        eventInfo.ESTT = 3;
                        db.Entry(eventInfo).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;

            }

            return RedirectToAction("modifyevent", "eventmanage", new { id = id });

        }

        // 

        [HttpPost]
        public ActionResult ModifyEvent(EventInfo info, string BeginTime, string EndTime, HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            var checkEvent = db.EventInfoes.Find(info.Id);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }

            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return RedirectToAction("modifyevent", "eventmanage", new { id = info.Id });
            }

            checkEvent.Name = info.Name;
            checkEvent.Descibe = info.Descibe;
            string urlThumbnail = checkEvent.Thumbnail;

            checkEvent.BeginTime = DateTime.ParseExact(BeginTime, "MM/dd/yyyy", null);
            checkEvent.EndTime = DateTime.ParseExact(EndTime, "MM/dd/yyyy", null);

            if (files != null)
            {
                try
                {
                    string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                    string fsave = "~/haiupload/event/" + dfolder;

                    bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                    MemoryStream target = new MemoryStream();
                    files.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Height = 160,
                        isSacle = true,
                        UploadPath = fsave
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, ".png");

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/haiupload/event/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {

                }

            }

            checkEvent.Thumbnail = urlThumbnail;

            checkEvent.ModifyTime = DateTime.Now;

            db.Entry(checkEvent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("modifyevent", "eventmanage", new { id = info.Id });
        }

        public ActionResult RemoveEventProduct(string eventId, string productId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền thực thi." }, JsonRequestBehavior.AllowGet);

            var checkEvent = db.EventProducts.Where(p => p.EventId == eventId && p.ProductId == productId).FirstOrDefault();

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.EventInfo.ESTT != 0 && checkEvent.EventInfo.ESTT != 3)
            {
                return Json(new { error = 1, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }
            db.EventProducts.Remove(checkEvent);
            db.SaveChanges();

            return Json(new { error = 1, msg = "Đã xóa sản phẩm." }, JsonRequestBehavior.AllowGet);
        }


        // phan qua
        public ActionResult AddEventAward(string eventId, string awardId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return Json(new { error = 1, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }
            var awards = db.AwardInfoes.Find(awardId);
            if (awards == null)
            {
                return RedirectToAction("error", "home");
            }


            var find = awards.EventInfoes.Where(p => p.Id == eventId).FirstOrDefault();

            if (find != null)
                return Json(new { error = 0, msg = "Đã có phần quà này trong danh sách." }, JsonRequestBehavior.AllowGet);

            awards.EventInfoes.Add(checkEvent);
            db.SaveChanges();

            return Json(new { error = 1, msg = "Đã thêm phần quà.", name = awards.Name, point = awards.Point, image = awards.Thumbnail, id = awards.Id }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RemoveEventAward(string eventId, string awardId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);


            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return Json(new { error = 1, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }
            var awards = db.AwardInfoes.Find(awardId);
            if (awards == null)
            {
                return RedirectToAction("error", "home");
            }


            var find = awards.EventInfoes.Where(p => p.Id == eventId).FirstOrDefault();

            if (find == null)
                return Json(new { error = 0, msg = "Chưa có phần quà này trong danh sách." }, JsonRequestBehavior.AllowGet);

            awards.EventInfoes.Remove(checkEvent);
            db.SaveChanges();

            return Json(new { error = 1, msg = "Đã xóa phần quà." }, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public ActionResult excelEventProduct(HttpPostedFileBase products, string eventId, List<string> ProductChoose)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return RedirectToAction("modifyevent", "eventmanage", new { id = eventId });
            }


            if (ProductChoose != null && ProductChoose.Count > 0)
            {
                foreach (var item in ProductChoose)
                {
                    var checkProduct = db.ProductInfoes.Find(item);

                    if (checkProduct != null)
                    {

                        var check = db.EventProducts.Where(p => p.EventId == checkEvent.Id && p.ProductId == item).FirstOrDefault();

                        if (check == null)
                        {
                            try
                            {
                                var eventProduct = new EventProduct()
                                {
                                    EventId = checkEvent.Id,
                                    ProductId = checkProduct.Id,
                                    Point = Convert.ToInt32(checkProduct.CardPoint)
                                };
                                db.EventProducts.Add(eventProduct);
                            }
                            catch
                            {

                            }
                        }


                    }
                }

                db.SaveChanges();
            }




            // lisst san pham
            if (products != null && products.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(products.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "eventproduct_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    products.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string pCode = Convert.ToString(sheet.Cells[i, 1].Value);

                        string pName = Convert.ToString(sheet.Cells[i, 2].Value);

                        int point = Convert.ToInt32(sheet.Cells[i, 3].Value);

                        var checkProduct = db.ProductInfoes.Where(p => p.PCode == pCode).FirstOrDefault();

                        if (checkProduct != null)
                        {
                            var check = db.EventProducts.Where(p => p.EventId == checkEvent.Id && p.ProductId == checkProduct.Id).FirstOrDefault();

                            if (check == null)
                            {
                                var eventProduct = new EventProduct()
                                {
                                    EventId = eventId,
                                    ProductId = checkProduct.Id,
                                    Point = point
                                };
                                db.EventProducts.Add(eventProduct);
                            }
                        }

                    }
                    db.SaveChanges();
                }
            }

            return RedirectToAction("modifyevent", "eventmanage", new { id = eventId });
        }



        public ActionResult AddEventArea(string eventId, string areaId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return Json(new { error = 0, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }
            var haiArea = db.HaiAreas.Find(areaId);
            if (haiArea == null)
            {
                return RedirectToAction("error", "home");
            }

            var find = db.EventAreas.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find != null)
                return Json(new { error = 0, msg = "Đã có khu vực này trong danh sách." }, JsonRequestBehavior.AllowGet);


            var eventArea = new EventArea()
            {
                AreaId = areaId,
                EventId = eventId,
                AllAgency = 1
            };

            db.EventAreas.Add(eventArea);
            db.SaveChanges();

            return Json(new { error = 1, msg = "Đã thêm khu vực.", name = haiArea.Name, code = haiArea.Code, notes = haiArea.Notes, id = haiArea.Id }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DowloadAreaCustomerEvent(string eventId, int type)
        {
            // type = 1: lay danh sach dai ly
            // type = 2: lay nong dan

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            if (type == 1)
            {
                var data = db.EventCustomers.Where(p => p.EventId == eventId).ToList();
                string pathRoot = Server.MapPath("~/haiupload/areacustomerevent.xlsx");
                string name = "areacustomer" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                string pathTo = Server.MapPath("~/temp/" + name);

                System.IO.File.Copy(pathRoot, pathTo);

                try 
                {
                    FileInfo newFile = new FileInfo(pathTo);

                    using (ExcelPackage package = new ExcelPackage(newFile))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                        for (int i = 0; i < data.Count; i++)
                        {

                            try
                            {
                                worksheet.Cells[i + 2, 1].Value = i + 1;
                                worksheet.Cells[i + 2, 2].Value = data[i].CInfoCommon.CCode;
                                worksheet.Cells[i + 2, 3].Value = data[i].CInfoCommon.Phone;
                                worksheet.Cells[i + 2, 4].Value = data[i].CInfoCommon.CDeputy;
                                worksheet.Cells[i + 2, 5].Value = data[i].CInfoCommon.CName;

                                worksheet.Cells[i + 2, 7].Value = "CẤP 2";
                            }
                            catch
                            {
                                return RedirectToAction("error", "home");
                            }

                        }

                        package.Save();

                        return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-dai-ly-cap2-tham-gia-khuyen-mai-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

                    }
                }
                catch
                {
                    return RedirectToAction("error", "home");
                }
            }
            else
            {
                var data = db.EventCustomerFarmers.Where(p => p.EventId == eventId).ToList();
                string pathRoot = Server.MapPath("~/haiupload/areacustomerevent.xlsx");
                string name = "areacustomer" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                string pathTo = Server.MapPath("~/temp/" + name);

                System.IO.File.Copy(pathRoot, pathTo);

                try
                {
                    FileInfo newFile = new FileInfo(pathTo);

                    using (ExcelPackage package = new ExcelPackage(newFile))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                        for (int i = 0; i < data.Count; i++)
                        {

                            try
                            {
                                worksheet.Cells[i + 2, 1].Value = i + 1;
                                worksheet.Cells[i + 2, 2].Value = data[i].CInfoCommon.CCode;
                                worksheet.Cells[i + 2, 3].Value = data[i].CInfoCommon.Phone;
                                worksheet.Cells[i + 2, 4].Value = data[i].CInfoCommon.CName;

  
                                worksheet.Cells[i + 2, 7].Value = "NÔNG DÂN";
                            }
                            catch
                            {
                                return RedirectToAction("error", "home");
                            }

                        }

                        package.Save();

                        return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-nong-dan-tham-gia-khuyen-mai-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

                    }
                }
                catch
                {
                    return RedirectToAction("error", "home");
                }
            }

        }

        public ActionResult RemoveEventArea(string eventId, string areaId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            var find = db.EventAreas.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return Json(new { error = 0, msg = "Chưa có khu vực này trong danh sách." }, JsonRequestBehavior.AllowGet);

            var eventInfo = find.EventInfo;

            if (eventInfo.ESTT != 0 && eventInfo.ESTT != 3)
            {
                return Json(new { error = 0, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }

            var listAreaCustomer = db.EventCustomers.ToList();

            foreach (var item in listAreaCustomer)
            {
                db.EventCustomers.Remove(item);
            }

            db.EventAreas.Remove(find);
            db.SaveChanges();


            return Json(new { error = 1, msg = "Đã xóa khu vực." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult eventAreaModify(string eventId, string areaId, int? page)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }

            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return RedirectToAction("modifyevent", "eventmanage", new { id = eventId });
            }

            var haiArea = db.HaiAreas.Find(areaId);
            if (haiArea == null)
            {
                return RedirectToAction("error", "home");
            }

            var find = db.EventAreas.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");

            ViewBag.AreaEvent = find;



            return View(db.EventCustomers.Where(p => p.EventId == eventId).OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult RemoveEventCustomer(string eventId, string areaId, string cid)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");


            var find = db.EventAreas.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");

            var eventInfo = find.EventInfo;

            EventCustomer info = db.EventCustomers.Where(p => p.EventId == eventId && p.CInfoId == cid).FirstOrDefault();

            if (info != null)
            {
                db.EventCustomers.Remove(info);
                db.SaveChanges();
            }
            return RedirectToAction("eventAreaModify", new { eventId = eventId, areaId = areaId });
        }



        [HttpPost]
        public ActionResult eventAreaModify(string eventId, string areaId, HttpPostedFileBase ciijoin)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            var find = db.EventAreas.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");
            var eventInfo = find.EventInfo;


            if (ciijoin != null && ciijoin.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(ciijoin.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "ciijoin_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    ciijoin.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string cCode = Convert.ToString(sheet.Cells[i, 1].Value);

                        var checkCII = db.C2Info.Where(p => p.Code == cCode).FirstOrDefault();
                        if (checkCII != null)
                        {
                            var cEvent = new EventCustomer()
                            {
                                CInfoId = checkCII.InfoId,
                                EventId = eventInfo.Id,
                                IsJoint = 1
                            };

                            db.EventCustomers.Add(cEvent);
                        }

                    }
                    db.SaveChanges();
                }
            }

            return RedirectToAction("eventAreaModify", new { eventId = eventId, areaId = areaId });
        }


        // famer
        public ActionResult AddEventAreaFarmer(string eventId, string areaId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }
            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return Json(new { error = 0, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }
            var haiArea = db.HaiAreas.Find(areaId);
            if (haiArea == null)
            {
                return RedirectToAction("error", "home");
            }

            var find = db.EventAreaFarmers.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find != null)
                return Json(new { error = 0, msg = "Đã có khu vực này trong danh sách." }, JsonRequestBehavior.AllowGet);


            var eventArea = new EventAreaFarmer()
            {
                AreaId = areaId,
                EventId = eventId,
                AllAgency = 1
            };

            db.EventAreaFarmers.Add(eventArea);
            db.SaveChanges();

            return Json(new { error = 1, msg = "Đã thêm khu vực.", name = haiArea.Name, code = haiArea.Code, notes = haiArea.Notes, id = haiArea.Id }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RemoveEventAreaFarmer(string eventId, string areaId)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return Json(new { error = 0, msg = "Không có quyền." }, JsonRequestBehavior.AllowGet);

            var find = db.EventAreaFarmers.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return Json(new { error = 0, msg = "Chưa có khu vực này trong danh sách." }, JsonRequestBehavior.AllowGet);

            var eventInfo = find.EventInfo;

            if (eventInfo.ESTT != 0 && eventInfo.ESTT != 3)
            {
                return Json(new { error = 0, msg = "Chương trình đang thực thi." }, JsonRequestBehavior.AllowGet);
            }

            var listAreaCustomer = db.EventCustomerFarmers.ToList();

            foreach (var item in listAreaCustomer)
            {
                db.EventCustomerFarmers.Remove(item);
            }

            db.EventAreaFarmers.Remove(find);
            db.SaveChanges();


            return Json(new { error = 1, msg = "Đã xóa khu vực." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EventAreaFarmerModify(string eventId, string areaId, int? page)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var checkEvent = db.EventInfoes.Find(eventId);

            if (checkEvent == null)
            {
                return RedirectToAction("error", "home");
            }

            if (checkEvent.ESTT != 0 && checkEvent.ESTT != 3)
            {
                return RedirectToAction("modifyevent", "eventmanage", new { id = eventId });
            }

            var haiArea = db.HaiAreas.Find(areaId);
            if (haiArea == null)
            {
                return RedirectToAction("error", "home");
            }

            var find = db.EventAreaFarmers.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");

            ViewBag.AreaEvent = find;



            return View(db.EventCustomerFarmers.OrderByDescending(p => p.CInfoCommon.CreateTime).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult RemoveEventCustomerFarmer(string eventId, string areaId, string cid)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");


            var find = db.EventAreaFarmers.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");

            var eventInfo = find.EventInfo;

            EventCustomerFarmer info = db.EventCustomerFarmers.Where(p => p.EventId == eventId && p.CInfoId == cid).FirstOrDefault();

            if (info != null)
            {
                db.EventCustomerFarmers.Remove(info);
                db.SaveChanges();
            }
            return RedirectToAction("EventAreaFarmerModify", new { eventId = eventId, areaId = areaId });
        }



        [HttpPost]
        public ActionResult EventAreaFarmerModify(string eventId, string areaId, HttpPostedFileBase farmerjoin)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");

            var find = db.EventAreaFarmers.Where(p => p.AreaId == areaId && p.EventId == eventId).FirstOrDefault();

            if (find == null)
                return RedirectToAction("error", "home");
            var eventInfo = find.EventInfo;


            if (farmerjoin != null && farmerjoin.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(farmerjoin.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "farmerjoin_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    farmerjoin.SaveAs(path);
                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string phone = Convert.ToString(sheet.Cells[i, 1].Value);

                        var checkFarmer = db.CInfoCommons.Where(p => p.Phone == phone && p.CType == "FARMER").FirstOrDefault();
                        if (checkFarmer != null)
                        {
                            var cEvent = new EventCustomerFarmer()
                            {
                                CInfoId = checkFarmer.Id,
                                EventId = eventInfo.Id,
                                IsJoint = 1
                            };

                            db.EventCustomerFarmers.Add(cEvent);
                        }

                    }
                    db.SaveChanges();
                }
            }

            return RedirectToAction("EventAreaFarmerModify", new { eventId = eventId, areaId = areaId });
        }


        // phan qua
        public ActionResult AwardManage(int? page)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 0))
                return RedirectToAction("relogin", "home");
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(db.AwardInfoes.OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize));
        }


        public ActionResult ModifyAward(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            var check = db.AwardInfoes.Find(id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }

            return View(check);
        }

        [HttpPost]
        public ActionResult ModifyAward(AwardInfo info, HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            var check = db.AwardInfoes.Find(info.Id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }

            check.Name = info.Name;
            check.Point = info.Point;
            string urlThumbnail = check.Thumbnail;
            if (files != null)
            {
                try
                {
                    string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                    string fsave = "~/haiupload/award/" + dfolder;

                    bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                    MemoryStream target = new MemoryStream();
                    files.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Width = 70,
                        isSacle = true,
                        UploadPath = fsave
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, ".png");

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/haiupload/award/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {

                }
            }
            check.Thumbnail = urlThumbnail;

            db.Entry(check).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("AwardManage");
        }

        [HttpPost]
        public ActionResult DeleteAward(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            var check = db.AwardInfoes.Find(id);

            if (check == null)
            {
                return RedirectToAction("error", "home");
            }

            db.AwardInfoes.Remove(check);
            db.SaveChanges();

            return RedirectToAction("AwardManage");
        }

        [HttpPost]
        public ActionResult CreateAward(AwardInfo info, HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 1))
                return RedirectToAction("relogin", "home");
            string urlThumbnail = "";
            if (files != null)
            {
                try
                {
                    string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                    string fsave = "~/haiupload/award/" + dfolder;

                    bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                    MemoryStream target = new MemoryStream();
                    files.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Width = 70,
                        isSacle = true,
                        UploadPath = fsave
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, ".png");

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/haiupload/award/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {

                }
            }

            info.Id = Guid.NewGuid().ToString();
            info.CreateDate = DateTime.Now;

            db.AwardInfoes.Add(info);
            db.SaveChanges();

            return RedirectToAction("AwardManage");
        }


        // show event point
        public ActionResult ShowAppHistory(string DateFrom, string DateTo, int? page, string search, string MSGType, string Agency)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 0))
                return RedirectToAction("relogin", "home");


            if (search == null)
                search = "";

            if (MSGType == null)
                MSGType = "";

            if (Agency == null)
                Agency = "";

            ViewBag.MSGType = MSGType;
            ViewBag.Agency = Agency;
            ViewBag.SearchText = search;
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            DateTime dFrom = DateTime.Now.Date;
            DateTime dTo = DateTime.Now.Date;

            if (DateFrom != null)
                dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);


            if (DateTo != null)
                dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);

            ViewBag.DateFrom = dFrom;
            ViewBag.DateTo = dTo;


            var allHistory = (from log in db.MSGPoints
                              where DbFunctions.TruncateTime(log.AcceptTime)
                                                 >= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(log.AcceptTime)
                                                <= DbFunctions.TruncateTime(dTo) && (log.CInfoCommon.Phone.Contains(search) || log.CInfoCommon.CCode.Contains(search) || log.CInfoCommon.CName.Contains(search)) && log.MSGType.Contains(MSGType) && log.CInfoCommon.CType.Contains(Agency)
                              select log).OrderByDescending(p => p.AcceptTime).ToPagedList(pageNumber, pageSize);

            return View(allHistory);





        }

        // diem tich luy
        public ActionResult ShowSavePoint(int? page, string atext = "", string etext = "", int status = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageEvent", 0))
                return RedirectToAction("relogin", "home");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.Status = status;
            ViewBag.Agency = atext;
            ViewBag.Event = etext;


            var arr = db.AgencySavePoints.Where(p => p.EventInfo.ESTT == status && p.EventInfo.Name.Contains(etext) && (p.CInfoCommon.CCode.Contains(atext) || p.CInfoCommon.Phone.Contains(atext) || p.CInfoCommon.CName.Contains(atext))).OrderByDescending(p => p.ModifyTime).ToPagedList(pageNumber, pageSize);

            return View(arr);
        }



    }
}