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
    
    public partial class ProductTracking
    {
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string PStatus { get; set; }
        public string Warehourse { get; set; }
        public string WarehourseId { get; set; }
        public Nullable<System.DateTime> ImportTime { get; set; }
        public Nullable<System.DateTime> ExportTime { get; set; }
        public string UserType { get; set; }
    }
}