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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NDHSITE.Controllers
{

    [Authorize]
    public class ProductController : Controller
    {
        NDHDBEntities db = new NDHDBEntities();

        //
        // GET: /Product/
        public ActionResult Manage(int? page, string search, int? type)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 0))
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
                    return View(db.ProductInfoes.Where(p => p.PGroup.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
                case 4:
                    ViewBag.STypeName = "Mã sản phẩm";
                    return View(db.ProductInfoes.Where(p => p.PCode.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
            }

            return View(db.ProductInfoes.Where(p => p.PName.Contains(search)).OrderBy(p => p.PName).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult AddProduct(ProductInfo product, string IsBox)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");

            product.Id = Guid.NewGuid().ToString();

            if (product.PCode != null && product.PCode.Trim() != "")
            {
                var checkDb = db.ProductInfoes.Where(p => p.PCode == product.PCode).FirstOrDefault();
                if (checkDb != null)
                    return RedirectToAction("manage", "product");
            }


            if (IsBox != null)
                product.IsBox = 1;
            else
                product.IsBox = 0;

            db.ProductInfoes.Add(product);
            db.SaveChanges();

            return RedirectToAction("manage", "product");
        }

        public ActionResult ModifyProduct(string id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");

            var product = db.ProductInfoes.Find(id);

            if (product == null)
            {
                return RedirectToAction("error", "home");
            }


            return View(product);
        }

        [HttpPost]
        public ActionResult ModifyProduct(ProductInfo product, string IsBox)
        {

            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");

            var productCheck = db.ProductInfoes.Find(product.Id);

            if (productCheck == null)
            {
                return RedirectToAction("error", "home");
            }

            var checkDb = db.ProductInfoes.Where(p => p.PCode == product.PCode).FirstOrDefault();
            if (checkDb == null)
                productCheck.PCode = product.PCode;

            productCheck.PName = product.PName;

            productCheck.Material = product.Material;
            productCheck.Producer = product.Producer;
            productCheck.PMajor = product.PMajor;
            productCheck.PGroup = product.PGroup;
            productCheck.Unit = product.Unit;
            productCheck.Utility = product.Utility;
            productCheck.Acronym = product.Acronym;
            productCheck.QuantityBox = product.QuantityBox;

            if (IsBox != null)
                productCheck.IsBox = 1;
            else
                productCheck.IsBox = 0;


            db.Entry(productCheck).State = System.Data.Entity.EntityState.Modified;


            db.SaveChanges();

            return RedirectToAction("modifyproduct", "product", new { id = productCheck.Id });
        }


        [HttpPost]
        public ActionResult excelProduct(HttpPostedFileBase files)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");

            if (files != null && files.ContentLength > 0)
            {

                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {

                    string fileSave = "product_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
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

                        string pName = Convert.ToString(sheet.Cells[i, 2].Value);

                        string Material = Convert.ToString(sheet.Cells[i, 3].Value);

                        string Utility = Convert.ToString(sheet.Cells[i, 4].Value);

                        string Unit = Convert.ToString(sheet.Cells[i, 5].Value);
                        string Producer = Convert.ToString(sheet.Cells[i, 6].Value);
                        string PGroup = Convert.ToString(sheet.Cells[i, 7].Value);

                        string Acronym = Convert.ToString(sheet.Cells[i, 8].Value);

                        string CardPoint = Convert.ToString(sheet.Cells[i, 9].Value);

                        string BoxPoint = Convert.ToString(sheet.Cells[i, 10].Value);

                        string PMajor = Convert.ToString(sheet.Cells[i, 11].Value);
                        string Barcode = Convert.ToString(sheet.Cells[i, 12].Value);

                        if (pCode != null && pCode.Trim() != "")
                        {

                            var checkDb = db.ProductInfoes.Where(p => p.PCode == pCode).FirstOrDefault();

                            if (checkDb == null)
                            {
                                var productInfo = new ProductInfo()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    PCode = pCode,
                                    PName = pName,
                                    Material = Material,
                                    Utility = Utility,
                                    Unit = Unit,
                                    Producer = Producer,
                                    PGroup = PGroup,
                                    Acronym = Acronym,
                                    CardPoint = CardPoint,
                                    BoxPoint = BoxPoint,
                                    PMajor = PMajor,
                                    Barcode = Barcode
                                };

                                db.ProductInfoes.Add(productInfo);
                            }
                            else
                            {
                                checkDb.PName = pName;
                                checkDb.Material = Material;
                                checkDb.Utility = Utility;
                                checkDb.Unit = Unit;
                                checkDb.Producer = Producer;
                                checkDb.PGroup = PGroup;
                                checkDb.Acronym = Acronym;
                                checkDb.CardPoint = CardPoint;
                                checkDb.BoxPoint = BoxPoint;
                                checkDb.PMajor = PMajor;
                                checkDb.Barcode = Barcode;
                                db.Entry(checkDb).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }


                    }
                    db.SaveChanges();



                }
            }
            return RedirectToAction("manage", "product");
        }
        public ActionResult delete(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");
            var product = db.ProductInfoes.Find(Id);

            if (product == null)
            {
                return RedirectToAction("error", "home");
            }

            db.ProductInfoes.Remove(product);
            db.SaveChanges();

            return RedirectToAction("manage", "product");
        }


        // tao seri
        public ActionResult ProductSeri(int? page, string productId, int? isUse, int? SeriType)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 0))
                return RedirectToAction("relogin", "home");

            if (String.IsNullOrEmpty(productId))
                productId = "-1";

            if (isUse == null)
                isUse = 0;

            if (SeriType == null)
                SeriType = 1;

            // 1: dai ly
            // 2: nong dan


            ViewBag.Products = db.ProductInfoes.ToList();
            ViewBag.ProductId = productId;

            ViewBag.IsUse = isUse;

            ViewBag.SeriType = SeriType;

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (productId == "-1")
                return View(db.ProductSeris.Where(p => p.IsUse == isUse && p.SeriType == SeriType).OrderByDescending(p => p.BeginTime).ToPagedList(pageNumber, pageSize));
            else
                return View(db.ProductSeris.Where(p => p.ProductId == productId && p.IsUse == isUse && p.SeriType == SeriType).OrderByDescending(p => p.BeginTime).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult ProductSeri(string productId, int Quantity, string DateFrom, string DateTo, int SeriType)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");


            var checkProduct = db.ProductInfoes.Find(productId);

            if (checkProduct == null)
                return RedirectToAction("error", "home");


            if (SeriType == 1 || SeriType == 2)
            {
                // 1: dai ly -- 2 : nong dan

                var maxSeri = db.ProductSeris.Max(p => p.Seri);

                if (maxSeri == null || maxSeri == 0)
                    maxSeri = 1000000;


                DateTime dFrom = DateTime.ParseExact(DateFrom, "dd/MM/yyyy", null);
                DateTime dTo = DateTime.ParseExact(DateTo, "dd/MM/yyyy", null);


                while (Quantity > 0)
                {
                    var generalCode = RandomCod();

                    var pSeri = new ProductSeri()
                    {
                        Id = Guid.NewGuid().ToString(),
                        BeginTime = dFrom,
                        ExpireTime = dTo,
                        IsUse = 0,
                        Seri = maxSeri + Quantity,
                        Code = generalCode,
                        ProductId = productId,
                        SeriType = SeriType
                    };

                    db.ProductSeris.Add(pSeri);
                    db.SaveChanges();

                    Quantity--;
                }

            }

            return RedirectToAction("productseri", "product");
        }

        private string RandomCod()
        {
            string[] arr = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            Random random = new Random();

            int length = arr.Length;

            string temp = "";

            for (var i = 0; i < 8; i++)
            {
                var idx = random.Next(0, length);
                temp += arr[idx];
            }

            var check = db.ProductSeris.Where(p => p.Code == temp).FirstOrDefault();
            if (check != null)
                temp = RandomCod();


            return temp;
        }

        [HttpPost]
        public ActionResult DeleteSeri(string Id)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "ManageProduct", 1))
                return RedirectToAction("relogin", "home");

            var check = db.ProductSeris.Find(Id);

            if (check != null && check.IsUse == 0)
            {
                db.ProductSeris.Remove(check);
                db.SaveChanges();
            }

            return RedirectToAction("productseri", "product");
        }

        public ActionResult ExportSeri(string productId = "-1", int? isUse = 0, int? SeriType = 1)
        {
            if (!Utitl.CheckUser(db, User.Identity.Name, "AgencyReport", 0))
                return RedirectToAction("relogin", "home");

            string pathRoot = Server.MapPath("~/haiupload/DSSERI.xlsx");
            string name = "seri" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            string pathTo = Server.MapPath("~/temp/" + name);

            System.IO.File.Copy(pathRoot, pathTo);

            try
            {
                FileInfo newFile = new FileInfo(pathTo);

                List<ProductSeri> data = new List<ProductSeri>();

                if (productId == "-1")
                  data = db.ProductSeris.Where(p => p.IsUse == isUse && p.SeriType == SeriType).OrderByDescending(p => p.BeginTime).ToList();
                else
                   data = db.ProductSeris.Where(p => p.ProductId == productId && p.IsUse == isUse && p.SeriType == SeriType).OrderByDescending(p => p.BeginTime).ToList();

                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["SERI"];

                    for (int i = 0; i < data.Count; i++)
                    {

                        try
                        {
                            worksheet.Cells[i + 2, 1].Value = i + 1;
                            worksheet.Cells[i + 2, 2].Value = data[i].Code;
                            worksheet.Cells[i + 2, 3].Value = data[i].Seri;
                            worksheet.Cells[i + 2, 4].Value = data[i].ProductInfo.PName;
                            worksheet.Cells[i + 2, 5].Value = data[i].BeginTime.Value.ToString("dd/MM/yyyy");

                            worksheet.Cells[i + 2, 6].Value = data[i].ExpireTime.Value.ToString("dd/MM/yyyy");


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


            return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-seri" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));

        }

        /*
        public ActionResult GeneralSeri()
        {


            return View();
        }

        [HttpPost]
        public ActionResult GeneralSeri(int Quantity)
        {

            string firstCharacter = DateTime.Now.Year.ToString();
            firstCharacter = firstCharacter.Substring(firstCharacter.Length - 1, 1);


            string[] arr = {"0","1", "2", "3", "4", "5", "6", "7", "8","9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            Random random = new Random();

            int length = arr.Length;

            List<string> listCode = new List<string>();
            while (Quantity > 0)
            {

                string temp = firstCharacter;

                for (var i = 0; i < 7; i++)
                {
                    var idx = random.Next(0, length);
                    temp += arr[idx];
                }

                listCode.Add(temp);

                Quantity--;

            }


            if (SaveFile(listCode))
            {
                ViewBag.MSG = "Thành công";
            }
            else
            {
                ViewBag.MSG = "Không tạo được mã";
            }

            return View();
        }


        private bool SaveFile(List<string> listInfo)
        {

            string filename = "~/saveseri/data1.json";

            bool exists = System.IO.Directory.Exists(Server.MapPath(filename));

            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath(filename));

            string json = JsonConvert.SerializeObject(listInfo, Formatting.Indented);
            System.IO.File.WriteAllText(Server.MapPath(filename), json);


            return true;
        }


        private List<string> ReadFile()
        {
            try
            {
                string filename = "~/saveseri/data.json";
                List<string> list = new List<string>();
                using (StreamReader file = System.IO.File.OpenText(Server.MapPath(filename)))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JArray o2 = (JArray)JToken.ReadFrom(reader);
                    list = o2.ToObject<List<string>>();
                    return list;
                }
            }
            catch
            {
                return new List<string>();
            }
        }
        */

    }
}