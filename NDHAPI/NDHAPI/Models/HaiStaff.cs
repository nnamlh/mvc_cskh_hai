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
    
    public partial class HaiStaff
    {
        public HaiStaff()
        {
            this.StaffCheckIns = new HashSet<StaffCheckIn>();
            this.C1Info = new HashSet<C1Info>();
            this.C2Info = new HashSet<C2Info>();
            this.StaffCalendarC2Approve = new HashSet<StaffCalendarC2Approve>();
            this.StaffCalendarC2 = new HashSet<StaffCalendarC2>();
        }
    
        public string Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> BirthDay { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public string PositionId { get; set; }
        public string BranchId { get; set; }
        public string DepartmentId { get; set; }
        public Nullable<int> IsLock { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> TimeLeave { get; set; }
        public string UserLogin { get; set; }
        public string PositionName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string AvatarUrl { get; set; }
        public string SignatureUrl { get; set; }
        public Nullable<int> IsDelete { get; set; }
    
        public virtual HaiBranch HaiBranch { get; set; }
        public virtual HaiDepartment HaiDepartment { get; set; }
        public virtual HaiPosition HaiPosition { get; set; }
        public virtual ICollection<StaffCheckIn> StaffCheckIns { get; set; }
        public virtual ICollection<C1Info> C1Info { get; set; }
        public virtual ICollection<C2Info> C2Info { get; set; }
        public virtual ICollection<StaffCalendarC2Approve> StaffCalendarC2Approve { get; set; }
        public virtual ICollection<StaffCalendarC2> StaffCalendarC2 { get; set; }
    }
}
