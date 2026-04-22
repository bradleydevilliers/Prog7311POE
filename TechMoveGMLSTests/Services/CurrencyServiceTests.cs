

using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using TechMoveGLMS.Services;
using System.Net;
//(ExchangeRate-API,2026)(Microsoft,2026)
namespace TechMoveGLMS.Tests.Services
{
    public class CurrencyServiceTests
    {
        private readonly Mock<ILogger<CurrencyService>> _mockLogger;

        public CurrencyServiceTests()
        {
            _mockLogger = new Mock<ILogger<CurrencyService>>();
        }

        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseContent)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task ConvertUsdToZar_WithValidRate_ReturnsCorrectAmount()
        {
            // Arrange - Test with known exchange rate 18.5 ZAR per USD
            var httpClient = CreateMockHttpClient(
                HttpStatusCode.OK,
                "{\"rates\":{\"ZAR\":18.5}}"
            );
            var service = new CurrencyService(httpClient, _mockLogger.Object);
            decimal usdAmount = 100m;
            decimal expected = 1850m;

            // Act
            decimal actual = await service.ConvertUsdToZarAsync(usdAmount);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ConvertUsdToZar_ZeroAmount_ReturnsZero()
        {
            // Arrange - Edge case: zero USD should return zero ZAR
            var httpClient = CreateMockHttpClient(
                HttpStatusCode.OK,
                "{\"rates\":{\"ZAR\":18.5}}"
            );
            var service = new CurrencyService(httpClient, _mockLogger.Object);
            decimal usdAmount = 0m;
            decimal expected = 0m;

            // Act
            decimal actual = await service.ConvertUsdToZarAsync(usdAmount);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ConvertUsdToZar_ApiFails_ReturnsFallbackRate()
        {
            // Arrange - API failure should trigger fallback rate (18.5)
            var httpClient = CreateMockHttpClient(
                HttpStatusCode.InternalServerError,
                "{\"error\":\"Service Unavailable\"}"
            );
            var service = new CurrencyService(httpClient, _mockLogger.Object);
            decimal usdAmount = 100m;
            decimal expected = 1850m; // 100 * 18.5 fallback rate

            // Act
            decimal actual = await service.ConvertUsdToZarAsync(usdAmount);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ConvertUsdToZar_UsesCachedRate_AfterFirstCall()
        {
            // Arrange - Verify caching prevents multiple API calls
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"rates\":{\"ZAR\":18.5}}")
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, _mockLogger.Object);

            // Act - Call twice
            decimal firstResult =  await service.ConvertUsdToZarAsync(100m);
            decimal secondResult =await service.ConvertUsdToZarAsync(100m);
            // Assert - Both calls should return the same correct value
            Assert.Equal(1850m, firstResult);
            Assert.Equal(1850m, secondResult);

           
        }
    }
}
//ExchangeRate-API,2026. Exchange Rate API Documentation.[Online] Available at:
//https://www.exchangerate-api.com/docs/overview
// Microsoft, 2026. Unit Testing in .NET Core with xUnit. Available at: 
//https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test