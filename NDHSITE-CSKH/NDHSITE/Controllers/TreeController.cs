using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDHSITE.Models;
using PagedList;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.IO;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class TreeController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        //
        // GET: /Tree/
        public ActionResult Manage(int? page, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 0))
                return RedirectToAction("relogin", "home");

            if (search == null)
                search = "";

            ViewBag.SearchText = search;
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(db.TreeInfoes.Where(p => p.Name.Contains(search)).OrderBy(p => p.TreeType).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult AddTree(TreeInfo info)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 1))
                return RedirectToAction("relogin", "home");
            info.Id = Guid.NewGuid().ToString();
            info.CreateDate = DateTime.Now;

            db.TreeInfoes.Add(info);
            db.SaveChanges();

            return RedirectToAction("manage", "tree");
        }


        [HttpPost]
        public ActionResult Delete(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 1))
                return RedirectToAction("relogin", "home");
            var info = db.TreeInfoes.Find(Id);

            if (info == null)
                return RedirectToAction("manage", "tree");

            db.TreeInfoes.Remove(info);
            db.SaveChanges();

            return RedirectToAction("manage", "tree");
        }

        public ActionResult Modify(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 1))
                return RedirectToAction("relogin", "home");
            var info = db.TreeInfoes.Find(Id);

            if (info == null)
                return RedirectToAction("manage", "tree");

            return View(info);
        }

        [HttpPost]
        public ActionResult Modify(TreeInfo info)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 1))
                return RedirectToAction("relogin", "home");
            var infoCheck = db.TreeInfoes.Find(info.Id);

            if (infoCheck == null)
                return RedirectToAction("manage", "tree");


            infoCheck.Name = info.Name;
            infoCheck.TreeType = info.TreeType;
            infoCheck.STT = info.STT;
            infoCheck.Acreage = info.Acreage;
            db.Entry(infoCheck).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("modify", "tree", new { Id = info.Id });
        }

        public ActionResult ShowTree(int? page, string search)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ShowTree", 0))
                return RedirectToAction("relogin", "home");
            if (search == null)
                search = "";

            ViewBag.SearchText = search;
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(db.TreeInfoes.Where(p => p.Name.Contains(search)).OrderBy(p => p.TreeType).ToPagedList(pageNumber, pageSize));
        }


        [HttpPost]
        public ActionResult ExcelTree(HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageTree", 1))
                return RedirectToAction("relogin", "home");
            if (files != null && files.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "tree_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
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
                        string pCode = Convert.ToString(sheet.Cells[i, 1].Value);
                        if (pCode != null && pCode.Trim() != "")
                        {
                            var checkDb = db.ProductInfoes.Where(p => p.PCode == pCode).FirstOrDefault();
                            if (checkDb == null)
                            {
                                string name = Convert.ToString(sheet.Cells[i, 2].Value);

                                string type = Convert.ToString(sheet.Cells[i, 3].Value);

                                string acreage = Convert.ToString(sheet.Cells[i, 4].Value);

                                string stt = Convert.ToString(sheet.Cells[i, 5].Value);

                                string dateStr = Convert.ToString(sheet.Cells[i, 6].Value);

                                DateTime date;
                                try
                                {
                                    date = DateTime.ParseExact(dateStr, "dd/MM/yyyy", null);
                                }
                                catch
                                {
                                    date = DateTime.Now;
                                }

                                var treeInfo = new TreeInfo()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Name = name,
                                    TreeType = type,
                                    Acreage = acreage,
                                    STT = stt,
                                    CreateDate = date
                                };

                                db.TreeInfoes.Add(treeInfo);
                            }
                        }


                    }
                    db.SaveChanges();



                }
            }
            return RedirectToAction("manage", "tree");
        }
    }
}