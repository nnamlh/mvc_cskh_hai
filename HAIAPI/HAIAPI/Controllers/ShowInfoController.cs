using HAIAPI.Models;
using HAIAPI.Util;
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
            /*
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
                barcode = find.Barcode,
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
                unit = find.Unit,
                images = new List<string>()
            };

            var imges = db.ProductImages.Where(p => p.ProductId == result.id).ToList();

            foreach (var item in imges)
            {
                result.images.Add(HaiUtil.HostName + item.ImageUrl);
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);
            */
            return result;

        }


        // kiem tra nhan vien

        #region Methoh checkStaff
        /// <summary>
        /// kiem tra nhan vien
        /// </summary>
        /// <param name="code"></param>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [HttpGet]
        public CheckStaffResult CheckStaff(string code, string user, string token)
        {
            // check sesion for login
            // /api/rest/loginsession
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/showinfo/checkstaff",
                Sucess = 1,
                Content = "code : " + code + " ; user : " + user + " token : " + token
            };

            var result = new CheckStaffResult()
            {
                id = "1"
            };

            if (!mongoHelper.checkLoginSession(user, token))
            {
                result.id = "0";
                result.msg = "Tài khoản bạn đã đăng nhập ở thiết bị khác.";
                history.Sucess = 0;
            }
            else
            {
                var staff = db.HaiStaffs.Where(p => p.Code == code).FirstOrDefault();

                if (staff == null)
                {
                    result.id = "0";
                    result.msg = "Không tìm thấy nhân viên này.";
                    history.Sucess = 0;
                }
                else
                {
                    result.avatar = HaiUtil.HostName + staff.AvatarUrl;
                    result.signature = HaiUtil.HostName + staff.SignatureUrl;

                    result.id = "1";
                    result.msg = "success";
                    result.status = staff.Notes;
                }
            }

            history.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(history);

            return result;
        }
        #endregion

    }
}
