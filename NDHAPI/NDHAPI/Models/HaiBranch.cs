//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NDHAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class HaiBranch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HaiBranch()
        {
            this.C1Info = new HashSet<C1Info>();
            this.HaiStaffs = new HashSet<HaiStaff>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
        public string AddressInfo { get; set; }
        public string Notes { get; set; }
        public string AreaId { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1Info> C1Info { get; set; }
        public virtual HaiArea HaiArea { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HaiStaff> HaiStaffs { get; set; }
    }
}
