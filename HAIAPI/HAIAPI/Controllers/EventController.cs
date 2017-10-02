using HAIAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace HAIAPI.Controllers
{
    public class EventController : RestMainController
    {
        #region Methob loyaltyEvent
        /// <summary>
        /// khuyen mai
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultEvent LoyaltyEvent()
        {
            // get all event
            // /api/rest/loyaltyevent
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/event/loyaltyevent",
                Sucess = 1
            };

            var result = new ResultEvent()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestInfo>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                var staff = db.HaiStaffs.Where(p => p.UserLogin == paser.user).FirstOrDefault();
                var dateNow = DateTime.Now;
                if (cInfo != null)
                {
                    // lay danh sach sự kiện phù hợp với user (theo khu vực)
                    List<EventInfo> listEventUser = new List<EventInfo>();
                    if (cInfo.CType == "CII")
                    {
                        var eventArea = (from log in db.EventAreas
                                         where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                       <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                       >= DbFunctions.TruncateTime(dateNow))
                                         select log).ToList();
                        foreach (var item in eventArea)
                        {
                            var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                            if (cusJoin.Count() > 0)
                            {
                                var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                                if (cJoin != null)
                                    listEventUser.Add(item.EventInfo);
                            }
                            else
                            {
                                listEventUser.Add(item.EventInfo);
                            }
                        }

                    }
                    else if (cInfo.CType == "FARMER")
                    {
                        var eventArea = (from log in db.EventAreaFarmers
                                         where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                       <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                       >= DbFunctions.TruncateTime(dateNow))
                                         select log).ToList();

                        foreach (var item in eventArea)
                        {
                            var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                            if (cusJoin.Count() > 0)
                            {
                                var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                                if (cJoin != null)
                                    listEventUser.Add(item.EventInfo);
                            }
                            else
                            {
                                listEventUser.Add(item.EventInfo);
                            }
                        }
                    }


                    List<Event> listEventInfo = new List<Event>();

                    foreach (var item in listEventUser)
                    {
                        listEventInfo.Add(new Event()
                        {
                            eid = item.Id,
                            ename = item.Name,
                            etime = item.BeginTime.Value.ToShortDateString() + " - " + item.EndTime.Value.ToShortDateString(),
                            eimage = HaiUtil.HostName + item.Thumbnail
                        });
                    }

                    result.events = listEventInfo;
                }
                else if (staff != null)
                {

                    var listEvent = db.EventAreas.Where(p => p.EventInfo.ESTT == 1 && p.AreaId == staff.HaiBranch.AreaId).ToList().OrderByDescending(p => p.EventInfo.BeginTime);

                    List<Event> listEventInfo = new List<Event>();

                    foreach (var item in listEvent)
                    {
                        listEventInfo.Add(new Event()
                        {
                            eid = item.EventInfo.Id,
                            ename = item.EventInfo.Name,
                            etime = item.EventInfo.BeginTime.Value.ToShortDateString() + " - " + item.EventInfo.EndTime.Value.ToShortDateString(),
                            eimage = HaiUtil.HostName + item.EventInfo.Thumbnail
                        });
                    }

                    result.events = listEventInfo;

                }
                else throw new Exception("Tài khoản không thuộc của HAI.");
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            mongoHelper.createHistoryAPI(history);
            return result;
        }
        #endregion

        #region Methob eventDetail
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public EventDetail EventDetail()
        {
            // get event detail
            // /api/rest/eventDetail
            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/event/eventdetail",
                Sucess = 1
            };

            var result = new EventDetail()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<EventDetailRequest>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var eventInfo = db.EventInfoes.Find(paser.eventId);

                if (eventInfo == null)
                    throw new Exception("Thông tin gửi lên không chính xác.");


                List<ResultEventAward> awards = new List<ResultEventAward>();

                List<string> areas = new List<string>();

                List<ResultEventProduct> products = new List<ResultEventProduct>();

                var listProduct = db.EventProducts.Where(p => p.EventId == eventInfo.Id).ToList();

                foreach (var item in listProduct)
                {
                    products.Add(new ResultEventProduct()
                    {
                        name = item.ProductInfo.PName,
                        point = item.Point + ""
                    });
                }

                var listArea = db.EventAreas.Where(p => p.EventId == eventInfo.Id).ToList();
                foreach (var item in listArea)
                {
                    areas.Add(item.HaiArea.Name);
                }

                var listAward = eventInfo.AwardInfoes.ToList();
                foreach (var item in listAward)
                {
                    awards.Add(new ResultEventAward()
                    {
                        name = item.Name,
                        point = item.Point + "",
                        image = HaiUtil.HostName + item.Thumbnail
                    });
                }


                result.eid = eventInfo.Id;
                result.eimage = HaiUtil.HostName + eventInfo.Thumbnail;
                result.ename = eventInfo.Name;
                result.etime = eventInfo.BeginTime.Value.ToShortDateString() + " - " + eventInfo.EndTime.Value.ToShortDateString();
                result.edescribe = eventInfo.Descibe;
                result.areas = areas;
                result.awards = awards;
                result.products = products;
            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            mongoHelper.createHistoryAPI(history);
            return result;
        }
        #endregion

        #region Methob sendCodeEvent
        /// <summary>
        /// ------------------------------------------------------
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultCodeEvent SendCodeEvent()
        {
            // send code 
            // /api/rest/sendcodeevent

            var history = new MongoHistoryAPI()
            {
                CreateTime = DateTime.Now,
                APIUrl = "/api/event/sendcodeevent",
                Sucess = 1
            };

            var result = new ResultCodeEvent()
            {
                id = "1"
            };

            var requestContent = Request.Content.ReadAsStringAsync().Result;

            history.Content = requestContent;

            try
            {
                var jsonserializer = new JavaScriptSerializer();
                var paser = jsonserializer.Deserialize<RequestCodeEvent>(requestContent);
                history.Content = new JavaScriptSerializer().Serialize(paser);
                if (!mongoHelper.checkLoginSession(paser.user, paser.token))
                    throw new Exception("Tài khoản bạn đã đăng nhập ở thiết bị khác.");

                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == paser.user).FirstOrDefault();

                if (cInfo == null)
                    throw new Exception("Chương trình chỉ dành cho khách hàng của HAI.");

                if (cInfo.CType == "CI")
                    throw new Exception("Chương trình chỉ dành cho khách hàng là đại lý cấp 2, nông dân của HAI.");


                var dateNow = DateTime.Now;
                // lay danh sach sự kiện phù hợp với user (theo khu vực)
                List<EventInfo> listEventUser = new List<EventInfo>();
                if (cInfo.CType == "CII")
                {
                    // var eventArea = db.EventAreas.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                    var eventArea = (from log in db.EventAreas
                                     where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                   <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                   >= DbFunctions.TruncateTime(dateNow))
                                     select log).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            //var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id && p.IsJoint == 0).FirstOrDefault();
                            //  if (cJoin == null)
                            listEventUser.Add(item.EventInfo);
                        }
                    }

                }
                else if (cInfo.CType == "FARMER")
                {
                    // var eventArea = db.EventAreaFarmers.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                    var eventArea = (from log in db.EventAreaFarmers
                                     where log.AreaId == cInfo.AreaId && log.EventInfo.ESTT == 1 && (DbFunctions.TruncateTime(log.EventInfo.BeginTime)
                                                   <= DbFunctions.TruncateTime(dateNow) && DbFunctions.TruncateTime(log.EventInfo.EndTime)
                                                   >= DbFunctions.TruncateTime(dateNow))
                                     select log).ToList();

                    foreach (var item in eventArea)
                    {
                        var cusJoin = db.EventCustomerFarmers.Where(p => p.EventId == item.EventId && p.CInfoCommon.AreaId == item.AreaId).ToList();

                        if (cusJoin.Count() > 0)
                        {
                            var cJoin = cusJoin.Where(p => p.CInfoId == cInfo.Id).FirstOrDefault();
                            if (cJoin != null)
                                listEventUser.Add(item.EventInfo);
                        }
                        else
                        {
                            listEventUser.Add(item.EventInfo);
                        }
                    }

                }


                if (listEventUser.Count() == 0)
                    throw new Exception("Không tìm thấy chương trình khuyến mãi nào phù hợp.");


                List<GeneralInfo> listCodeReturn = new List<GeneralInfo>();

                Hashtable map = new Hashtable();

                foreach (var item in paser.codes)
                {
                    try
                    {
                        // lây mã seri // eventCode or seri
                        var product = GetProduct(item);

                        if (product == null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = "Không phải sản phẩm HAI",
                                code = item,
                                status = "Mã không hợp lệ"
                            });
                            throw new Exception();
                        }

                        var caseCode = item.Substring(0, 15);
                        var boxCode = item.Substring(0, 16);

                        // kiem tra la da nhap kho chua
                        PHistory pHis = null;
                        if (product.IsBox == 1)
                            pHis = db.PHistories.Where(p => p.BoxCode == boxCode && p.WCode == cInfo.CCode && p.PStatus == "NK").FirstOrDefault();
                        else
                            pHis = db.PHistories.Where(p => p.CaseCode == caseCode && p.WCode == cInfo.CCode && p.PStatus == "NK").FirstOrDefault();

                        if (pHis == null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = product.PName,
                                code = item,
                                status = "Mã chưa nhập kho"
                            });
                            throw new Exception();
                        }

                        // kiem tra la da su dung chua

                        var checkUse = db.MSGPoints.Where(p => p.Barcode == item).FirstOrDefault();
                        if (checkUse != null)
                        {
                            listCodeReturn.Add(new GeneralInfo()
                            {
                                name = product.PName,
                                code = item,
                                status = "Mã đã được sử dụng"
                            });
                            throw new Exception();
                        }

                        // cap nhat lịch su
                        var hEvent = new MSGPoint()
                        {
                            Id = Guid.NewGuid().ToString(),
                            AcceptTime = DateTime.Now,
                            CInfoId = cInfo.Id,
                            UserLogin = paser.user,
                            ProductId = product.Id,
                            Barcode = item,
                            MSGType = "APP"
                        };


                        List<MSGPointEvent> listPointEvent = new List<MSGPointEvent>();
                        foreach (var userEvent in listEventUser)
                        {

                            // kiem tra san pham co trong su kien nay ko
                            var productEvent = userEvent.EventProducts.Where(p => p.ProductId == product.Id).FirstOrDefault();
                            if (productEvent != null)
                            {

                                var pointEvemt = new MSGPointEvent()
                                {
                                    EventId = userEvent.Id,
                                    MSGPointId = hEvent.Id,
                                    Point = productEvent.Point
                                };
                                listPointEvent.Add(pointEvemt);

                                // tra thong tin ve client
                                listCodeReturn.Add(new GeneralInfo()
                                {
                                    code = item,
                                    name = product.PName,
                                    status = productEvent.EventInfo.Name + " + " + productEvent.Point + "***"
                                });

                            }
                        }
                        //

                        if (listPointEvent.Count() > 0)
                        {

                            db.MSGPoints.Add(hEvent);

                            db.SaveChanges();

                            foreach (var pevent in listPointEvent)
                            {

                                if (map.ContainsKey(pevent.EventId))
                                {
                                    var oldPoint = Convert.ToInt32(map[pevent.EventId]);
                                    map[pevent.EventId] = oldPoint + pevent.Point;
                                }
                                else
                                {
                                    map.Add(pevent.EventId, Convert.ToInt32(pevent.Point));
                                }

                                db.MSGPointEvents.Add(pevent);

                                // luu diem cho khach hang
                                var agencyPoint = db.AgencySavePoints.Where(p => p.EventId == pevent.EventId && p.CInfoId == cInfo.Id).FirstOrDefault();

                                if (agencyPoint == null)
                                {
                                    var newAgencyPoint = new AgencySavePoint()
                                    {
                                        EventId = pevent.EventId,
                                        CInfoId = cInfo.Id,
                                        PointSave = pevent.Point,
                                        CreateTime = DateTime.Now
                                    };
                                    db.AgencySavePoints.Add(newAgencyPoint);
                                }
                                else
                                {
                                    var newPoint = agencyPoint.PointSave + pevent.Point;
                                    agencyPoint.PointSave = newPoint;
                                    agencyPoint.ModifyTime = DateTime.Now;
                                    db.Entry(agencyPoint).State = System.Data.Entity.EntityState.Modified;
                                }

                                db.SaveChanges();
                            }
                        }



                    }
                    catch
                    {

                    }
                }

                string pointEvent = "";
                int countPoint = 0;
                foreach (var item in map.Keys)
                {
                    var eventInfo = db.EventInfoes.Find(item);

                    var savePoint = eventInfo.AgencySavePoints.Where(p => p.CInfoCommon.UserLogin == paser.user).FirstOrDefault();
                    int? point = 0;
                    if (savePoint != null)
                        point = savePoint.PointSave;

                    string aWard = "";
                    var listAward = eventInfo.AwardInfoes.ToList();
                    foreach (var aWardItem in listAward)
                    {
                        if (aWardItem.Point <= point)
                        {
                            aWard += ", " + aWardItem.Name + "(" + aWardItem.Point + "điểm )";
                        }
                    }

                    var nameEvent = eventInfo.Name;

                    pointEvent = pointEvent + ";" + nameEvent + " (" + map[item] + " điểm)";

                    if (!String.IsNullOrEmpty(aWard))
                    {
                        aWard = aWard.Remove(0, 2);
                        pointEvent = pointEvent + " - phần thưởng đổi: " + aWard;
                    }
                    else
                    {
                        pointEvent = pointEvent + "-chưa đủ điểm đổi quà";
                    }

                    countPoint += Convert.ToInt32(map[item]);
                }
                string msgReturn = "Cộng " + countPoint;

                msgReturn = msgReturn + pointEvent;

                result.msg = msgReturn;

                result.codes = listCodeReturn;

            }
            catch (Exception e)
            {
                result.id = "0";
                result.msg = e.Message;
                history.Sucess = 0;
            }

            mongoHelper.createHistoryAPI(history);
            return result;

        }
        #endregion

    }
}
