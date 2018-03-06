using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PagedList;
using System.Web.Script.Serialization;
using HAIAPI.Util;

namespace HAIAPI.Controllers
{
    public class ManageOrderController : RestMainController
    {

        //
        [HttpPost]
        public YourOrderResult ShowOrderC2()
        {
            //
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/manageorder/showorderc2",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new YourOrderResult()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<C2OrderRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                // 
                var cInfo = db.C2Info.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();

                if (cInfo == null)
                    throw new Exception("Sai thong tin");


                int pageSize = 20;
                int pageNumber = (paser.page ?? 1);


                List<HaiOrder> data = new List<HaiOrder>();

                data = db.HaiOrders.Where(p => p.Agency.Contains(cInfo.Code)).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize).ToList();

                List<YourOrder> orders = new List<YourOrder>();

                foreach (var order in data)
                {
                    YourOrder yourOrder = new YourOrder()
                    {
                        address = order.ReceiveAddress,
                        c2Code = order.CInfoCommon.CCode,
                        c2Name = order.CInfoCommon.CName,
                        code = order.Code,
                        date = order.CreateDate.Value.ToString("dd/MM/yyyy"),
                        dateSuggest = order.ExpectDate == null ? "" : order.ExpectDate.Value.ToString("dd/MM/yyyy"),
                        orderId = order.Id,
                        phone = order.ReceivePhone1,
                        status = order.OrderStt.Name
                    };
                    yourOrder.productCount = order.OrderProducts.Count();
                    orders.Add(yourOrder);
                }
                result.orders = orders;
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

        //
        [HttpGet]
        public List<ProductOrderHistory> OrderProductHistory(string orderId, string productId)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/manageorder/orderproducthistory",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<ProductOrderHistory>();

            var data = db.OrderProductHistories.Where(p => p.OrderId == orderId && p.ProductId == productId).OrderByDescending(p => p.CreateDate).ToList();

            foreach (var item in data)
            {
                result.Add(new ProductOrderHistory()
                {
                    date = item.CreateDate.Value.ToString("dd/MM/yyyy"),
                    quantity = item.Quantity,
                    notes = item.Notes,
                    quantityBox = item.ProductInfo.Quantity,
                    unit = item.ProductInfo.Unit
                });
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(log);

            return result;
        }

        #region
        [HttpPost]
        public ResultInfo UpdateOrderProduct()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/c1order/updateorderproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new C1OrderResult()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<UpdateOrderRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Sai thong tin");

                var orderProduct = db.OrderProducts.Where(p => p.ProductId == paser.productId && p.OrderId == paser.orderId).FirstOrDefault();
                if (orderProduct == null)
                    throw new Exception("Sai thong tin");

                if (orderProduct.HaiOrder.OrderStatus != "process")
                {
                    throw new Exception("Đơn hàng không thể cập nhật");
                }

                // check quantity
                int? quantityRemain = orderProduct.Quantity - orderProduct.QuantityFinish;

                if (quantityRemain < paser.quantity)
                    throw new Exception("Số lượng giao vượt quá");

                orderProduct.QuantityFinish = orderProduct.QuantityFinish + paser.quantity;

                db.Entry(orderProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save history

                var history = new OrderProductHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.Now,
                    Notes = "Nhân viên HAI: " + staff.FullName + "(" + staff.Code+ ") đã cập nhật" ,
                    OrderId = orderProduct.OrderId,
                    ProductId = orderProduct.ProductId,
                    Quantity = paser.quantity
                };

                db.OrderProductHistories.Add(history);
                db.SaveChanges();

                // gui thong bao
                var order = db.HaiOrders.Find(paser.orderId);
                if (order != null)
                {
                    var staffCreate = order.OrderStaffs.Where(p => p.ProcessId == "create").FirstOrDefault();
                    // nhan vien
                    if (staff != null)
                        HaiUtil.SendNotifi("Đơn hàng " + order.Code, "Cửa hàng vừa giao hàng với số lượng : " + paser.quantity + " " + orderProduct.ProductInfo.Unit, staffCreate.HaiStaff.UserLogin, db, mongoHelper);

                    // c2
                    HaiUtil.SendNotifi("Đơn hàng " + order.Code, "Cửa hàng vừa giao hàng với số lượng : " + paser.quantity + " " + orderProduct.ProductInfo.Unit, order.CInfoCommon.UserLogin, db, mongoHelper);
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
        #endregion

        #region danh sach san pham
        [HttpGet]
        public List<ProductOrderInfo> GetProduct(string user, string id)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/manageorder/getproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<ProductOrderInfo>();

            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();


            try
            {
                var orders = db.HaiOrders.Find(id); 

                if (staff == null && orders == null)
                    throw new Exception();

                string c1Address = "";
                string c1Code = "";
                string c1Id = "";
                string c1Phone = "";
                string c1Store = "";

                if (orders.SalePlace == "CI")
                {
                    var checkC1 = db.C1Info.Find(orders.C1Id);

                    if (checkC1 != null)
                    {
                        c1Address = checkC1.CInfoCommon.AddressInfo + ", " + checkC1.CInfoCommon.DistrictName + ", " + checkC1.CInfoCommon.ProvinceName;
                        c1Code = checkC1.Code;
                        c1Id = checkC1.Id;
                        c1Phone = checkC1.CInfoCommon.Phone;
                        c1Store = checkC1.StoreName;
                    }
                } else
                {
                    var branch = db.HaiBranches.Where(p => p.Code == orders.BrachCode).FirstOrDefault();
                    if(branch != null)
                    {
                        c1Address = branch.AddressInfo;
                        c1Code = branch.Code;
                        c1Id = branch.Id;
                        c1Phone = branch.Phone;
                        c1Store = branch.Name;
                    }
                }

                foreach (var item in orders.OrderProducts)
                {
                    result.Add(new ProductOrderInfo()
                    {
                        orderId = item.OrderId,
                        productId = item.ProductId,
                        productName = item.ProductInfo.PName,
                        quantity = item.Quantity,
                        quantityFinish = item.QuantityFinish,
   
                        perPrice = item.PerPrice,
                        price = item.PriceTotal,
                        quantityBox = item.ProductInfo.Quantity,
                        unit = item.ProductInfo.Unit,
                        hasBill = item.HasBill
                    });
                }

            } catch
            {
                result = new List<ProductOrderInfo>();
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(log);

            return result;

        }
        #endregion


        #region cap nhat giao du
        [HttpPost]
        public UpdateDeliveryCompleteResult CompleteDelivery()
        {
             //
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/manageorder/completedelivery",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new UpdateDeliveryCompleteResult()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<UpdateDeliveryCompleteRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                HaiStaff staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                if (staff == null)
                    throw new Exception("Sai thong tin nguoi dat");

                var checkOrder = db.HaiOrders.Where(p => p.Code == paser.orderId).FirstOrDefault();

                if (checkOrder == null)
                    throw new Exception("Sai thông tin");

                var product = checkOrder.OrderProducts;
                result.products = new List<ProductOrderInfo>();

                foreach (var item in product)
                {
                    item.ModifyDate = DateTime.Now;
                    item.QuantityFinish = item.Quantity;
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    result.products.Add(new ProductOrderInfo()
                    {
                        orderId = item.OrderId,
                        productId = item.ProductId,
                        productName = item.ProductInfo.PName,
                        quantity = item.Quantity,
                        quantityFinish = item.QuantityFinish,

                        perPrice = item.PerPrice,
                        price = item.PriceTotal,
                        quantityBox = item.ProductInfo.Quantity,
                        unit = item.ProductInfo.Unit
                    });
                }

                checkOrder.DStatus = "complete";

                db.Entry(checkOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                result.deliveryStatus = checkOrder.DeliveryStatu.Name;
                result.deliveryStatusCode = checkOrder.DeliveryStatu.Id;


                // update process: nhan vien khoi tao
                OrderStaff orderStaff = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = checkOrder.Id,
                    Notes = "Cập nhật giao đủ hàng",
                    ProcessId = "deliverycomplete",
                    StaffId = staff.Id
                };

                db.OrderStaffs.Add(orderStaff);
                db.SaveChanges();

                // gui thong bao
                // nhan vien
                HaiUtil.SendNotifi("Đơn hàng " + checkOrder.Code, "Cập nhật giao hàng đủ", staff.UserLogin, db, mongoHelper);

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

        #region giao hang
        [HttpGet]
        public UpdateDeliveryResult UpdateDelivery(string productId, string orderId, int quantity, string user, string token)
        {

                var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/manageorder/updatedelivery",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

                var result = new UpdateDeliveryResult()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                if (!mongoHelper.checkLoginSession(user, token))
                    throw new Exception("Wrong token and user login!");

                HaiStaff staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                if (staff == null)
                    throw new Exception("Sai thong tin nguoi dat");


                var checkOrderProduct = db.OrderProducts.Where(p => p.OrderId == orderId && p.ProductId == productId).FirstOrDefault();

                if(checkOrderProduct == null)
                    throw new Exception("Sai thong tin");


                checkOrderProduct.QuantityFinish = quantity;

                checkOrderProduct.ModifyDate = DateTime.Now;

                db.Entry(checkOrderProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


                //
                var order = checkOrderProduct.HaiOrder;
                string deliveryStt = GetDeliveryStatus(order);
                var status = db.DeliveryStatus.Find(deliveryStt);
                order.DStatus = deliveryStt;

                result.deliveryStatus = status.Name;
                result.deliveryStatusCode = status.Id;
                result.finish = quantity;

                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


                //
                // update process: nhan vien khoi tao
                OrderStaff orderStaff = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = order.Id,
                    Notes = "Cập nhật hàng",
                    ProcessId = "updatedelivery",
                    StaffId = staff.Id
                };

                db.OrderStaffs.Add(orderStaff);
                db.SaveChanges();

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

 
        private string GetDeliveryStatus(HaiOrder order)
        {

            int? totalOrder = 0;
            int? totalDelivery = 0;
            var products = order.OrderProducts;

            foreach (var item in products)
            {
                totalOrder += item.Quantity;
                totalDelivery += item.QuantityFinish;
            }

            if (totalDelivery == 0)
                return "incomplete";

            if (totalDelivery == totalOrder)
                return "complete";

            if (totalOrder > totalDelivery)
                return "less";

            if (totalOrder < totalDelivery)
                return "more";

            return "incomplete";
        }

        #endregion

    }
}
