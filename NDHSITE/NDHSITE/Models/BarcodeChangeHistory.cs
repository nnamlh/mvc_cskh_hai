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
    
    public partial class BarcodeChangeHistory
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string CaseCode { get; set; }
        public string Barcode { get; set; }
        public Nullable<int> InAction { get; set; }
        public string WCode { get; set; }
        public string WChange { get; set; }
    }
}