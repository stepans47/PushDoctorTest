using System;

namespace PDR.PatientBooking.Service.Helpers
{
    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.UtcNow;
        }
    }
}
