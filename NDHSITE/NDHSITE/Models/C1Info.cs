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
    
    public partial class C1Info
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C1Info()
        {
            this.HaiStaffs = new HashSet<HaiStaff>();
        }
    
        public string Id { get; set; }
        public string Code { get; set; }
        public string StoreName { get; set; }
        public string Deputy { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<int> IsLock { get; set; }
        public string InfoId { get; set; }
    
        public virtual CInfoCommon CInfoCommon { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HaiStaff> HaiStaffs { get; set; }
    }
}
