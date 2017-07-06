using NDHAPI.Models;
using NDHAPI.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace NDHAPI.Controllers
{
    public class SMSAPIController : ApiController
    {

        NDHDBEntities db = new NDHDBEntities();


        [HttpPost]
        public SMSResult haisms()
        {
            // update regid firebase
            // /api/smsapi/haisms
            var history = new APIHistory()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                APIUrl = "/api/smsapi/haisms",
                Sucess = 1
            };

            HttpRequestHeaders headers = Request.Headers;

            var authInfo = checkAuth(headers);
            if (authInfo.status == 0)
            {
                history.Sucess = 0;
                history.ReturnInfo = new JavaScriptSerializer().Serialize(authInfo);

                db.APIHistories.Add(history);
                db.SaveChanges();

                return authInfo;
            }
            else
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();


                SMSHistory smsHistory = new SMSHistory()
                {
                    CreateTime = DateTime.Now,
                    Id = Guid.NewGuid().ToString()
                };

                SMSResult result = new SMSResult()
                {
                    status = 0,
                    message = ""
                };
                history.Content = requestContent;


                try
                {
                    var paser = jsonserializer.Deserialize<SMSRequest>(requestContent);
                    SMSContent content = analysisContent(paser.content, paser.phone);

                    smsHistory.PhoneNumber = paser.phone;
                    smsHistory.ContentSend = paser.content;

                    if (content.status == 0)
                    {
                        result.status = 1;
                        // sai cu phap
                        result.message = "Cu phap nhan tin cua Quy Khach vua thuc hien khong dung. Chi tiet lien he NVTT hoac 1800577768";
                    }
                    else
                    {
                        if (content.isAgency)
                        {
                            smsHistory.AgencyType = "CII";
                            result = checkContent(content);
                        }          
                        else
                        {
                            smsHistory.AgencyType = "FARMER";
                            result = checkContentFarmer(content);
                        }
                           
                    }

                }
                catch (Exception e)
                {
                    result.status = 0;
                    result.message = e.Message;
                }


                smsHistory.ContentReturn = result.message;

                db.SMSHistories.Add(smsHistory);

                history.ReturnInfo = new JavaScriptSerializer().Serialize(result);

                history.Sucess = result.status;

                db.APIHistories.Add(history);
                db.SaveChanges();

                return result;

            }

        }


        private SMSContent analysisContent(string content, string phone)
        {
            SMSContent result = new SMSContent()
            {
                status = 0
            };


            var checkPhone = db.CInfoCommons.Where(p => p.Phone == phone).FirstOrDefault();

            if (checkPhone == null)
            {
                result.status = 2;
                // sai sdt
                return result;
            }

            var arr = content.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            if (arr.Count() <= 1)
            {
                result.status = 0;
                return result;
            }

            if (arr[0] != "HAI")
            {
                result.status = 0;
                return result;
            }
                


            if (checkPhone.CType == "CII")
            {
                result.status = 1;
                result.isAgency = true;
                result.phone = phone;

                if (arr.Count() < 3)
                {
                    result.status = 0;
                    return result;
                }

                List<string> series = new List<string>();
                for (int i = 2; i < arr.Count(); i++)
                {
                    series.Add(arr[i]);
                }

                result.products = series;
                result.code = arr[1];
                return result;

            }
            else if (checkPhone.CType == "FARMER")
            {
                result.status = 1;
                result.isAgency = false;
                result.phone = phone;


                if (arr.Count() < 2)
                {
                    result.status = 0;
                    return result;
                }

                List<string> series = new List<string>();
                for (int i = 1; i < arr.Count(); i++)
                {
                    series.Add(arr[i]);
                }

                result.products = series;
                result.code = "0000";

            }

            return result;
        }


        private SMSResult checkContent(SMSContent paser)
        {

            var result = new SMSResult()
            {
                status = 1
            };

            try
            {

                var c2 = db.C2Info.Where(p => p.Code == paser.code).FirstOrDefault();

                if (c2 == null)
                    return new SMSResult { status = 1, message = "Ma khach hang vua nhan tin khong ton tai trong he thong. Chi tiet lien he NVTT hoac 1800577768" };


                if (c2.CInfoCommon.Phone != paser.phone)
                {
                    return new SMSResult { status = 1, message = "So dien thoai Quy Khach vua nhan tin chua dang ky tham gia Chuong trinh nhan tin cung voi Cong ty HAI. Chi tiet lien he NVTT hoac 1800577768" };
                }

                var cInfo = c2.CInfoCommon;

                // lay danh sach sự kiện phù hợp với user (theo khu vực)
                List<EventInfo> listEventUser = new List<EventInfo>();

              //  var eventArea = db.EventAreas.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                var dateNow = DateTime.Now;
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

                if (listEventUser.Count() == 0)
                    return new SMSResult { status = 1, message = "Hien tai Cong ty HAI chua co Chuong trinh khuyen mai. Quy khach vui long theo doi tin nhan SMS hoac tin thong bao trong ung dung CSKH HAI. Xin cam on" };


                // kiem tra ma dung, ma sau
                List<ProductSeri> righCode = new List<ProductSeri>();

                foreach (var item in paser.products)
                {
                    // lây mã seri // eventCode or seri
                    var productSeri = db.ProductSeris.Where(p => p.Code == item && p.SeriType == 1).FirstOrDefault();

                    if (productSeri == null)
                    {
                         return new SMSResult { status = 4, message = "Ma " + item + " khong dung. Vui long kiem tra lai ma so nhan tin. Xin cam on" };
                    }
                    else if (productSeri.IsUse == 1)
                    {
                        return new SMSResult { status = 4, message = "Ma " + item + " da duoc su dung vao (" + productSeri.ModifyTime + "), Vui long kiem tra lai ma so nhan tin. Xin cam on" };
                    }
                    else
                        righCode.Add(productSeri);

                }

                Hashtable map = new Hashtable();

                List<string> productAttend = new List<string>();

                result.status = 5;

                foreach (var item in righCode)
                {
                    try
                    {
                        // cap nhat lịch su
                        var hEvent = new MSGPoint()
                        {
                            Id = Guid.NewGuid().ToString(),
                            AcceptTime = DateTime.Now,
                            CInfoId = cInfo.Id,
                            UserLogin = paser.code,
                            ProductId = item.ProductId,
                            Barcode = item.Code,
                            MSGType = "SMS"
                        };

                        List<MSGPointEvent> listPointEvent = new List<MSGPointEvent>();

                        foreach (var userEvent in listEventUser)
                        {

                            // kiem tra san pham co trong su kien nay ko
                            var productEvent = userEvent.EventProducts.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                            if (productEvent != null)
                            {
                                var pointEvemt = new MSGPointEvent()
                                {
                                    EventId = userEvent.Id,
                                    MSGPointId = hEvent.Id,
                                    Point = productEvent.Point
                                };
                                listPointEvent.Add(pointEvemt);

                            }
                        }
                        //

                        if (listPointEvent.Count() > 0)
                        {

                            if (!productAttend.Contains(item.ProductId))
                                productAttend.Add(item.ProductId);

                            item.IsUse = 1;
                            item.ModifyTime = DateTime.Now;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;

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
                //

                // phan qua
                string pointEvent = "";
                int countPoint = 0;
                foreach (var item in map.Keys)
                {

                    var eventInfo = db.EventInfoes.Find(item);

                    var savePoint = eventInfo.AgencySavePoints.Where(p => p.CInfoCommon.CCode == paser.code).FirstOrDefault();
                    int? point = 0;
                    if (savePoint != null)
                        point = savePoint.PointSave;

                    string aWard = "";
                    var listAward = eventInfo.AwardInfoes.OrderByDescending(p => p.Point).ToList();
                    foreach (var aWardItem in listAward)
                    {
                        if (aWardItem.Point <= point)
                        {
                            aWard = ConvertToUnsign3(aWardItem.Name);
                            break;
                        }
                    }

                    var nameEvent = ConvertToUnsign3(eventInfo.Name);

                    if (!String.IsNullOrEmpty(aWard))
                    {
                        pointEvent = " , " + aWard + " tu " + nameEvent;
                    }


                    countPoint += Convert.ToInt32(map[item]);
                }


                //


                string msgReturn = "MKH " + paser.code + " vua tich luy " + countPoint + " diem tu ";
                foreach (string item in productAttend)
                {
                    var productCheck = db.ProductInfoes.Find(item);
                    if (productCheck != null)
                        msgReturn += ConvertToUnsign3(productCheck.PName) + " ,";
                }

                msgReturn = msgReturn.Remove(msgReturn.Count() - 1, 1);

                msgReturn += ". Cam on quy khach da tham gia CT Ket noi cung phat trien cua cty CPND HAI. ";


                if (!String.IsNullOrEmpty(pointEvent))
                {
                    pointEvent = pointEvent.Remove(0, 2);
                    msgReturn += "Chuc mung MKH " + paser.code + " nhan duoc" + pointEvent + ". Cam on quy khach da tham gia CT Ket noi cung phat trien cua cty CPND HAI.";
                }

                result.message = msgReturn;

            }
            catch (Exception e)
            {
                result.status = 0;
                result.message = e.Message;
            }

            return result;

        }


        private SMSResult checkContentFarmer(SMSContent paser)
        {

            var result = new SMSResult()
            {
                status = 1
            };

            try
            {

                // so

                var cInfo = db.CInfoCommons.Where(p => p.Phone == paser.phone).FirstOrDefault();

                if (cInfo == null)
                    return new SMSResult { status = 1, message = "So dien thoai Quy Khach vua nhan tin chua dang ky tham gia Chuong trinh nhan tin cung voi Cong ty HAI. Chi tiet lien he NVTT hoac 1800577768" };

                if (cInfo.CType != "FARMER")
                    return new SMSResult { status = 1, message = "So dien thoai Quy Khach vua nhan tin chua dang ky tham gia Chuong trinh nhan tin cung voi Cong ty HAI. Chi tiet lien he NVTT hoac 1800577768" };


                // lay danh sach sự kiện phù hợp với user (theo khu vực)
                List<EventInfo> listEventUser = new List<EventInfo>();

                //var eventArea = db.EventAreaFarmers.Where(p => p.AreaId == cInfo.AreaId && p.EventInfo.ESTT == 1).ToList();
                var dateNow = DateTime.Now;
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



                if (listEventUser.Count() == 0)
                    return new SMSResult { status = 4, message = "Quy khach dang dung hang chinh hang. Cam on Quy khach da tin dung & ung ho hang cua cong ty CPND HAI" };

                // kiem tra ma dung, ma sau
                List<ProductSeri> righCode = new List<ProductSeri>();

                foreach (var item in paser.products)
                {
                    // lây mã seri // eventCode or seri
                    var productSeri = db.ProductSeris.Where(p => p.Code == item && p.IsUse == 0 && p.SeriType == 2).FirstOrDefault();


                    if (productSeri == null)
                    {
                        return new SMSResult { status = 4, message = "Ma " + item + " khong dung. Vui long kiem tra lai ma so nhan tin. Xin cam on" };
                    }
                    else if (productSeri.IsUse == 1)
                    {
                        return new SMSResult { status = 4, message = "Ma " + item + " da duoc su dung vao (" + productSeri.ModifyTime + "), Vui long kiem tra lai ma so nhan tin. Xin cam on" };
                    }
                    else
                        righCode.Add(productSeri);

                }

                Hashtable map = new Hashtable();

                List<string> productAttend = new List<string>();

                result.status = 5;

                foreach (var item in righCode)
                {
                    try
                    {

                        // cap nhat lịch su
                        var hEvent = new MSGPoint()
                        {
                            Id = Guid.NewGuid().ToString(),
                            AcceptTime = DateTime.Now,
                            CInfoId = cInfo.Id,
                            UserLogin = paser.code,
                            ProductId = item.ProductId,
                            Barcode = item.Code,
                            MSGType = "SMS"
                        };

                        List<MSGPointEvent> listPointEvent = new List<MSGPointEvent>();

                        foreach (var userEvent in listEventUser)
                        {

                            // kiem tra san pham co trong su kien nay ko
                            var productEvent = userEvent.EventProducts.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                            if (productEvent != null)
                            {
                                var pointEvemt = new MSGPointEvent()
                                {
                                    EventId = userEvent.Id,
                                    MSGPointId = hEvent.Id,
                                    Point = productEvent.Point
                                };
                                listPointEvent.Add(pointEvemt);

                            }
                        }
                        //

                        if (listPointEvent.Count() > 0)
                        {

                            if (!productAttend.Contains(item.ProductId))
                                productAttend.Add(item.ProductId);

                            item.IsUse = 1;
                            item.ModifyTime = DateTime.Now;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;

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
                //

                // phan qua
                string pointEvent = "";
                int countPoint = 0;
                foreach (var item in map.Keys)
                {

                    var eventInfo = db.EventInfoes.Find(item);

                    var savePoint = eventInfo.AgencySavePoints.Where(p => p.CInfoCommon.CCode == paser.code).FirstOrDefault();
                    int? point = 0;
                    if (savePoint != null)
                        point = savePoint.PointSave;

                    string aWard = "";
                    var listAward = eventInfo.AwardInfoes.OrderByDescending(p => p.Point).ToList();
                    foreach (var aWardItem in listAward)
                    {
                        if (aWardItem.Point <= point)
                        {
                            aWard = ConvertToUnsign3(aWardItem.Name);
                            break;
                        }
                    }

                    var nameEvent = ConvertToUnsign3(eventInfo.Name);

                    if (!String.IsNullOrEmpty(aWard))
                    {
                        pointEvent = " , " + aWard + " tu " + nameEvent;
                    }


                    countPoint += Convert.ToInt32(map[item]);
                }


                //
                string msgReturn = "Chuc mung Quy khach vua tich luy " + countPoint + " diem tu ";
                foreach (string item in productAttend)
                {
                    var productCheck = db.ProductInfoes.Find(item);
                    if (productCheck != null)
                        msgReturn += ConvertToUnsign3(productCheck.PName) + " ,";
                }

                msgReturn = msgReturn.Remove(msgReturn.Count() - 1, 1);

                msgReturn += ". Cam on Quy khach da tin dung & ung ho hang cua cong ty CPND HAI. ";


                if (!String.IsNullOrEmpty(pointEvent))
                {
                    pointEvent = pointEvent.Remove(0, 2);
                    msgReturn += "Chuc mung Quy khach nhan duoc " + pointEvent + ". Cam on Quy khach da tin dung & ung ho hang cua cong ty CPND HAI.";
                }

                result.message = msgReturn;

            }
            catch (Exception e)
            {
                result.status = 0;
                result.message = e.Message;
            }

            return result;

        }



        public string ConvertToUnsign3(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty)
                        .Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        private SMSResult checkAuth(HttpRequestHeaders headers)
        {
            if (!headers.Contains("Authorization"))
            {
                return new SMSResult() { status = 0, message = "Nead authorization info" };
            }

            string token;

            try
            {
                string base64Auth = headers.GetValues("Authorization").First().Replace("Basic", "").Trim();
                token = XString.FromBase64(base64Auth);
            }
            catch
            {
                return new SMSResult { status = 0, message = "Wrong authorization info" };
            }

            var arrtok = token.Split(':');
            if (arrtok.Length != 2)
                return new SMSResult { status = 0, message = "Wrong authorization format" };

            string UserName = arrtok[0];
            string PassWord = arrtok[1];


            if (UserName != "admin" || PassWord != "SMSHai2016")
            {
                return new SMSResult { status = 0, message = "Username or password worng" };
            }

            return new SMSResult { status = 1, message = "auth" };
        }

    }
}
