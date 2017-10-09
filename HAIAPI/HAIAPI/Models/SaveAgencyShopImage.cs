//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HAIAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SaveAgencyShopImage
    {
        public string Id { get; set; }
        public string Cinfo { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string AddressFull { get; set; }
        public Nullable<double> Lat { get; set; }
        public Nullable<double> Lng { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string StaffId { get; set; }
        public string ImagePath { get; set; }
    
        public virtual CInfoCommon CInfoCommon { get; set; }
        public virtual HaiStaff HaiStaff { get; set; }
    }
}