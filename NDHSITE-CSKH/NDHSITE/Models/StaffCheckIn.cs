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
    
    public partial class StaffCheckIn
    {
        public string Id { get; set; }
        public string StaffUser { get; set; }
        public string StaffId { get; set; }
        public Nullable<System.DateTime> AcceptTime { get; set; }
        public string ImageUrl { get; set; }
        public string Comment { get; set; }
        public Nullable<double> Longitude { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string Agency { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }
        public string AgencyType { get; set; }
        public string AgencyTypeName { get; set; }
    
        public virtual HaiStaff HaiStaff { get; set; }
    }
}