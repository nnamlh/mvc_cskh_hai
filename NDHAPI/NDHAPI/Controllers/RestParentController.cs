using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NDHAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NDHAPI.Controllers
{
    public class RestParentController : ApiController
    {
        protected NDHDBEntities db = new NDHDBEntities();

        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));


        protected List<AgencyInfoC2Result> getListAgency(HaiStaff staff)
        {
            List<AgencyInfoC2Result> agencyResult = new List<AgencyInfoC2Result>();
            List<C2Info> c2List = new List<C2Info>();
            c2List = staff.C2Info.Where(p => p.IsActive == 1).OrderByDescending(p => p.CInfoCommon.CGroup).ToList();
            foreach (var item in c2List)
            {
                agencyResult.Add(new AgencyInfoC2Result()
                {
                    code = item.Code,
                    name = item.StoreName,
                    type = "CII",
                    deputy = item.Deputy,
                    address = item.CInfoCommon.AddressInfo,
                    lat = item.CInfoCommon.Lat == null ? 0 : item.CInfoCommon.Lat,
                    lng = item.CInfoCommon.Lng == null ? 0 : item.CInfoCommon.Lng,
                    phone = item.CInfoCommon.Phone,
                    id = item.Id,
                    rank = item.CInfoCommon.CRank,
                    group = item.CInfoCommon.CGroup,
                    identityCard = item.CInfoCommon.IdentityCard,
                    businessLicense = item.CInfoCommon.BusinessLicense,
                    province = item.CInfoCommon.ProvinceName,
                    district = item.CInfoCommon.DistrictName,
                    taxCode = item.CInfoCommon.TaxCode,
                    c1Id = item.C1Info.Code
                });
            }

            return agencyResult;

        }

        protected List<AgencyInfo> getListAgencyC1(HaiStaff staff)
        {
            List<AgencyInfo> agencyResult = new List<AgencyInfo>();

            List<C1Info> c1List = new List<C1Info>();
            c1List = db.C1Info.Where(p => p.IsActive == 1 && p.HaiBrandId == staff.BranchId).OrderByDescending(p => p.CInfoCommon.CGroup).ToList();
            foreach (var item in c1List)
            {
                agencyResult.Add(new AgencyInfo()
                {
                    code = item.Code,
                    name = item.StoreName,
                    type = "CI",
                    deputy = item.Deputy
                });
            }

            return agencyResult;

        }

        protected bool checkLoginSession(string user, string token)
        {
            var check = db.APIAuthHistories.Where(p => p.UserLogin == user && p.Token == token && p.IsExpired == 0).FirstOrDefault();

            return check != null ? true : false;
        }

    }
}
