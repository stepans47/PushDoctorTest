using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (MissingRequiredFields(request, ref result))
                return result;

            if (BookingDateInvalid(request, ref result))
                return result;

            if (DoctorIsBusy(request, ref result))
                return result;

            return result;
        }

        public bool MissingRequiredFields(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (request.StartTime == null || request.EndTime == null)
                errors.Add("Booking date is not specified");

            if (request.DoctorId == 0)
                errors.Add("Doctor is not specified");

            if (request.PatientId == 0)
                errors.Add("Patient is not specified");

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        public bool BookingDateInvalid(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (request.StartTime <= DateTime.UtcNow)
            {
                result.PassedValidation = false;
                result.Errors.Add("Booking date cannot be set in the past");
                return true;
            }

            return false;
        }

        private bool DoctorIsBusy(AddBookingRequest request, ref PdrValidationResult result)
        {
            var alreadyBookedVisits = _context.Order.Any(o => 
                o.DoctorId == request.DoctorId && 
               (o.StartTime >= request.StartTime && o.EndTime <= request.EndTime));

            if (alreadyBookedVisits)
            {
                result.PassedValidation = false;
                result.Errors.Add("Doctor is already busy");
                return true;
            }

            return false;
        }
    }
}
