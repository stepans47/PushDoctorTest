using System;

namespace PDR.PatientBooking.Service.Helpers
{
    public class GuidHelper : IGuidHelper
    {
        public Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
