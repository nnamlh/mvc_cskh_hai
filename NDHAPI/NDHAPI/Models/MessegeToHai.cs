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
    
    public partial class MessegeToHai
    {
        public string Id { get; set; }
        public string Messenge { get; set; }
        public string UserLogin { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string MsgType { get; set; }
        public Nullable<int> IsSeen { get; set; }
        public string UserSeen { get; set; }
    }
}
