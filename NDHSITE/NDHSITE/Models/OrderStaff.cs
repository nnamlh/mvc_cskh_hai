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
    
    public partial class OrderStaff
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string StaffId { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string ProcessId { get; set; }
        public string Notes { get; set; }
    
        public virtual HaiOrder HaiOrder { get; set; }
        public virtual HaiStaff HaiStaff { get; set; }
        public virtual OrderStaffProcess OrderStaffProcess { get; set; }
    }
}
