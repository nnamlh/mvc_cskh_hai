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
    
    public partial class SmsAccount
    {
        public int Id { get; set; }
        public string AddressSend { get; set; }
        public Nullable<int> PortSend { get; set; }
        public string UserName { get; set; }
        public string Pass { get; set; }
        public string BrandName { get; set; }
        public string Method { get; set; }
    }
}