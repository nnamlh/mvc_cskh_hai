//------------------------------------------------------------------------------
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
    
    public partial class report_checkin_detail_by_staff_Result
    {
        public string Branch { get; set; }
        public string StaffCode { get; set; }
        public string StaffName { get; set; }
        public string CalendarType { get; set; }
        public Nullable<int> CalendarDay { get; set; }
        public Nullable<int> CalendarMonth { get; set; }
        public Nullable<int> CalendarYear { get; set; }
        public string AgencyCode { get; set; }
        public string StoreName { get; set; }
        public Nullable<int> InPlan { get; set; }
        public Nullable<int> Perform { get; set; }
    }
}
