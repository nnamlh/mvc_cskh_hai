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
    
    public partial class BasicNotification
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Messenge { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string TocpicCode { get; set; }
        public string UserSend { get; set; }
        public string UserAccept { get; set; }
        public string MessengeResult { get; set; }
        public string ImageAttach { get; set; }
    }
}
