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
    using System.Collections.Generic;
    
    public partial class CalendarWork
    {
        public string Id { get; set; }
        public Nullable<int> CDay { get; set; }
        public Nullable<int> CMonth { get; set; }
        public Nullable<int> CYear { get; set; }
        public string StaffId { get; set; }
        public Nullable<int> InPlan { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyType { get; set; }
        public string TypeId { get; set; }
        public string Notes { get; set; }
        public Nullable<int> CIn { get; set; }
        public Nullable<int> COut { get; set; }
        public Nullable<System.TimeSpan> CInTime { get; set; }
        public Nullable<System.TimeSpan> COutTime { get; set; }
        public Nullable<int> AllTime { get; set; }
        public Nullable<double> LatCheck { get; set; }
        public Nullable<double> LngCheck { get; set; }
        public Nullable<System.DateTime> TimeCheck { get; set; }
        public Nullable<double> Distance { get; set; }
        public Nullable<int> Perform { get; set; }
        public string DayInWeek { get; set; }
    }
}
