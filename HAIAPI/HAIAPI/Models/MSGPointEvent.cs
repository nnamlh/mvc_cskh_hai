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
    
    public partial class MSGPointEvent
    {
        public string EventId { get; set; }
        public string MSGPointId { get; set; }
        public Nullable<int> Point { get; set; }
    
        public virtual EventInfo EventInfo { get; set; }
        public virtual MSGPoint MSGPoint { get; set; }
    }
}