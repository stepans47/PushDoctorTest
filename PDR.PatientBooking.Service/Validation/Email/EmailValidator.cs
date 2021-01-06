using System.Text.RegularExpressions;

namespace PDR.PatientBooking.Service.Validation
{
    public class EmailValidator : IEmailValidator
    {
        public EmailValidator() { }

        public bool IsEmailValid(string email)
        {
            string emailPattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" +
                                   "@" +
                                  @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";

            Regex regex = new Regex(emailPattern);
            Match match = regex.Match(email);
            return match.Success;
        }
    }
}
