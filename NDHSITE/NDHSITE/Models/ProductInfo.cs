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
    
    public partial class ProductInfo
    {
        public ProductInfo()
        {
            this.EventProducts = new HashSet<EventProduct>();
            this.MSGPoints = new HashSet<MSGPoint>();
            this.ProductImages = new HashSet<ProductImage>();
            this.ProductSeris = new HashSet<ProductSeri>();
            this.PTrackings = new HashSet<PTracking>();
        }
    
        public string Id { get; set; }
        public string PName { get; set; }
        public string PCode { get; set; }
        public string Material { get; set; }
        public string Unit { get; set; }
        public string Producer { get; set; }
        public string CardPoint { get; set; }
        public string BoxPoint { get; set; }
        public Nullable<int> IsLock { get; set; }
        public string Barcode { get; set; }
        public Nullable<int> QuantityBox { get; set; }
        public Nullable<int> IsBox { get; set; }
        public string Register { get; set; }
        public string CommerceName { get; set; }
        public string Activce { get; set; }
        public string Poision { get; set; }
        public string Describe { get; set; }
        public string Uses { get; set; }
        public string Introduce { get; set; }
        public string Notes { get; set; }
        public string Other { get; set; }
        public string Thumbnail { get; set; }
        public string PGroup { get; set; }
        public Nullable<int> Forcus { get; set; }
        public Nullable<int> New { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> PVat { get; set; }
        public string ShortDescibe { get; set; }
    
        public virtual ICollection<EventProduct> EventProducts { get; set; }
        public virtual ICollection<MSGPoint> MSGPoints { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<ProductSeri> ProductSeris { get; set; }
        public virtual ICollection<PTracking> PTrackings { get; set; }
    }
}
