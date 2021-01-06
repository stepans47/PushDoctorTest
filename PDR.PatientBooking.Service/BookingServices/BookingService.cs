using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Enums;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IAddBookingRequestValidator _bookingValidator;
        private readonly ICancelBookingRequestValidator _cancellationValidator;

        public BookingService(PatientBookingContext context, 
            IAddBookingRequestValidator bookingValidator,
            ICancelBookingRequestValidator cancellationValidator)
        {
            _context = context;
            _bookingValidator = bookingValidator;
            _cancellationValidator = cancellationValidator;
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _bookingValidator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            _context.Order.Add(new Order
            {
                Id = new Guid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                SurgeryType = request.SurgeryType
            });

            _context.SaveChanges();
        }

        public GetPatientNextAppointmentResponse GetPatientNextAppointment(long patientId)
        {
            var orders = _context
                .Order
                .Select(o => new GetPatientNextAppointmentResponse
                {
                    OrderId = o.Id,
                    OrderStatus = o.OrderStatus,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    PatientId = o.PatientId,
                    DoctorId = o.DoctorId,
                    Doctor = new Responses.Doctor
                    { 
                        FirstName = o.Doctor.FirstName,
                        LastName = o.Doctor.LastName
                    },
                    SurgeryType = o.SurgeryType
                })
                .OrderByDescending(o => o.StartTime)
                .Where(o => 
                    o.PatientId == patientId && 
                    o.StartTime > DateTime.UtcNow &&
                    o.OrderStatus == OrderStatus.Active)
                .AsNoTracking()
                .ToList();

            return orders.FirstOrDefault();
        }

        public void CancelBooking(CancelBookingRequest request)
        {
            var validationResult = _cancellationValidator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var orderForCancellation = _context.Order.Where(o =>
                o.Id == request.OrderId &&
                o.PatientId == request.PatientId)
                .FirstOrDefault();

            if (orderForCancellation == null)
            {
                throw new NullReferenceException("No order was found for cancellation");
            }

            orderForCancellation.OrderStatus = OrderStatus.Canceled;
            _context.SaveChanges();
        }
    }
}
