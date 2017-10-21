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
        public OrderInitialize Confirm()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/order/confirm",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new OrderInitialize()
            {
                id = "1",
                msg = "success"
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<OrderInitializeRequest>(requestContent);
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
                result.address = c2.CInfoCommon.AddressInfo + " , " + c2.CInfoCommon.DistrictName + " , " + c2.CInfoCommon.ProvinceName;


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
    }
}
