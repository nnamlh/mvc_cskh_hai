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
    
    public partial class ProductSeri
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> BeginTime { get; set; }
        public Nullable<System.DateTime> ExpireTime { get; set; }
        public Nullable<int> IsUse { get; set; }
        public string ProductId { get; set; }
        public Nullable<int> Seri { get; set; }
        public Nullable<int> SeriType { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
    
        public virtual ProductInfo ProductInfo { get; set; }
    }
}