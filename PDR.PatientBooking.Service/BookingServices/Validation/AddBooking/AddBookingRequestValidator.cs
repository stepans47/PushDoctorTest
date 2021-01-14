using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Helpers;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;
        private readonly IDateTimeHelper _dateTimeHelper;

        public AddBookingRequestValidator(PatientBookingContext context, IDateTimeHelper dateTimeHelper)
        {
            _context = context;
            _dateTimeHelper = dateTimeHelper;
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

            if (request.StartTime == default(DateTime) || request.EndTime == default(DateTime))
                errors.Add("Booking date is not specified");

            if (request.DoctorId <= 0)
                errors.Add("Doctor is not specified");

            if (request.PatientId <= 0)
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
            if (request.StartTime <= _dateTimeHelper.GetCurrentDateTime())
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
               (request.StartTime >= o.StartTime && request.StartTime <= o.EndTime));

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
