using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class CancelBookingRequestValidator : ICancelBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public CancelBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(CancelBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (MissingRequiredFields(request, ref result))
                return result;

            return result;
        }

        public bool MissingRequiredFields(CancelBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (request.OrderId == null)
                errors.Add("Booking identifier is not specified");

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
    }
}
