using HAIAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class StaffOrderController : RestMainController
    {

        //
        [HttpPost]
        public YourOrderResult Show()
        {
            //
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/stafforder/show",
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
                var paser = jsonserializer.Deserialize<StaffOrderRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                // 
                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Sai thong tin");


                int pageSize = 20;
                int pageNumber = (paser.page ?? 1);


                List<HaiOrder> data = new List<HaiOrder>();

                data = staff.OrderStaffs.Where(p => p.HaiOrder.CInfoCommon.CCode.Contains(paser.c2Code) && p.ProcessId == "create").Select(p => p.HaiOrder).OrderByDescending(p => p.CreateDate).ToPagedList(pageNumber, pageSize).ToList();

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
                        status = order.OrderStt.Name,
                        money = order.PriceTotal==null?0:order.PriceTotal
                    };

                    if (order.Sender == "B")
                    {
                        yourOrder.senderCode = order.BrachCode;
                        yourOrder.senderName = "Tại chi nhánh";
                    } else
                    {
                        yourOrder.senderName = order.C1Name;
                        yourOrder.senderCode = order.C1Code;
                    }

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

    }
}
