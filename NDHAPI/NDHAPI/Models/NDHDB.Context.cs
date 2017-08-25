﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NDHAPI.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class NDHDBEntities : DbContext
    {
        public NDHDBEntities()
            : base("name=NDHDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AgencySavePoint> AgencySavePoints { get; set; }
        public virtual DbSet<AllStatu> AllStatus { get; set; }
        public virtual DbSet<APIAuthHistory> APIAuthHistories { get; set; }
        public virtual DbSet<APIDetail> APIDetails { get; set; }
        public virtual DbSet<APIHistory> APIHistories { get; set; }
        public virtual DbSet<APILogoutHistory> APILogoutHistories { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AwardInfo> AwardInfoes { get; set; }
        public virtual DbSet<BarcodeHistory> BarcodeHistories { get; set; }
        public virtual DbSet<BasicNotification> BasicNotifications { get; set; }
        public virtual DbSet<C1Info> C1Info { get; set; }
        public virtual DbSet<C2Info> C2Info { get; set; }
        public virtual DbSet<CInfoCommon> CInfoCommons { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<ErrorInfo> ErrorInfoes { get; set; }
        public virtual DbSet<EventArea> EventAreas { get; set; }
        public virtual DbSet<EventAreaFarmer> EventAreaFarmers { get; set; }
        public virtual DbSet<EventCustomer> EventCustomers { get; set; }
        public virtual DbSet<EventCustomerFarmer> EventCustomerFarmers { get; set; }
        public virtual DbSet<EventInfo> EventInfoes { get; set; }
        public virtual DbSet<EventProduct> EventProducts { get; set; }
        public virtual DbSet<FarmerInfo> FarmerInfoes { get; set; }
        public virtual DbSet<FuncGroup> FuncGroups { get; set; }
        public virtual DbSet<FuncInfo> FuncInfoes { get; set; }
        public virtual DbSet<FuncRole> FuncRoles { get; set; }
        public virtual DbSet<HaiArea> HaiAreas { get; set; }
        public virtual DbSet<HaiBranch> HaiBranches { get; set; }
        public virtual DbSet<HaiDepartment> HaiDepartments { get; set; }
        public virtual DbSet<HaiPosition> HaiPositions { get; set; }
        public virtual DbSet<HaiStaff> HaiStaffs { get; set; }
        public virtual DbSet<HappyBirthday> HappyBirthdays { get; set; }
        public virtual DbSet<ImeiUser> ImeiUsers { get; set; }
        public virtual DbSet<MessegeToHai> MessegeToHais { get; set; }
        public virtual DbSet<MobileFunction> MobileFunctions { get; set; }
        public virtual DbSet<MSGPoint> MSGPoints { get; set; }
        public virtual DbSet<MSGPointEvent> MSGPointEvents { get; set; }
        public virtual DbSet<NotificationTopic> NotificationTopics { get; set; }
        public virtual DbSet<PHistory> PHistories { get; set; }
        public virtual DbSet<ProductInfo> ProductInfoes { get; set; }
        public virtual DbSet<ProductSeri> ProductSeris { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<PTracking> PTrackings { get; set; }
        public virtual DbSet<RegFirebase> RegFirebases { get; set; }
        public virtual DbSet<RoleCheckImei> RoleCheckImeis { get; set; }
        public virtual DbSet<SendSmsHistory> SendSmsHistories { get; set; }
        public virtual DbSet<ServerInfo> ServerInfoes { get; set; }
        public virtual DbSet<SmsAccount> SmsAccounts { get; set; }
        public virtual DbSet<SMSCode> SMSCodes { get; set; }
        public virtual DbSet<SMSHistory> SMSHistories { get; set; }
        public virtual DbSet<StaffCheckIn> StaffCheckIns { get; set; }
        public virtual DbSet<TreeInfo> TreeInfoes { get; set; }
        public virtual DbSet<Ward> Wards { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<CheckInCalendarStatu> CheckInCalendarStatus { get; set; }
        public virtual DbSet<CheckInCalendarHistory> CheckInCalendarHistories { get; set; }
        public virtual DbSet<CheckInCalendar> CheckInCalendars { get; set; }
    
        public virtual ObjectResult<checkin_getcalendar_Result2> checkin_getcalendar(Nullable<int> month, Nullable<int> year, string staffId)
        {
            var monthParameter = month.HasValue ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            var staffIdParameter = staffId != null ?
                new ObjectParameter("staffId", staffId) :
                new ObjectParameter("staffId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<checkin_getcalendar_Result2>("checkin_getcalendar", monthParameter, yearParameter, staffIdParameter);
        }
    }
}
