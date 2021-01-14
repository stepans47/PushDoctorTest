﻿using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.PatientServices.Requests;
using PDR.PatientBooking.Service.PatientServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;

namespace PDR.PatientBooking.Service.Tests.PatientServices.Validation
{
    [TestFixture]
    public class AddPatientRequestValidatorTests
    {
        private IFixture _fixture;
        private MockRepository _mockRepository;

        private PatientBookingContext _context;
        private Mock<IEmailValidator> _emailValidator;

        private AddPatientRequestValidator _addPatientRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _emailValidator = _mockRepository.Create<IEmailValidator>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _addPatientRequestValidator = new AddPatientRequestValidator(
                _context,
                _emailValidator.Object
            );
        }

        private void SetupMockDefaults()
        {
            _emailValidator.Setup(x => x.IsEmailNullOrEmpty(It.IsAny<string>())).Returns(false);
            _emailValidator.Setup(x => x.IsEmailValid(It.IsAny<string>())).Returns(true);
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ValidateRequest_FirstNameNullOrEmpty_ReturnsFailedValidationResult(string firstName)
        {
            //arrange
            var request = GetValidRequest();
            request.FirstName = firstName;

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("FirstName must be populated");
        }

        [TestCase("")]
        [TestCase(null)]
        public void ValidateRequest_LastNameNullOrEmpty_ReturnsFailedValidationResult(string lastName)
        {
            //arrange
            var request = GetValidRequest();
            request.LastName = lastName;

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("LastName must be populated");
        }

        [Test]
        public void ValidateRequest_EmailNullOrEmpty_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            _emailValidator.Setup(x => x.IsEmailNullOrEmpty(It.IsAny<string>())).Returns(true);

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Email must be populated");
        }

        [Test]
        public void ValidateRequest_InvalidEmail_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            _emailValidator.Setup(x => x.IsEmailNullOrEmpty(It.IsAny<string>())).Returns(false);
            _emailValidator.Setup(x => x.IsEmailValid(It.IsAny<string>())).Returns(false);

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Email must be a valid email address");
        }

        [Test]
        public void ValidateRequest_ValidEmail_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            _emailValidator.Setup(x => x.IsEmailNullOrEmpty(It.IsAny<string>())).Returns(false);
            _emailValidator.Setup(x => x.IsEmailValid(It.IsAny<string>())).Returns(true);

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_PatientWithEmailAddressAlreadyExists_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var existingPatient = _fixture
                .Build<Patient>()
                .With(x => x.Email, request.Email)
                .Create();

            _context.Add(existingPatient);
            _context.SaveChanges();

            //act
            var res = _addPatientRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("A patient with that email address already exists");
        }

        [Test]
        public void ValidateRequest_ClinicDoesNotExist_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.ClinicId++; //offset clinicId

            //act
            var res = _addPatientRequestValidator.ValidateRequest(_fixture.Create<AddPatientRequest>());

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("A clinic with that ID could not be found");
        }

        private AddPatientRequest GetValidRequest()
        {
            var clinic = _fixture.Create<Clinic>();
            _context.Clinic.Add(clinic);
            _context.SaveChanges();

            var request = _fixture.Build<AddPatientRequest>()
                .With(x => x.ClinicId, clinic.Id)
                .With(x => x.Email, "user@domain.com")
                .Create();
            return request;
        }
    }
}
