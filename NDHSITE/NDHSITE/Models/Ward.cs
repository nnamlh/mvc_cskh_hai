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
    
    public partial class Ward
    {
        public Ward()
        {
            this.CInfoCommons = new HashSet<CInfoCommon>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
        public string WType { get; set; }
        public string Location { get; set; }
        public string Districtid { get; set; }
    
        public virtual ICollection<CInfoCommon> CInfoCommons { get; set; }
        public virtual District District { get; set; }
    }
}
