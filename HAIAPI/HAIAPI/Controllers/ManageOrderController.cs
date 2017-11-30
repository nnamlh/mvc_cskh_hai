using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PagedList;
using System.Web.Script.Serialization;

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

                if (orders.Sender == "CI")
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
                        c1Address = c1Address,
                        c1Code = c1Code,
                        c1Id = c1Id,
                        c1Phone = c1Phone,
                        c1Store = c1Store,
                        perPrice = item.PerPrice,
                        price = item.PriceTotal,
                        quantityBox = item.ProductInfo.Quantity,
                        unit = item.ProductInfo.Unit
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
    }
}
