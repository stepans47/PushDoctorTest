using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Enums;
using PDR.PatientBooking.Service.Helpers;
using System;
using System.Collections.Generic;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class AddBookingRequestValidatorTests
    {
        private IFixture _fixture;
        private MockRepository _mockRepository;

        private PatientBookingContext _context;
        private Mock<IDateTimeHelper> _dateTimeHelper;

        private AddBookingRequestValidator _addBookingRequestValidator;

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
            _dateTimeHelper = _mockRepository.Create<IDateTimeHelper>();

            // Mock default
            SetupMockDefaults();

            // Set instantiation
            _addBookingRequestValidator = new AddBookingRequestValidator(
                _context,
                _dateTimeHelper.Object
            );
        }

        private void SetupMockDefaults()
        {
            _dateTimeHelper.Setup(x => x.GetCurrentDateTime())
                .Returns(new DateTime(2020, 01, 12, 13, 0, 0));

            var defaultDoctor = _fixture.Build<Doctor>()
                .With(d => d.Id, 10)
                .Create();
            defaultDoctor.Orders = new List<Order>();
            var defaultOrder = _fixture.Build<Order>()
                .With(o => o.Id, Guid.NewGuid())
                .With(o => o.StartTime, new DateTime(2020, 01, 12, 14, 30, 0))
                .With(o => o.EndTime, new DateTime(2020, 01, 12, 15, 30, 0))
                .Create();
            defaultDoctor.Orders.Add(defaultOrder);

            _context.Doctor.Add(defaultDoctor);
            _context.SaveChanges();
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ValidateRequest_PatientIdInvalid_ReturnsFailedValidationResult(int patientId)
        {
            //arrange
            var request = GetValidRequest();
            request.PatientId = patientId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Patient is not specified");
        }


        [Test]
        public void ValidateRequest_BookingDateInvalid_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = new DateTime(2020, 01, 12, 12, 50, 0);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Booking date cannot be set in the past");
        }

        [Test]
        public void ValidateRequest_DoctorIsBusy_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.DoctorId = 10;
            request.StartTime = new DateTime(2020, 01, 12, 14, 45, 0);
            request.EndTime = new DateTime(2020, 01, 12, 15, 45, 0);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Doctor is already busy");
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<AddBookingRequest>()
                .With(r => r.StartTime, new DateTime(2020, 01, 12, 14, 0, 0))
                .With(r => r.EndTime, new DateTime(2020, 01, 12, 15, 0, 0))
                .Create<AddBookingRequest>();
            return request;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
