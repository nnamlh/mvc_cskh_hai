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
    
    public partial class AgencySavePoint
    {
        public string EventId { get; set; }
        public string CInfoId { get; set; }
        public Nullable<int> PointSave { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
    
        public virtual CInfoCommon CInfoCommon { get; set; }
        public virtual EventInfo EventInfo { get; set; }
    }
}