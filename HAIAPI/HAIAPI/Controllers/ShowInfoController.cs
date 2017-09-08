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
    public class ShowInfoController : RestMainController
    {
        //
       

        [HttpGet]
        public List<ProductInfoResult> GetProduct(string user, string token)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/showinfo/getproduct",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            if (!mongoHelper.checkLoginSession(user, token))
                throw new Exception("Wrong token and user login!");

            var result = new List<ProductInfoResult>();
            var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();

            if (staff == null)
                throw new Exception("Chỉ nhân viên công ty mới được quyền truy cập");

            result = GetProductCodeInfo();

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }
    }
}
