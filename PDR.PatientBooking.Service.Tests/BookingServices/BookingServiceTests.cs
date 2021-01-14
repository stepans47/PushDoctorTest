using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Enums;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Helpers;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private IFixture _fixture;
        private MockRepository _mockRepository;

        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _addBookingValidator;
        private Mock<ICancelBookingRequestValidator> _cancleBookingValidator;
        private Mock<IDateTimeHelper> _dateTimeHelper;
        private Mock<IGuidHelper> _guidHelper;

        private Guid _defaultNewOrderId;
        private Guid _defaultCanceledOrderId;
        private int _defaultPatientId;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _defaultNewOrderId = new Guid("964669ea-17b4-4fa8-af00-8f359bad2452");
            _defaultCanceledOrderId = new Guid("b85dc61a-a32d-4237-ad39-d069a7c6804b");
            _defaultPatientId = 20;

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _addBookingValidator = _mockRepository.Create<IAddBookingRequestValidator>();
            _cancleBookingValidator = _mockRepository.Create<ICancelBookingRequestValidator>();
            _dateTimeHelper = _mockRepository.Create<IDateTimeHelper>();
            _guidHelper = _mockRepository.Create<IGuidHelper>();

            // Mock default
            SetupMockDefaults();

            // Set instantiation
            _bookingService = new BookingService(
                _context,
                _addBookingValidator.Object,
                _cancleBookingValidator.Object,
                _dateTimeHelper.Object,
                _guidHelper.Object
            );
        }

        private void SetupMockDefaults()
        {
            _addBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));

            _cancleBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>()))
                .Returns(new PdrValidationResult(true));

            _dateTimeHelper.Setup(x => x.GetCurrentDateTime())
                .Returns(new DateTime(2020, 01, 12, 13, 0, 0));

            _guidHelper.Setup(x => x.GetNewGuid())
                .Returns(_defaultNewOrderId);

            var defaultPatient = _fixture.Build<Patient>()
                .With(p => p.Id, _defaultPatientId)
                .Create();
            var defaultOrder = _fixture.Build<Order>()
                .With(o => o.Id, _defaultCanceledOrderId)
                .With(r => r.Patient, defaultPatient)
                .With(o => o.StartTime, new DateTime(2020, 01, 12, 14, 30, 0))
                .With(o => o.EndTime, new DateTime(2020, 01, 12, 15, 30, 0))
                .Create();

            _context.Order.Add(defaultOrder);
            _context.SaveChanges();
        }

        #region Add booking tests
        [Test]
        public void AddBooking_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            //act
            _bookingService.AddBooking(request);

            //assert
            _addBookingValidator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());
            _addBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.AddBooking(_fixture.Create<AddBookingRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void AddBooking_AddsBookingToContextWithGeneratedId()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            var expected = new Order
            {
                Id = _defaultNewOrderId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SurgeryType = request.SurgeryType,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId
            };

            //act
            _bookingService.AddBooking(request);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected);
        }
        #endregion

        #region Cancel booking test
        [Test]
        public void CancelBooking_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Build<CancelBookingRequest>()
                .With(r => r.OrderId, _defaultCanceledOrderId)
                .With(r => r.PatientId, _defaultPatientId)
                .Create();

            //act
            _bookingService.CancelBooking(request);

            //assert
            _cancleBookingValidator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void CancelBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());
            _cancleBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(_fixture.Create<CancelBookingRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }
        #endregion

        #region Get booking info tests
        [Test]
        public void GetNextAppointment_ValidatesRequest()
        {
            //arrange
            var patientId = _defaultPatientId;

            //act
            var result = _bookingService.GetPatientNextAppointment(patientId);

            //assert
            Assert.IsNotNull(result);
            Assert.That(result.DoctorId > 0);
            Assert.That(result.OrderStatus == OrderStatus.Active);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(100)]
        public void GetNextAppointment_ValidatesRequest(long patientId)
        {
            //arrange

            //act
            var result = _bookingService.GetPatientNextAppointment(patientId);

            //assert
            Assert.Null(result);
        }
        #endregion


        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
