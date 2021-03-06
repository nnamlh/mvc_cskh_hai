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
    
    public partial class EventInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EventInfo()
        {
            this.AgencySavePoints = new HashSet<AgencySavePoint>();
            this.EventAreas = new HashSet<EventArea>();
            this.EventAreaFarmers = new HashSet<EventAreaFarmer>();
            this.EventCustomers = new HashSet<EventCustomer>();
            this.EventCustomerFarmers = new HashSet<EventCustomerFarmer>();
            this.EventProducts = new HashSet<EventProduct>();
            this.MSGPointEvents = new HashSet<MSGPointEvent>();
            this.AwardInfoes = new HashSet<AwardInfo>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> BeginTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public string Descibe { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string UserCretea { get; set; }
        public string Thumbnail { get; set; }
        public Nullable<int> ESTT { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AgencySavePoint> AgencySavePoints { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EventArea> EventAreas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EventAreaFarmer> EventAreaFarmers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EventCustomer> EventCustomers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EventCustomerFarmer> EventCustomerFarmers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EventProduct> EventProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MSGPointEvent> MSGPointEvents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AwardInfo> AwardInfoes { get; set; }
    }
}
