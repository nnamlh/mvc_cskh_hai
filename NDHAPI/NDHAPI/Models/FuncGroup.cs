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
    
    public partial class FuncGroup
    {
        public FuncGroup()
        {
            this.FuncInfoes = new HashSet<FuncInfo>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> Number { get; set; }
    
        public virtual ICollection<FuncInfo> FuncInfoes { get; set; }
    }
}
