//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScheduleBuilderApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Schedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Schedule()
        {
            this.EmployeeEvents = new HashSet<EmployeeEvent>();
        }
    
        public int ID { get; set; }
        public System.DateTime WeekOf { get; set; }
        public decimal TotalHours { get; set; }
        public int ApprovedStatusID { get; set; }
        public string StoreNumber { get; set; }
    
        public virtual ApprovedCode ApprovedCode { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeEvent> EmployeeEvents { get; set; }
    }
}
