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
    
    public partial class FuncInfo
    {
        public FuncInfo()
        {
            this.FuncRoles = new HashSet<FuncRole>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> Position { get; set; }
        public Nullable<int> Number { get; set; }
        public Nullable<int> GroupId { get; set; }
        public string Code { get; set; }
        public string UrlInfo { get; set; }
        public string IconInfo { get; set; }
    
        public virtual FuncGroup FuncGroup { get; set; }
        public virtual ICollection<FuncRole> FuncRoles { get; set; }
    }
}
