//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class CalendarInfo
    {
        public string Id { get; set; }
        public Nullable<int> CMonth { get; set; }
        public Nullable<int> CYear { get; set; }
        public Nullable<int> CStatus { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
        public string StaffId { get; set; }
        public string CType { get; set; }
        public string Notes { get; set; }
    
        public virtual HaiStaff HaiStaff { get; set; }
    }
}