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
    
    public partial class C1Info
    {
        public C1Info()
        {
            this.C2Info = new HashSet<C2Info>();
            this.HaiStaffs = new HashSet<HaiStaff>();
        }
    
        public string Id { get; set; }
        public string Code { get; set; }
        public string StoreName { get; set; }
        public string Deputy { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<int> IsLock { get; set; }
        public string InfoId { get; set; }
        public string HaiBrandId { get; set; }
    
        public virtual HaiBranch HaiBranch { get; set; }
        public virtual CInfoCommon CInfoCommon { get; set; }
        public virtual ICollection<C2Info> C2Info { get; set; }
        public virtual ICollection<HaiStaff> HaiStaffs { get; set; }
    }
}