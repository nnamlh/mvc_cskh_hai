using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NDHSITE.Controllers
{
    public class UploadController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        [HttpPost]
        public ActionResult CheckIn(HttpPostedFileBase image, string extension, string user, string token)
        {
            if (checkLoginSession(user, token))
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
                        UploadPath = fsave
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

        private bool checkLoginSession(string user, string token)
        {
            var check = db.APIAuthHistories.Where(p => p.UserLogin == user && p.Token == token && p.IsExpired == 0).FirstOrDefault();

            return check != null ? true : false;
        }
    }
}