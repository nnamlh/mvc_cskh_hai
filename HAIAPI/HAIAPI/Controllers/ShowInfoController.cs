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
            var result = new List<ProductInfoResult>();
            if (!mongoHelper.checkLoginSession(user, token))
                return result;

            result = GetProductCodeInfo();

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }

        [HttpGet]
        public ProductDetailResult GetProductDetail(string user, string token, string id)
        {

            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/showinfo/getproductdetail",
                CreateTime = DateTime.Now,
                Sucess = 1
            };
            var result = new ProductDetailResult();
            if (!mongoHelper.checkLoginSession(user, token))
                return result;

            var find = db.procduct_item_detail(id).FirstOrDefault();

            if (find == null)
                return result;

            result = new ProductDetailResult()
            {
                id = find.Id,
                code = find.PCode,
                name = find.PName,
                activce = find.Activce,
                barcode = find.Barcode,
                commerceName = find.CommerceName,
                isForcus = find.Forcus,
                groupId = find.GroupId,
                groupName = find.GroupName,
                image = HaiUtil.HostName + find.Thumbnail,
                isNew = find.New,
                producer = find.Producer,
                describe = find.Describe,
                introduce = find.Introduce,
                notes = find.Notes,
                other = find.Other,
                register = find.Register,
                unit = find.Unit,
                uses = find.Uses,
                images = new List<string>()
            };

            var imges = db.ProductImages.Where(p => p.ProductId == result.id).ToList();

            foreach(var item in imges)
            {
                result.images.Add(HaiUtil.HostName + item.ImageUrl);
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }


        // trung bay



    }
}
