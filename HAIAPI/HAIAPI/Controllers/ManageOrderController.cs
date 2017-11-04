using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class ManageOrderController : RestMainController
    {
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

            if (staff != null)
            {
                var data = db.OrderProducts.Where(p => p.OrderId == id).ToList();

                foreach (var item in data)
                {
                    result.Add(new ProductOrderInfo()
                    {
                        orderId = item.OrderId,
                        productId = item.ProductId,
                        productName = item.ProductInfo.PName,
                        quantity = item.Quantity,
                        quantityFinish = item.QuantityFinish,
                        c1Address = item.C1Info.CInfoCommon.AddressInfo,
                        c1Code = item.C1Info.Code,
                        c1Id = item.C1Id,
                        c1Phone = item.C1Info.CInfoCommon.Phone,
                        c1Store = item.C1Info.StoreName,
                        perPrice = item.PerPrice,
                        price = item.PriceTotal,
                        quantityBox = item.ProductInfo.Quantity,
                        unit = item.ProductInfo.Unit
                    });
                }
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);

            mongoHelper.createHistoryAPI(log);

            return result;

        }
        #endregion
    }
}
