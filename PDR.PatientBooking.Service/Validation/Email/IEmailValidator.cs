
namespace PDR.PatientBooking.Service.Validation
{
    public interface IEmailValidator
    {
        bool IsEmailNullOrEmpty(string email);
        bool IsEmailValid(string email);
    }
}
