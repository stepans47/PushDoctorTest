using NUnit.Framework;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.Tests.Validation
{
    [TestFixture]
    public class EmailValidatorTests
    {
        private EmailValidator _emailValidator;

        [SetUp]
        public void SetUp()
        {
            // Set instantiation
            _emailValidator = new EmailValidator();
        }

        [TestCase("user@domain.com")]
        [TestCase("user@domain-domain.com")]
        [TestCase("user@domain.net")]
        [TestCase("user@1.net")]
        [TestCase("user@domain.co.uk")]
        [TestCase("user.name@domain.com")]
        [TestCase("user.name@domain-domain.com")]
        [TestCase("user.name@domain.net")]
        [TestCase("user.name@1.net")]
        [TestCase("user.name@domain.co.uk")]
        [TestCase("user+100@domain.com")]
        [TestCase("user+100@domain-domain.com")]
        [TestCase("user+100@domain.net")]
        [TestCase("user+100@1.net")]
        [TestCase("user+100@domain.co.uk")]
        public void IsEmailValid_ValidEmail_ReturnsTrue(string email)
        {
            //arrange

            //act
            var result = _emailValidator.IsEmailValid(email);

            //assert
            Assert.IsTrue(result);
        }

        [TestCase("user@")]
        [TestCase("@")]
        [TestCase("user")]
        public void IsEmailValid_InvalidEmail_ReturnsFalse(string email)
        {
            //arrange

            //act
            var result = _emailValidator.IsEmailValid(email);

            //assert
            Assert.IsFalse(result);
        }

        [TestCase("")]
        [TestCase(null)]
        public void IsEmailValid_EmailNullOrEmpty_ReturnsTrue(string email)
        {
            //arrange

            //act
            var result = _emailValidator.IsEmailNullOrEmpty(email);

            //assert
            Assert.IsTrue(result);
        }
    }
}
