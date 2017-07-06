using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDHAPI.Models;

namespace NDHAPI.Models
{
    public abstract class ProductScanAction
    {

        protected NDHDBEntities db;
        protected string user;
        protected WavehouseInfo wareReceieInfo;
        protected WavehouseInfo wareActionInfo;

        public ProductScanAction(NDHDBEntities db, WavehouseInfo wareActionInfo, string user, WavehouseInfo wareReceieInfo)
        {
            this.db = db;
            this.user = user;
            this.wareActionInfo = wareActionInfo;
            this.wareReceieInfo = wareReceieInfo;
        }
        
        public abstract BarcodeHistory Handle(string barcode);

        protected ProductInfo GetProduct(string barcode)
        {

            if (String.IsNullOrEmpty(barcode))
                return null;

            if (barcode.Length < 17)
                return null;

            string countryCode = barcode.Substring(0, 3);
            if (countryCode != "893")
                return null;

            string companyCode = barcode.Substring(3, 5);
            if (companyCode != "52433")
                return null;

            string productCode = barcode.Substring(8, 2);

            var product = db.ProductInfoes.Where(p => p.Barcode == productCode).FirstOrDefault();

            return product;

        }

        public abstract string checkRole();


        protected decimal getInventory(string caseCode, string wCode)
        {
            var data = db.PTrackings.Where(p => p.CaseCode == caseCode && p.WCode == wCode).FirstOrDefault();

            if (data == null)
                return 0;
            else
                return Convert.ToDecimal(data.Quantity);
        }

        // save history
        protected void saveHistory(string barcode, string caseCode, string boxCode, ProductInfo product, string stt, decimal? quantity, WavehouseInfo wInfo)
        {

            var history = new PHistory()
            {
                Id = Guid.NewGuid().ToString(),
                Barcode = barcode,
                PStatus = stt,
                BoxCode = boxCode,
                UserSend = user,
                WCode = wInfo.wCode,
                WName = wInfo.wName,
                WType = wInfo.wType,
                ProductCode = product.Barcode,
                CreateDate = DateTime.Now,
                CaseCode = caseCode,
                Quantity = quantity
            };
            db.PHistories.Add(history);
            db.SaveChanges();

            var tracking = db.PTrackings.Where(p => p.CaseCode == caseCode && p.WCode == wInfo.wCode).FirstOrDefault();


            if (tracking == null)
            {
                var pTracking = new PTracking()
                {
                    Id = Guid.NewGuid().ToString(),
                    WCode = wInfo.wCode,
                    WType = wInfo.wType,
                    WName = wInfo.wName,
                    CaseCode = caseCode,
                    ProductId = product.Id,
                    Quantity = quantity
                };

                if (stt == "NK")
                {
                    pTracking.ImportTime = DateTime.Now;
                }
                else
                {
                    pTracking.ExportTime = DateTime.Now;
                }
                db.PTrackings.Add(pTracking);
                db.SaveChanges();
            }
            else
            {
                if (stt == "NK")
                {
                    tracking.Quantity = tracking.Quantity + quantity;
                    tracking.ImportTime = DateTime.Now;
                }
                else
                {
                    tracking.Quantity = tracking.Quantity - quantity;
                    tracking.ExportTime = DateTime.Now;
                }

                db.Entry(tracking).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

        }

    }

    public class ExportAction : ProductScanAction
    {
        public ExportAction(NDHDBEntities db, WavehouseInfo wareAction, string user, WavehouseInfo waveReceie)
            : base(db, wareAction, user, waveReceie)
        {

        }

        public override BarcodeHistory Handle(string barcode)
        {

            var caseCode = barcode.Substring(0, 15);
            var boxCode = "";

            BarcodeHistory history = new BarcodeHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                UserLogin = user,
                WareHouse = wareActionInfo.wCode,
                WareHouseType = wareActionInfo.wType,
                WareHouseName = wareActionInfo.wName,
                PStatus = "XK",
                Barcode =barcode,
                CaseCode = caseCode,
                IsSuccess = 0
            };


            var product = GetProduct(barcode);

            if (product == null)
            {

                history.ProductCode = "";
                history.ProductName = "";
                history.Messenge = "Mã sản phẩm sai";

                return history;
            }

            history.ProductName = product.PName;
            history.ProductCode = product.Barcode;

            var wareInfo = db.Warehouses.Where(p => p.Code == wareActionInfo.wType).FirstOrDefault();
           
            PHistory checkHistory = null;
           
            decimal quantity = 1.0M;
            var remain = getInventory(caseCode, wareActionInfo.wCode);

            if (product.IsBox == 1 && wareInfo.IsScanBox == 1)
            {
                boxCode = barcode.Substring(0, 16);
                // check ma co phai la hop ko
                var lastChar = barcode.Substring(barcode.Length - 1, 1);
                if (lastChar != "1")
                {
                    history.Messenge = "Sản phẩm này chỉ quét hộp";
                    return history;
                }

                checkHistory = db.PHistories.Where(p => p.BoxCode == boxCode && p.WCode == wareActionInfo.wCode).OrderByDescending(p => p.CreateDate).FirstOrDefault();
  
                quantity = 1.0M / (int)product.QuantityBox;
            }
            else
            {
                checkHistory = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode == wareActionInfo.wCode).OrderByDescending(p => p.CreateDate).FirstOrDefault();
            }

            if (checkHistory != null)
            {
                if (checkHistory.PStatus == "XK")
                {
                    history.Messenge = "Sản phẩm đã xuất kho : " + checkHistory.CreateDate.ToString();
                    return history;
                }

            }
            else
            {
                history.Messenge = "Sản phẩm phải nhập kho";
                return history;
            }

            if (remain < quantity)
            {
                history.Messenge ="Không đủ hàng để xuất";
                return history;
            }


             saveHistory(barcode, caseCode, boxCode, product, "XK",quantity, wareActionInfo);

            if (wareReceieInfo != null) {
                saveHistory(barcode, caseCode, boxCode, product, "NK", quantity, wareReceieInfo);
                history.WareRelative = wareReceieInfo.wCode;
                history.WareRelativeName = wareReceieInfo.wName;
                history.WareRelativeType = wareReceieInfo.wType;
            }

            history.Quantity = quantity;
            history.IsSuccess = 1;
            history.Messenge = "Đã xuất kho";
            return history;
        }

        public override string checkRole()
        {

            var wareInfo = db.Warehouses.Where(p => p.Code == wareActionInfo.wType).FirstOrDefault();

            if (wareInfo == null)
                return "Người quét không hợp lệ";

            if (wareInfo.ScanExport == 0)
            {
                return "Không quét xuất kho";
            }

            if (wareReceieInfo == null)
            {
                if (wareInfo.InputReceive == 1)
                {
                    return "Phải nhập kho nhận";
                }
            }
            else
            {
                if (!wareInfo.WareReceive.Contains(wareReceieInfo.wType))
                {
                    return "Sai nơi nhận";
                }
            }


            return null;
        }
    }

    public class ImportAction : ProductScanAction
    {

        public ImportAction(NDHDBEntities db, WavehouseInfo wareAction, string user, WavehouseInfo waveReceie)
            : base(db, wareAction, user, waveReceie)
        {

        }

        public override string checkRole()
        {

            var wareInfo = db.Warehouses.Where(p => p.Code == wareActionInfo.wType).FirstOrDefault();

            if (wareInfo == null)
                return "Người quét không hợp lệ";


            if (wareInfo.ScanImport == 0)
            {
                return "Không quét nhập kho";
            }


            return null;
        }


        public override BarcodeHistory Handle(string barcode)
        {
            var caseCode = barcode.Substring(0, 15);
            var boxCode = "";

            BarcodeHistory history = new BarcodeHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                UserLogin = user,
                WareHouse = wareActionInfo.wCode,
                WareHouseType = wareActionInfo.wType,
                WareHouseName = wareActionInfo.wName,
                PStatus = "NK",
                Barcode = barcode,
                CaseCode = caseCode,
                IsSuccess = 0
            };


            var product = GetProduct(barcode);
         
            if (product == null)
            {
                history.ProductCode = "";
                history.ProductName = "";
                history.Messenge = "Mã sản phẩm sai";

                return history;
            }

            history.ProductName = product.PName;
            history.ProductCode = product.Barcode;

            var wareInfo = db.Warehouses.Where(p => p.Code == wareActionInfo.wType).FirstOrDefault();
           
            PHistory checkHistory = null;
            
            decimal quantity = 1.0M;

            if (product.IsBox == 1 && wareInfo.IsScanBox == 1)
            {
                boxCode = barcode.Substring(0, 16);
                // check ma co phai la hop ko
                var lastChar = barcode.Substring(barcode.Length - 1, 1);
                if (lastChar != "1")
                {
                    history.Messenge = "Sản phẩm này chỉ quét hộp";
                    return history;
                }

                checkHistory = db.PHistories.Where(p => p.BoxCode == boxCode && p.WCode == wareActionInfo.wCode && p.PStatus == "NK").FirstOrDefault();

                if (checkHistory != null)
                {
                    history.Messenge = "Sản phẩm đã xuất kho : " + checkHistory.CreateDate.ToString();
                    return history;
                }

                quantity = 1.0M / (int)product.QuantityBox;
            }
            else
            {
                checkHistory = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode == wareActionInfo.wCode && p.PStatus == "NK").FirstOrDefault();

                if (checkHistory != null)
                {
                    history.Messenge = "Sản phẩm đã nhập kho: " + checkHistory.CreateDate.ToString();
                    return history;
                }
            }

            // check kho truoc 
            PHistory checkLastImport = null;

            if (product.IsBox == 1 && wareInfo.IsScanBox == 1)
            {
                checkLastImport = db.PHistories.Where(p => p.CaseCode == caseCode && p.BoxCode == boxCode && p.WCode != wareActionInfo.wCode).OrderByDescending(p => p.CreateDate).FirstOrDefault();

                if (checkLastImport == null)
                {
                    checkLastImport = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode != wareActionInfo.wCode).OrderByDescending(p => p.CreateDate).FirstOrDefault();
                }
            }
            else
            {
                checkLastImport = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode != wareActionInfo.wCode).OrderByDescending(p => p.CreateDate).FirstOrDefault();
            }

            if (checkLastImport != null)
            {
                // kiem tra do phai kho cha khong
                var wareLastImport = db.Warehouses.Where(p => p.Code == checkLastImport.WType).FirstOrDefault();

                if (!wareLastImport.WareReceive.Contains(wareActionInfo.wType))
                {
                    history.Messenge = "Sản phẩm không thể nhập kho";
                    return history;
                }

                if (checkLastImport.PStatus == "NK")
                {
                    // tru kho
                    saveHistory(barcode, caseCode, boxCode, product, "XK", quantity, new WavehouseInfo() {
                        wCode = checkLastImport.WCode,
                        wName = checkLastImport.WName,
                        wType= checkLastImport.WType
                    });

                    history.WareRelative = checkLastImport.WCode;
                    history.WareRelativeName = checkLastImport.WName;
                    history.WareRelativeType = checkLastImport.WType;
                }

            }
            else
            {
                if (wareActionInfo.wType != "W")
                {
                    history.Messenge = "Phải nhập kho tổng";
                    return history;
                }
            }

            saveHistory(barcode, caseCode, boxCode, product, "NK", quantity, wareActionInfo);
            history.Quantity = quantity;
            history.Messenge = "Đã nhập kho";
            history.IsSuccess = 1;
            return history;
        }
    }
}

