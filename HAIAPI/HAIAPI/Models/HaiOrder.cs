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
    
    public partial class HaiOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HaiOrder()
        {
            this.OrderProducts = new HashSet<OrderProduct>();
            this.OrderStaffs = new HashSet<OrderStaff>();
        }
    
        public string Id { get; set; }
        public string OrderType { get; set; }
        public string PayType { get; set; }
        public string ShipType { get; set; }
        public string Agency { get; set; }
        public string ReceiveAddress { get; set; }
        public string ReceivePhone1 { get; set; }
        public string ReceivePhone2 { get; set; }
        public Nullable<System.DateTime> ExpectDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string OrderStatus { get; set; }
        public string Notes { get; set; }
        public string Code { get; set; }
        public Nullable<double> PriceTotal { get; set; }
        public string BrachCode { get; set; }
    
        public virtual CInfoCommon CInfoCommon { get; set; }
        public virtual OrderStatu OrderStt { get; set; }
        public virtual OrderType OType { get; set; }
        public virtual PayType PType { get; set; }
        public virtual ShipType SType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderStaff> OrderStaffs { get; set; }
    }
}
