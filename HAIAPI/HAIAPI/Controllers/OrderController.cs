using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HAIAPI.Models;
using HAIAPI.Util;
using System.Web.Script.Serialization;
using System.Data.Entity;

namespace HAIAPI.Controllers
{
    public class OrderController : RestMainController
    {
        [HttpPost]
        public OrderConfirm Confirm()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/order/confirm",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new OrderConfirm()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<OrderConfirmRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                // check C2
                C2Info c2 = db.C2Info.Where(p => p.Code == paser.agency).FirstOrDefault();

                if (c2 == null)
                    throw new Exception("Sai thông tin khách hàng");

                if (c2.IsActive == 0)
                    throw new Exception("Khách hàng đang tạm khóa");

                result.agencyCode = c2.Code;
                result.agencyId = c2.Id;
                result.store = c2.StoreName;
                result.deputy = c2.Deputy;
                result.phone = c2.CInfoCommon.Phone;
                result.address = c2.CInfoCommon.AddressInfo ;


                // lay danh sach type
                var payType = db.PayTypes.ToList();
                List<IdentityCommon> paytypeAll = new List<IdentityCommon>();
                foreach (var item in payType)
                {
                    paytypeAll.Add(new IdentityCommon()
                    {
                        code = item.Id,
                        name = item.Name
                    });
                }

                result.payType = paytypeAll;

                // 
                var shipType = db.ShipTypes.ToList();
                List<IdentityCommon> shipTypeAll = new List<IdentityCommon>();
                foreach (var item in shipType)
                {
                    shipTypeAll.Add(new IdentityCommon()
                    {
                        code = item.Id,
                        name = item.Name
                    });
                }
                result.shipType = shipTypeAll;

                // danh sach khuyen mai
                result.events = new List<EventOrderInfo>();
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
        public ResultInfo StaffComplete()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/order/staffcomplete",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultInfo()
            {
                id = "1",
                msg = "success"
            };
            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<OrderInfoRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");
                   

                DateTime dateSuggest = DateTime.ParseExact(paser.timeSuggest, "d/M/yyyy", null);

                CInfoCommon cinfo = db.CInfoCommons.Where(p => p.CCode == paser.code).FirstOrDefault();

                HaiStaff staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                string orderType = "";
                if (paser.inCheckIn == 1)
                    orderType = "checkinorder";
                else if (paser.inCheckIn == 0)
                    orderType = "order";

                if (String.IsNullOrEmpty(orderType))
                    throw new Exception("Sai thong tin dat hang");

                if (staff == null)
                    throw new Exception("Sai thong tin nguoi dat");

                //
                if (paser.product == null || paser.product.Count() == 0)
                    throw new Exception("Thieu thong tin san pham");

                if (cinfo == null)
                    throw new Exception("Sai thong tin khach hang");

                // create code
                int? number = GetOrderNumber(cinfo.BranchCode);
                string code = cinfo.BranchCode + (100000 + number);


                // tạo đơn hàng
                var order = new HaiOrder()
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderType = "checkinorder",
                    ShipType = paser.shipType,
                    PayType = paser.payType,
                    Agency = cinfo.Id,
                    CreateDate =DateTime.Now,
                    OrderStatus = "begin",
                    ReceiveAddress = paser.address,
                    Notes = paser.notes,
                    ExpectDate = dateSuggest,
                    BrachCode = cinfo.BranchCode,
                    Code = code,
                    OrderNumber = number,
                    ReceivePhone1 = paser.phone
                };
                db.HaiOrders.Add(order);
                db.SaveChanges();

                // danh sach san pham mua
                double? priceTotal = 0;
                foreach(var item in paser.product)
                {
                    // kiem tra san pham
                    var checkProduct = db.ProductInfoes.Find(item.code);
                    var checkC1 = db.C1Info.Where(p => p.Code == item.c1).FirstOrDefault();
                    if (checkProduct != null && item.quantity > 0 && checkC1 != null)
                    {
                        double? perPrice = checkProduct.Price != null ? checkProduct.Price : 0;
                        double? price = perPrice * item.quantity;
                        var productOrder = new OrderProduct()
                        {
                            OrderId = order.Id,
                            C1Id = checkC1.Id,
                            ModifyDate = DateTime.Now,
                            PerPrice = checkProduct.Price,
                            Quantity = item.quantity,
                            ProductId = checkProduct.Id,
                            PriceTotal = price,
                            QuantityFinish = 0
                        };
                        db.OrderProducts.Add(productOrder);
                        db.SaveChanges();
                        priceTotal += price;
                    }
                }

                if (priceTotal == 0)
                {
                    db.HaiOrders.Remove(order);
                    db.SaveChanges();

                    throw new Exception("Sai thong tin san pham (ma san pham) hoac so luong");
                } else
                {
                    order.PriceTotal = priceTotal;
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                }


                // update process: nhan vien khoi tao
                OrderStaff orderStaff = new OrderStaff()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now,
                    OrderId = order.Id,
                    Notes = "Khoi tao",
                    ProcessId = "create",
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

        //
        private int? GetOrderNumber(string branch)
        {
            //
            int? number = db.HaiOrders.Where(p => p.BrachCode == branch).Max(p => p.OrderNumber);
            if (number == null)
                number = 0;

            number++;

           
            return number;
        }

        // show danh sach don hang
        #region lay danh sách don hang
        //lam sao
        
        #endregion

    }
}
