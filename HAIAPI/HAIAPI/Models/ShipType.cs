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
    
    public partial class ShipType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ShipType()
        {
            this.HaiOrders = new HashSet<HaiOrder>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public Nullable<int> Idx { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HaiOrder> HaiOrders { get; set; }
    }
}
