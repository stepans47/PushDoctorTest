using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class CancelBookingRequestValidatorTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Guid _defaultOrderId;

        private CancelBookingRequestValidator _cancelBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _defaultOrderId = new Guid("6c9d06c2-c0ce-43ca-a19e-9d37548eb5ce");

            // Mock default
            SetupMockDefaults();

            // Set instantiation
            _cancelBookingRequestValidator = new CancelBookingRequestValidator(
                _context
            );
        }

        private void SetupMockDefaults()
        {
            var defaultOrder = _fixture.Build<Order>()
                .With(o => o.Id, _defaultOrderId)
                .With(o => o.StartTime, new DateTime(2020, 01, 12, 14, 30, 0))
                .With(o => o.EndTime, new DateTime(2020, 01, 12, 15, 30, 0))
                .Create();

            _context.Order.Add(defaultOrder);
            _context.SaveChanges();
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _cancelBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_OrderIdEmpty_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.OrderId = Guid.Empty;

            //act
            var res = _cancelBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Booking identifier is not specified");
        }

        private CancelBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<CancelBookingRequest>()
                .With(r => r.OrderId, _defaultOrderId)
                .Create();
            return request;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
