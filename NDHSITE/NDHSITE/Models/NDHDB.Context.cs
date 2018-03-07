﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NDHSITE.Models
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
        public virtual DbSet<AgencyType> AgencyTypes { get; set; }
        public virtual DbSet<AllStatu> AllStatus { get; set; }
        public virtual DbSet<APIAuthHistory> APIAuthHistories { get; set; }
        public virtual DbSet<APIHistory> APIHistories { get; set; }
        public virtual DbSet<APILogoutHistory> APILogoutHistories { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AwardInfo> AwardInfoes { get; set; }
        public virtual DbSet<BarcodeChangeHistory> BarcodeChangeHistories { get; set; }
        public virtual DbSet<BarcodeHistory> BarcodeHistories { get; set; }
        public virtual DbSet<BasicNotification> BasicNotifications { get; set; }
        public virtual DbSet<C1Info> C1Info { get; set; }
        public virtual DbSet<C2C1> C2C1 { get; set; }
        public virtual DbSet<C2Info> C2Info { get; set; }
        public virtual DbSet<CalendarInfo> CalendarInfoes { get; set; }
        public virtual DbSet<CalendarType> CalendarTypes { get; set; }
        public virtual DbSet<CalendarWork> CalendarWorks { get; set; }
        public virtual DbSet<CInfoCommon> CInfoCommons { get; set; }
        public virtual DbSet<DecorGroup> DecorGroups { get; set; }
        public virtual DbSet<DecorImage> DecorImages { get; set; }
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
        public virtual DbSet<HaiOrder> HaiOrders { get; set; }
        public virtual DbSet<HaiPosition> HaiPositions { get; set; }
        public virtual DbSet<HaiStaff> HaiStaffs { get; set; }
        public virtual DbSet<HappyBirthday> HappyBirthdays { get; set; }
        public virtual DbSet<ImeiUser> ImeiUsers { get; set; }
        public virtual DbSet<MessegeToHai> MessegeToHais { get; set; }
        public virtual DbSet<MobileFunction> MobileFunctions { get; set; }
        public virtual DbSet<MSGPoint> MSGPoints { get; set; }
        public virtual DbSet<MSGPointEvent> MSGPointEvents { get; set; }
        public virtual DbSet<NotificationTopic> NotificationTopics { get; set; }
        public virtual DbSet<OldKeySave> OldKeySaves { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<OrderProductHistory> OrderProductHistories { get; set; }
        public virtual DbSet<OrderStaff> OrderStaffs { get; set; }
        public virtual DbSet<OrderStaffProcess> OrderStaffProcesses { get; set; }
        public virtual DbSet<OrderStatu> OrderStatus { get; set; }
        public virtual DbSet<OrderType> OrderTypes { get; set; }
        public virtual DbSet<PayType> PayTypes { get; set; }
        public virtual DbSet<PHistory> PHistories { get; set; }
        public virtual DbSet<ProcessHistory> ProcessHistories { get; set; }
        public virtual DbSet<ProcessWork> ProcessWorks { get; set; }
        public virtual DbSet<ProductGroup> ProductGroups { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<ProductInfo> ProductInfoes { get; set; }
        public virtual DbSet<ProductSeri> ProductSeris { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<PTracking> PTrackings { get; set; }
        public virtual DbSet<RegFirebase> RegFirebases { get; set; }
        public virtual DbSet<RoleCheckImei> RoleCheckImeis { get; set; }
        public virtual DbSet<SaveAgencyShopImage> SaveAgencyShopImages { get; set; }
        public virtual DbSet<SendSmsHistory> SendSmsHistories { get; set; }
        public virtual DbSet<ServerInfo> ServerInfoes { get; set; }
        public virtual DbSet<ShipType> ShipTypes { get; set; }
        public virtual DbSet<SmsAccount> SmsAccounts { get; set; }
        public virtual DbSet<SMSCode> SMSCodes { get; set; }
        public virtual DbSet<SMSHistory> SMSHistories { get; set; }
        public virtual DbSet<StaffCheckIn> StaffCheckIns { get; set; }
        public virtual DbSet<StaffWithC2> StaffWithC2 { get; set; }
        public virtual DbSet<StoreAgencyId> StoreAgencyIds { get; set; }
        public virtual DbSet<StoreStaffId> StoreStaffIds { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<TreeInfo> TreeInfoes { get; set; }
        public virtual DbSet<UserBranchPermiss> UserBranchPermisses { get; set; }
        public virtual DbSet<Ward> Wards { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<DeliveryStatu> DeliveryStatus { get; set; }
    
        public virtual ObjectResult<checkin_calendartype_group_Result> checkin_calendartype_group(Nullable<int> month, Nullable<int> year, string staffId)
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
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<checkin_calendartype_group_Result>("checkin_calendartype_group", monthParameter, yearParameter, staffIdParameter);
        }
    
        public virtual ObjectResult<checkin_count_day_agency_Result> checkin_count_day_agency(Nullable<int> month, Nullable<int> year, string staffId)
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
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<checkin_count_day_agency_Result>("checkin_count_day_agency", monthParameter, yearParameter, staffIdParameter);
        }
    
        public virtual ObjectResult<checkin_getcalendar_Result> checkin_getcalendar(Nullable<int> month, Nullable<int> year, string staffId)
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
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<checkin_getcalendar_Result>("checkin_getcalendar", monthParameter, yearParameter, staffIdParameter);
        }
    
        public virtual int checkin_report_plan(Nullable<int> maxday, string month, string year, string staffId)
        {
            var maxdayParameter = maxday.HasValue ?
                new ObjectParameter("maxday", maxday) :
                new ObjectParameter("maxday", typeof(int));
    
            var monthParameter = month != null ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(string));
    
            var yearParameter = year != null ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(string));
    
            var staffIdParameter = staffId != null ?
                new ObjectParameter("staffId", staffId) :
                new ObjectParameter("staffId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("checkin_report_plan", maxdayParameter, monthParameter, yearParameter, staffIdParameter);
        }
    
        public virtual ObjectResult<procduct_item_detail_Result> procduct_item_detail(string id)
        {
            var idParameter = id != null ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<procduct_item_detail_Result>("procduct_item_detail", idParameter);
        }
    
        public virtual ObjectResult<product_list_Result> product_list()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<product_list_Result>("product_list");
        }
    
        public virtual ObjectResult<report_checkin_detail_by_branch_Result> report_checkin_detail_by_branch(Nullable<int> month, Nullable<int> year, string branch)
        {
            var monthParameter = month.HasValue ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            var branchParameter = branch != null ?
                new ObjectParameter("branch", branch) :
                new ObjectParameter("branch", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_checkin_detail_by_branch_Result>("report_checkin_detail_by_branch", monthParameter, yearParameter, branchParameter);
        }
    
        public virtual ObjectResult<report_checkin_detail_by_staff_Result> report_checkin_detail_by_staff(Nullable<int> month, Nullable<int> year, string staffId)
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
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_checkin_detail_by_staff_Result>("report_checkin_detail_by_staff", monthParameter, yearParameter, staffIdParameter);
        }
    
        public virtual ObjectResult<report_cii_product_Result> report_cii_product(string dFrom, string dTo)
        {
            var dFromParameter = dFrom != null ?
                new ObjectParameter("DFrom", dFrom) :
                new ObjectParameter("DFrom", typeof(string));
    
            var dToParameter = dTo != null ?
                new ObjectParameter("DTo", dTo) :
                new ObjectParameter("DTo", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_cii_product_Result>("report_cii_product", dFromParameter, dToParameter);
        }
    
        public virtual ObjectResult<report_event_agency_Result> report_event_agency(string cType, string eventId)
        {
            var cTypeParameter = cType != null ?
                new ObjectParameter("CType", cType) :
                new ObjectParameter("CType", typeof(string));
    
            var eventIdParameter = eventId != null ?
                new ObjectParameter("EventId", eventId) :
                new ObjectParameter("EventId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_event_agency_Result>("report_event_agency", cTypeParameter, eventIdParameter);
        }
    
        public virtual ObjectResult<report_remain_product_Result> report_remain_product(string product, string dFrom, string dTo)
        {
            var productParameter = product != null ?
                new ObjectParameter("Product", product) :
                new ObjectParameter("Product", typeof(string));
    
            var dFromParameter = dFrom != null ?
                new ObjectParameter("DFrom", dFrom) :
                new ObjectParameter("DFrom", typeof(string));
    
            var dToParameter = dTo != null ?
                new ObjectParameter("DTo", dTo) :
                new ObjectParameter("DTo", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_remain_product_Result>("report_remain_product", productParameter, dFromParameter, dToParameter);
        }
    
        public virtual ObjectResult<report_remain_wcode_Result> report_remain_wcode(string wCode, string dFrom, string dTo)
        {
            var wCodeParameter = wCode != null ?
                new ObjectParameter("WCode", wCode) :
                new ObjectParameter("WCode", typeof(string));
    
            var dFromParameter = dFrom != null ?
                new ObjectParameter("DFrom", dFrom) :
                new ObjectParameter("DFrom", typeof(string));
    
            var dToParameter = dTo != null ?
                new ObjectParameter("DTo", dTo) :
                new ObjectParameter("DTo", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_remain_wcode_Result>("report_remain_wcode", wCodeParameter, dFromParameter, dToParameter);
        }
    
        public virtual ObjectResult<c2_get_list_c1_Result> c2_get_list_c1(string code)
        {
            var codeParameter = code != null ?
                new ObjectParameter("Code", code) :
                new ObjectParameter("Code", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<c2_get_list_c1_Result>("c2_get_list_c1", codeParameter);
        }
    
        public virtual int calendar_remove(string staffId, Nullable<int> cMonth, Nullable<int> cYear)
        {
            var staffIdParameter = staffId != null ?
                new ObjectParameter("StaffId", staffId) :
                new ObjectParameter("StaffId", typeof(string));
    
            var cMonthParameter = cMonth.HasValue ?
                new ObjectParameter("CMonth", cMonth) :
                new ObjectParameter("CMonth", typeof(int));
    
            var cYearParameter = cYear.HasValue ?
                new ObjectParameter("CYear", cYear) :
                new ObjectParameter("CYear", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("calendar_remove", staffIdParameter, cMonthParameter, cYearParameter);
        }
    
        public virtual ObjectResult<get_decor_info_Result> get_decor_info(string branch, string agency, string group, Nullable<int> month, Nullable<int> year)
        {
            var branchParameter = branch != null ?
                new ObjectParameter("branch", branch) :
                new ObjectParameter("branch", typeof(string));
    
            var agencyParameter = agency != null ?
                new ObjectParameter("agency", agency) :
                new ObjectParameter("agency", typeof(string));
    
            var groupParameter = group != null ?
                new ObjectParameter("group", group) :
                new ObjectParameter("group", typeof(string));
    
            var monthParameter = month.HasValue ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<get_decor_info_Result>("get_decor_info", branchParameter, agencyParameter, groupParameter, monthParameter, yearParameter);
        }
    
        public virtual ObjectResult<report_checkin_general_Result2> report_checkin_general(Nullable<int> month, Nullable<int> year, Nullable<int> fDay, Nullable<int> tDay, string fDate, string tDate)
        {
            var monthParameter = month.HasValue ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            var fDayParameter = fDay.HasValue ?
                new ObjectParameter("fDay", fDay) :
                new ObjectParameter("fDay", typeof(int));
    
            var tDayParameter = tDay.HasValue ?
                new ObjectParameter("tDay", tDay) :
                new ObjectParameter("tDay", typeof(int));
    
            var fDateParameter = fDate != null ?
                new ObjectParameter("fDate", fDate) :
                new ObjectParameter("fDate", typeof(string));
    
            var tDateParameter = tDate != null ?
                new ObjectParameter("tDate", tDate) :
                new ObjectParameter("tDate", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_checkin_general_Result2>("report_checkin_general", monthParameter, yearParameter, fDayParameter, tDayParameter, fDateParameter, tDateParameter);
        }
    
        public virtual ObjectResult<report_checkin_general_day_Result1> report_checkin_general_day(Nullable<int> month, Nullable<int> year, Nullable<int> day)
        {
            var monthParameter = month.HasValue ?
                new ObjectParameter("month", month) :
                new ObjectParameter("month", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            var dayParameter = day.HasValue ?
                new ObjectParameter("day", day) :
                new ObjectParameter("day", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_checkin_general_day_Result1>("report_checkin_general_day", monthParameter, yearParameter, dayParameter);
        }
    
        public virtual ObjectResult<report_order_detail_Result> report_order_detail(string fDate, string tDate)
        {
            var fDateParameter = fDate != null ?
                new ObjectParameter("fDate", fDate) :
                new ObjectParameter("fDate", typeof(string));
    
            var tDateParameter = tDate != null ?
                new ObjectParameter("tDate", tDate) :
                new ObjectParameter("tDate", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_order_detail_Result>("report_order_detail", fDateParameter, tDateParameter);
        }
    
        public virtual ObjectResult<get_list_orders_Result> get_list_orders(string fdate, string tdate, string ostt, string dstt, string staff, string c1)
        {
            var fdateParameter = fdate != null ?
                new ObjectParameter("fdate", fdate) :
                new ObjectParameter("fdate", typeof(string));
    
            var tdateParameter = tdate != null ?
                new ObjectParameter("tdate", tdate) :
                new ObjectParameter("tdate", typeof(string));
    
            var osttParameter = ostt != null ?
                new ObjectParameter("ostt", ostt) :
                new ObjectParameter("ostt", typeof(string));
    
            var dsttParameter = dstt != null ?
                new ObjectParameter("dstt", dstt) :
                new ObjectParameter("dstt", typeof(string));
    
            var staffParameter = staff != null ?
                new ObjectParameter("staff", staff) :
                new ObjectParameter("staff", typeof(string));
    
            var c1Parameter = c1 != null ?
                new ObjectParameter("c1", c1) :
                new ObjectParameter("c1", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<get_list_orders_Result>("get_list_orders", fdateParameter, tdateParameter, osttParameter, dsttParameter, staffParameter, c1Parameter);
        }
    
        public virtual ObjectResult<get_list_orders_Result> get_list_orders_by_code(string code)
        {
            var codeParameter = code != null ?
                new ObjectParameter("code", code) :
                new ObjectParameter("code", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<get_list_orders_Result>("get_list_orders_by_code", codeParameter);
        }
    
        public virtual ObjectResult<get_list_orders_product_Result> get_list_orders_product(string fdate, string tdate, string ostt, string dstt, string staff, string c1)
        {
            var fdateParameter = fdate != null ?
                new ObjectParameter("fdate", fdate) :
                new ObjectParameter("fdate", typeof(string));
    
            var tdateParameter = tdate != null ?
                new ObjectParameter("tdate", tdate) :
                new ObjectParameter("tdate", typeof(string));
    
            var osttParameter = ostt != null ?
                new ObjectParameter("ostt", ostt) :
                new ObjectParameter("ostt", typeof(string));
    
            var dsttParameter = dstt != null ?
                new ObjectParameter("dstt", dstt) :
                new ObjectParameter("dstt", typeof(string));
    
            var staffParameter = staff != null ?
                new ObjectParameter("staff", staff) :
                new ObjectParameter("staff", typeof(string));
    
            var c1Parameter = c1 != null ?
                new ObjectParameter("c1", c1) :
                new ObjectParameter("c1", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<get_list_orders_product_Result>("get_list_orders_product", fdateParameter, tdateParameter, osttParameter, dsttParameter, staffParameter, c1Parameter);
        }
    
        public virtual ObjectResult<report_order_staff_sales_Result> report_order_staff_sales(string fdate, string tdate)
        {
            var fdateParameter = fdate != null ?
                new ObjectParameter("fdate", fdate) :
                new ObjectParameter("fdate", typeof(string));
    
            var tdateParameter = tdate != null ?
                new ObjectParameter("tdate", tdate) :
                new ObjectParameter("tdate", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<report_order_staff_sales_Result>("report_order_staff_sales", fdateParameter, tdateParameter);
        }
    }
}
