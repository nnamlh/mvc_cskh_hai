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
    
    public partial class HaiOrder
    {
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
        public virtual OrderStatu OrderStatu { get; set; }
        public virtual OrderType OrderType1 { get; set; }
        public virtual PayType PayType1 { get; set; }
        public virtual ShipType ShipType1 { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        public virtual ICollection<OrderStaff> OrderStaffs { get; set; }
    }
}
