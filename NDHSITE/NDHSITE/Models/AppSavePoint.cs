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
    
    public partial class AppSavePoint
    {
        public AppSavePoint()
        {
            this.AppPointEvents = new HashSet<AppPointEvent>();
        }
    
        public string Id { get; set; }
        public string EventCode { get; set; }
        public string CInfoId { get; set; }
        public string UserLogin { get; set; }
        public Nullable<System.DateTime> AcceptTime { get; set; }
        public string SeriId { get; set; }
    
        public virtual ICollection<AppPointEvent> AppPointEvents { get; set; }
        public virtual CInfoCommon CInfoCommon { get; set; }
        public virtual ProductSeri ProductSeri { get; set; }
    }
}
