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


    }
}
