﻿using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class C1OrderController : RestMainController
    {

        [HttpPost]
        public C1OrderResult Show()
        {
            //
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/c1order/show",
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
                var paser = jsonserializer.Deserialize<C1OrderRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                // 
                var c1Info = db.C1Info.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();

                if (c1Info == null)
                    throw new Exception("Sai thong tin");


                int pageSize = 20;
                int pageNumber = (paser.page ?? 1);


                List<string> data = new List<string>();
                if (String.IsNullOrEmpty(paser.c2Code))
                {
                    data = c1Info.OrderProducts.Where(p => p.HaiOrder.OrderStatus == paser.status).OrderByDescending(p => p.HaiOrder.CreateDate).Select(p => p.OrderId).Distinct().ToList();
                }

                List<YourOrder> orders = new List<YourOrder>();

                foreach (var item in data)
                {
                    var order = db.HaiOrders.Find(item);
                    if (order != null)
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
                            phone = order.ReceivePhone1
                        };

                        var orderProduct = order.OrderProducts.Where(p => p.C1Id == c1Info.Id).ToList();
                        yourOrder.productCount = orderProduct.Count();

                        orders.Add(yourOrder);
                    }

                    result.orders = orders;
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


        #region danh sach san pham
        [HttpGet]
        public List<ProductOrderInfo> GetProduct(string user, string id)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/c1order/getproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new List<ProductOrderInfo>();

            var c1Info = db.C1Info.Where(p => p.CInfoCommon.UserLogin == user).FirstOrDefault();

            if (c1Info != null)
            {
                var data = db.OrderProducts.Where(p => p.OrderId == id && p.C1Id == c1Info.Id).ToList();

                foreach (var item in data)
                {
                    result.Add(new ProductOrderInfo()
                    {
                        orderId = item.OrderId,
                        productId = item.ProductId,
                        productName = item.ProductInfo.PName,
                        quantity = item.Quantity,
                        quantityFinish = item.QuantityFinish
                    });
                }
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(log);

            return result;

        }
        #endregion
        //
        [HttpGet]
        public List<ProductOrderHistory> OrderProductHistory(string orderId, string productId)
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/c1order/orderproducthistory",
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
                    notes = item.Notes
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
                var paser = jsonserializer.Deserialize<C1UpdateOrderRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var c1Info = db.C1Info.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();

                if (c1Info == null)
                    throw new Exception("Sai thong tin");

                var orderProduct = db.OrderProducts.Where(p => p.C1Id == c1Info.Id && p.ProductId == paser.productId && p.OrderId == paser.orderId).FirstOrDefault();
                if (orderProduct == null)
                    throw new Exception("Sai thong tin");

                // check quantity
                int? quantityRemain = orderProduct.Quantity - orderProduct.QuantityFinish;

                if (quantityRemain < paser.quantity)
                    throw new Exception("Sai thong tin");

                orderProduct.QuantityFinish = orderProduct.QuantityFinish + paser.quantity;

                db.Entry(orderProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // save history

                var history = new OrderProductHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.Now,
                    Notes = paser.notes,
                    OrderId = orderProduct.OrderId,
                    ProductId = orderProduct.ProductId,
                    Quantity = paser.quantity
                };

                db.OrderProductHistories.Add(history);
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
        #endregion
    }
}