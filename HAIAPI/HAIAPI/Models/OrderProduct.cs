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
    
    public partial class OrderProduct
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<double> PriceTotal { get; set; }
        public Nullable<double> PerPrice { get; set; }
        public Nullable<int> QuantityFinish { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
    
        public virtual HaiOrder HaiOrder { get; set; }
        public virtual ProductInfo ProductInfo { get; set; }
    }
}
