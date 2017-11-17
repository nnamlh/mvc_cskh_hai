using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Util;

namespace NDHSITE.Controllers
{
    public class UploadController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();
        MongoHelper mongoHelp = new MongoHelper();

        [HttpPost]
        public ActionResult CheckIn(HttpPostedFileBase image, string extension, string user, string token)
        {
          
            if (mongoHelp.checkLoginSession(user, token))
            {
                string dfolder = DateTime.Now.Date.ToString("d-M-yyyy");
                string fsave = "~/uploadfolder/" + dfolder;

                bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(fsave));


                string urlThumbnail = "";
                try
                {
                    MemoryStream target = new MemoryStream();
                    image.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Width = 3000,
                        isSacle = false,
                        UploadPath = fsave,
                        user = user
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, extension);

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/uploadfolder/" + dfolder + "/" + imageResult.ImageName;
                    }

                }
                catch
                {
                    return Json(new { id = "0", msg = "Image upload to fail" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { id = "1", msg = urlThumbnail }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { id = "0", msg = "Faile token user" }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult Decor(HttpPostedFileBase image, string extension, string user, string token, string agency, string group, double lng, double lat)
        {
            if (mongoHelp.checkLoginSession(user, token))
            {

                HaiStaff staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                if (staff == null)
                    return Json(new { id = "0", msg = "Sai thong tin" }, JsonRequestBehavior.AllowGet);


                CInfoCommon cinfo = db.CInfoCommons.Where(p => p.CCode == agency).FirstOrDefault();

                if (cinfo == null)
                    return Json(new { id = "0", msg = "Sai thong tin" }, JsonRequestBehavior.AllowGet);


                string dfolder = user + "/" + group + "/" +  DateTime.Now.Date.ToString("dd-MM-yyyy");

                string fsave = "~/uploadfolder/" + dfolder;

                bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(fsave));


                string urlThumbnail = "";
                try
                {
                    MemoryStream target = new MemoryStream();
                    image.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Width = 3000,
                        isSacle = false,
                        UploadPath = fsave,
                        user = agency
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, extension);

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/uploadfolder/" + dfolder + "/" + imageResult.ImageName;
                    }


                    // save inffo
                    var decor = new DecorImage()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AgencyCode = agency,
                        AgencyType = cinfo.CType,
                        CreateTime = DateTime.Now.TimeOfDay,
                        DaySend = DateTime.Now.Day,
                        DecorGroup = group,
                        StaffId = staff.Id,
                        ImageUrl = urlThumbnail,
                        Lat = lat,
                        Lng = lng,
                        MonthSend =DateTime.Now.Month,
                        YearSend = DateTime.Now.Year
                    };

                    db.DecorImages.Add(decor);
                    db.SaveChanges();

                }
                catch
                {
                    return Json(new { id = "0", msg = "Image upload to fail" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { id = "1", msg = urlThumbnail }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { id = "0", msg = "Faile token user" }, JsonRequestBehavior.AllowGet);
        }




        private bool checkLoginSession(string user, string token)
        {
            // var check = db.APIAuthHistories.Where(p => p.UserLogin == user && p.Token == token && p.IsExpired == 0).FirstOrDefault();

            //  return check != null ? true : false;
            return true;
        }

        private string FolderSave(int id)
        {
            switch (id)
            {
                case 1:
                    return "agencyshop";
                default:
                    return "temp";
            }
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase image, string extension, string user, string token, int folder)
        {

            if (mongoHelp.checkLoginSession(user, token))
            {
                string folderSave = FolderSave(folder);

                string fsave = "~/uploadfolder/" + folderSave;

                bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(fsave));


                string urlThumbnail = "";
                try
                {
                    MemoryStream target = new MemoryStream();
                    image.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();

                    ImageUpload imageUpload = new ImageUpload
                    {
                        Width = 3000,
                        isSacle = false,
                        UploadPath = fsave,
                        user = user
                    };

                    ImageResult imageResult = imageUpload.RenameUploadFile(data, extension);

                    if (imageResult.Success)
                    {
                        urlThumbnail = "/uploadfolder/" + folderSave+ "/" + imageResult.ImageName;
                    }

                }
                catch
                {
                    return Json(new { id = "0", msg = "Image upload to fail" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { id = "1", msg = urlThumbnail }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { id = "0", msg = "Faile token user" }, JsonRequestBehavior.AllowGet);
        }



    }
}