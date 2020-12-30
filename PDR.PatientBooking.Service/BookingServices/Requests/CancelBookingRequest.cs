using PDR.PatientBooking.Data.Enums;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Requests
{
    public class CancelBookingRequest
    {
        public Guid OrderId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}