using HAIAPI.Models;
using HAIAPI.Util;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class ProductController : RestMainController
    {
        [HttpGet]
        public List<string> GetProductTask(string user, string token)
        {
            // update regid firebase
            // /api/rest/functionproduct
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/product/getproducttask",
                Sucess = 1
            };

            var result = new List<string>();

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                result = GetUserFunction(user, "product");
            }
            catch (Exception e)
            {
                history.Sucess = 0;
                history.Error = e.Message;
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(history);

            return result;
        }


        #region updateProduct
        /// <summary>
        /// san pham
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ResultProduct UpdateProduct()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/product/updateproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultProduct()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestProduct>(requestContent);
                //  log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");

                WavehouseInfo wareActionInfo = new WavehouseInfo();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff != null)
                {
                    wareActionInfo.wCode = staff.HaiBranch.Code;
                    wareActionInfo.wName = staff.HaiBranch.Name;

                    var userStaff = db.AspNetUsers.Where(p => p.UserName == paser.user).FirstOrDefault();

                    var role = userStaff.AspNetRoles.FirstOrDefault();

                    if (role.Name == "Warehouse")
                        wareActionInfo.wType = "W";
                    else
                        wareActionInfo.wType = "B";
                }
                else
                {
                    var agency = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                    if (agency != null)
                    {
                        wareActionInfo.wCode = agency.CCode;
                        wareActionInfo.wName = agency.CName;
                        wareActionInfo.wType = agency.CType;
                    }
                }

                // check user receiver
                WavehouseInfo wareReceiInfo = null;
                // lay thong tin noi nhan
                if (!String.IsNullOrEmpty(paser.receiver.Trim()) && paser.status == "XK")
                {
                    wareReceiInfo = new WavehouseInfo();
                    var branchReceiver = db.HaiBranches.Where(p => p.Code == paser.receiver).FirstOrDefault();
                    if (branchReceiver != null)
                    {
                        wareReceiInfo.wType = "B";
                        wareReceiInfo.wCode = branchReceiver.Code;
                        wareReceiInfo.wName = branchReceiver.Name;
                    }
                    else
                    {
                        var agencyReceiver = db.CInfoCommons.Where(p => p.CCode == paser.receiver).FirstOrDefault();
                        if (agencyReceiver != null)
                        {
                            wareReceiInfo.wType = agencyReceiver.CType;
                            wareReceiInfo.wCode = agencyReceiver.CCode;
                            wareReceiInfo.wName = agencyReceiver.CName;
                        }

                    }

                    if (String.IsNullOrEmpty(wareReceiInfo.wCode) || wareActionInfo.wCode == wareReceiInfo.wCode)
                        throw new Exception("Sai thông tin nơi nhập hàng");
                }

                ProductScanAction wareAction = null;

                if (paser.status == "NK")
                {
                    wareAction = new ImportAction(db, wareActionInfo, paser.user, null);
                }
                else if (paser.status == "XK")
                {
                    wareAction = new ExportAction(db, wareActionInfo, paser.user, wareReceiInfo);
                }

                if (wareAction == null)
                    throw new Exception("Sai thông tin quét");

                string msg = wareAction.checkRole();
                if (msg != null)
                    throw new Exception(msg);

                // kiem tra nhap kho
                List<GeneralInfo> productsReturn = new List<GeneralInfo>();
                foreach (string barcode in paser.products)
                {
                    BarcodeHistory resultSave = wareAction.Handle(barcode);
                    db.BarcodeHistories.Add(resultSave);
                    db.SaveChanges();
                    productsReturn.Add(new GeneralInfo()
                    {
                        code = resultSave.Barcode,
                        name = resultSave.ProductName,
                        status = resultSave.Messenge,
                        success = Convert.ToInt32(resultSave.IsSuccess)
                    });

                }

                result.products = productsReturn;
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }

        #endregion

        #region Staff import for agency

        [HttpPost]
        public ResultInfo CheckLocationDistance()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/product/checklocationdistance",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };
            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestCheckLocationDistance>(requestContent);
                //  log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");


                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                if (staff == null)
                {
                    throw new Exception("Nhân viên mới được dùng chức năng này");
                }

                var agency = db.CInfoCommons.Where(p => p.CCode == paser.agency).FirstOrDefault();

                if (agency == null)
                {
                    throw new Exception("Sai mã đại lý");
                }

                var distance = GetDistanceTwoLocation(Convert.ToDouble(paser.latitude), Convert.ToDouble(paser.longitude), Convert.ToDouble(agency.Lat), Convert.ToDouble(agency.Lng));

                if (distance > 200)
                {
                    result.id = "2";
                    result.msg = "Ngoài đại lý";
                }
                else
                {
                    result.id = "1";
                    result.msg = "Tại đại lý";
                }

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }


        [HttpPost]
        public ResultProduct HelpAgencyImport()
        {
            //stt: nhập kho: NK
            // Xuất kho : XK
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/product/helpagencyimport",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultProduct()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;
            log.Content = requestContent;
            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestProductHelp>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);


                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");

                WavehouseInfo wareActionInfo = new WavehouseInfo();
                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                if (staff == null)
                {
                    throw new Exception("Chỉ nhân viên công ty mới sử dụng chức năng này");
                }

                var agency = db.CInfoCommons.Where(p => p.CCode == paser.agency).FirstOrDefault();

                if (agency != null)
                {
                    wareActionInfo.wCode = agency.CCode;
                    wareActionInfo.wName = agency.CName;
                    wareActionInfo.wType = agency.CType;
                }
                else
                {
                    throw new Exception("Sai mã khách hàng");
                }

                // kiem tra toa do

                ProductScanAction wareAction = new ImportAction(db, wareActionInfo, paser.user, null);

                string msg = wareAction.checkRole();
                if (msg != null)
                    throw new Exception(msg);

                // kiem tra nhap kho
                List<GeneralInfo> productsReturn = new List<GeneralInfo>();
                foreach (string barcode in paser.products)
                {
                    BarcodeHistory resultSave = wareAction.Handle(barcode);
                    resultSave.StaffHelp = staff.Code;
                    if (paser.nearAgency == 1)
                    {
                        resultSave.StaffHelpIssue = "locationin";
                    }
                    else
                    {
                        resultSave.StaffHelpIssue = "locationout";
                    }
                    resultSave.LocationStaff = paser.latitude + "|" + paser.longitude;
                    db.BarcodeHistories.Add(resultSave);
                    db.SaveChanges();

                    productsReturn.Add(new GeneralInfo()
                    {
                        code = resultSave.Barcode,
                        name = resultSave.ProductName,
                        status = resultSave.Messenge,
                        success = Convert.ToInt32(resultSave.IsSuccess)
                    });
                }

                result.products = productsReturn;
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;
        }

        private double GetDistanceTwoLocation(double sLatitude, double sLongitude, double eLatitude, double eLongitude)
        {
            var sCoord = new GeoCoordinate(sLatitude, sLongitude);
            var eCoord = new GeoCoordinate(eLatitude, eLongitude);

            return sCoord.GetDistanceTo(eCoord);
        }
        #endregion


        #region Tracking
        /// <summary>
        /// --------------------------------
        /// </summary>
        /// <returns></returns>
        // tracking san pham
        [HttpPost]
        public ResultTracking Tracking()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/product/tracking",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultTracking()
            {
                id = "1",
                msg = "success"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestTracking>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                  throw new Exception("Tài khoản của bạn đã bị đăng nhập trên một thiết bị khác!");

                result.code = paser.code;

                var checkProduct = GetProduct(paser.code);

                if (checkProduct == null)
                    throw new Exception("Mã không hợp lệ");


                result.name = checkProduct.PName;

                var boxCode = paser.code.Substring(0, 15);

                List<PTrackingInfo> productTracking = new List<PTrackingInfo>();

                var pHistoryTracking = db.PTrackings.Where(p => p.CaseCode == boxCode).OrderBy(p => p.ImportTime).ToList();

                foreach (var item in pHistoryTracking)
                {
                    PTrackingInfo pTracking = new PTrackingInfo();

                    if (item.WType == "CI")
                        pTracking.name = "CẤP 1: " + item.WName;
                    else if (item.WType == "CII")
                        pTracking.name = "CẤP 2: " + item.WName;
                    else if (item.WType == "B")
                        pTracking.name = "CHI NHÁNH: " + item.WName;
                    else if (item.WType == "FARMER")
                        pTracking.name = "NÔNG DÂN: " + item.WName;
                    else if (item.WType == "W")
                        pTracking.name = "TỔNG KHO: " + item.WName;

                    if (item.ImportTime != null)
                        pTracking.importTime = "Nhập kho lúc " + item.ImportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.importTime = "Chưa nhập kho";

                    if (item.ExportTime != null)
                        pTracking.exportTime = "Xuất kho lúc " + item.ExportTime.Value.ToString("dd/MM/yyyy HH:mm");
                    else
                        pTracking.exportTime = "Chưa xuất kho";


                    if (item.Quantity == 1 || item.Quantity == 0)
                    {
                        pTracking.status = Convert.ToInt32(item.Quantity) + " thùng";
                    }
                    else
                    {
                        int quantity = Convert.ToInt32(item.Quantity * item.ProductInfo.QuantityBox);
                        pTracking.status = quantity + " hộp";
                    }

                    productTracking.Add(pTracking);
                }

                result.tracking = productTracking;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                log.Sucess = 0;
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }
        #endregion

      
    }
}
