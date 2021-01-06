using PDR.PatientBooking.Data.Enums;
using System.Collections.Generic;

namespace PDR.PatientBooking.Data.Models
{
    public class Clinic
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public SurgeryType SurgeryType { get; set; }
        public virtual ICollection<Patient> Patients { get; set; }
    }
}
