using PDR.PatientBooking.Data.Enums;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Responses
{
    public class GetPatientNextAppointmentResponse
    {
        public Guid OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }
        public int SurgeryType { get; set; }
    }

    public class Doctor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
