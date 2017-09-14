using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NDHSITE.Controllers
{
    public class EditorController : Controller
    {
        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor, string langCode)
        {
            string dfolder = DateTime.Now.Date.ToString("ddMMyyyy");
            string url = "/images/media/" + dfolder + "/"; // url to return
            string message; // message to display (optional)

            if (upload != null)
            {
                string ImageName = upload.FileName;

                string fsave = "~/images/media/" + dfolder;

                bool exists = System.IO.Directory.Exists(Server.MapPath(fsave));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(fsave));

                string path = System.IO.Path.Combine(Server.MapPath(fsave), ImageName);

                MemoryStream target = new MemoryStream();
                upload.InputStream.CopyTo(target);
                byte[] data = target.ToArray();

                ImageUpload imageUpload = new ImageUpload
                {
                    Width = 800,
                    isSacle = false,
                    UploadPath = fsave
                };
                ImageResult imageResult = imageUpload.RenameUploadFile(data, Path.GetExtension(upload.FileName));

                if (imageResult.Success)
                {
                    message = "Đả tải";
                    url = url + imageResult.ImageName;
                }
                else
                {
                    message = "";
                    url = "";
                }
            }
            else
            {
                message = "";
                url = "";
            }
            string output = @"<html><body><script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\", \"" + message + "\");</script></body></html>";
            return Content(output);

        }

        public ActionResult uploadPartial()
        {
            var appData = Server.MapPath("~/images/media");
            var images = Directory.GetFiles(appData).Select(x => new ImageViewModel
            {
                Url = Url.Content("/images/media" + Path.GetFileName(x))
            });

            return View(images);
        }
    }
}