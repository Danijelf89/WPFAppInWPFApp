using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WpfAppAITest.Interfaces;
using WpfAppAITest.Services;

namespace WPFClientTestSuite.ServiceTests
{
   
    [TestFixture]
    class HealthCheckServiceTest
    {
        private Mock<IHttpBuilder> _mockHttpBuilder;
        private HealthCheckService _service;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;


        [SetUp]
        public void Setup()
        {
            _mockHttpBuilder = new Mock<IHttpBuilder>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpBuilder.Setup(h => h.HttpClient).Returns(httpClient);

            _service = new HealthCheckService(_mockHttpBuilder.Object);
        }

        [Test]
        public void Constructor_ShouldInitialize_WhenValidIHttpBuilderIsPassed()
        {
            // Arrange
            var mockHttpBuilder = new Mock<IHttpBuilder>();

            // Act
            var service = new HealthCheckService(mockHttpBuilder.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenIHttpBuilderIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new HealthCheckService(null!));

            Assert.That(ex?.ParamName, Is.EqualTo("http"));
        }

        [Test]
        public async Task CheckIfAlive_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });


            // Act
            var result = await _service.CheckIfAlive();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task CheckIfAlive_ShouldReturnFalse_WhenResponseIsSuccess()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });


            // Act
            var result = await _service.CheckIfAlive();

            // Assert
            Assert.IsFalse(result);
        }
    }
}
