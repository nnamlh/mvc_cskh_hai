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
    
    public partial class ProcessWork
    {
        public ProcessWork()
        {
            this.AspNetRoles = new HashSet<AspNetRole>();
        }
    
        public string Id { get; set; }
        public string ProcessName { get; set; }
        public string ProcessType { get; set; }
        public Nullable<int> TimeRequire { get; set; }
        public string Notes { get; set; }
        public Nullable<int> SortIndex { get; set; }
    
        public virtual ICollection<AspNetRole> AspNetRoles { get; set; }
    }
}
