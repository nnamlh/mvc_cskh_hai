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
    
    public partial class KPIWork
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KPIWork()
        {
            this.KPIDetails = new HashSet<KPIDetail>();
        }
    
        public string Id { get; set; }
        public string Title { get; set; }
        public Nullable<int> STT { get; set; }
        public string ExcelCol { get; set; }
        public string KPITypeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KPIDetail> KPIDetails { get; set; }
        public virtual KPIType KPIType { get; set; }
    }
}