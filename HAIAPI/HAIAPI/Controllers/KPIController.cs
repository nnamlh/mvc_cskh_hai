using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HAIAPI.Models;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class KPIController : RestMainController
    {

        [HttpGet]
        public ResultCommonType ListKPITypes()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/kpi/listkpitypes",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new ResultCommonType()
            {
                id = "1",
                msg = "success",
                data = new List<IdentityCommon>()
            };

            var types = db.KPITypes.ToList();

            foreach(var item in types)
            {
                result.data.Add(new IdentityCommon()
                {
                    code = item.Id,
                    name = item.Title
                });
            }

            log.ReturnInfo = new JavaScriptSerializer().Serialize(result);
            mongoHelper.createHistoryAPI(log);

            return result;

        }

        [HttpPost]
        public StaffKPIResult StaffKPI()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/kpi/staffkpi",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new StaffKPIResult()
            {
                id = "1",
                msg = "success",
                data = new List<StaffKPIInfo>()
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<StaffKPIRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                //
                var data = db.StaffKPIs.Where(p => p.StaffId == staff.Id && p.TypeId == paser.type).OrderByDescending(p=>p.CreateTime).ToList();

                foreach(var item in data)
                {
                    result.data.Add(new StaffKPIInfo()
                    {
                        id = item.Id,
                        createTime = item.CreateTime.Value.ToString("dd/MM/yyyy"),
                        title = item.Title
                    });
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

        // detail
        [HttpPost]
        public StaffKPIDetailResult StaffKPIDetail()
        {
            var log = new MongoHistoryAPI()
            {
                APIUrl = "/api/kpi/staffkpidetail",
                CreateTime = DateTime.Now,
                Sucess = 1
            };

            var result = new StaffKPIDetailResult()
            {
                id = "1",
                msg = "success",
                data = new List<StaffKPIDetail>()
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<StaffKPIDetailRequest>(requestContent);
                log.Content = new JavaScriptSerializer().Serialize(paser);

                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Wrong token and user login!");

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (staff == null)
                    throw new Exception("Chỉ nhân viên công ty mới được quyền tạo");

                //
                var data = db.KPIDetails.Where(p => p.StaffKPIId == paser.kpiId).OrderBy(p => p.Number).ToList();

                foreach (var item in data)
                {
                    result.data.Add(new StaffKPIDetail()
                    {
                        title = item.KPIWork.Title,
                        percent = item.PlanPercent + "",
                        perform = item.Perform + "",
                        plan = item.PlanPoint + "",
                        point = item.PlanPoint + ""
                    });
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

    }
}
