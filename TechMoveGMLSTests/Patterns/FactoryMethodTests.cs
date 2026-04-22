using Xunit;
using System;
using TechMoveGLMS.Services.Contracts;

namespace TechMoveGLMS.Tests.Patterns
{
    public class FactoryMethodTests
    {
        private readonly IContractFactory _factory;

        public FactoryMethodTests()
        {
            _factory = new ContractFactory();
        }

        [Fact]
        public void CreateContract_BasicServiceLevel_ReturnsCorrectDefaults()
        {
            // Arrange
            string serviceLevel = "Basic";
            int clientId = 1;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(1);

            // Act
            var contract = _factory.CreateContract(serviceLevel, clientId, startDate, endDate);

            // Assert
            Assert.Equal(clientId, contract.ClientId);
            Assert.Equal(serviceLevel, contract.ServiceLevel);
            Assert.Equal("Active", contract.Status);
            Assert.Equal(endDate, contract.EndDate); // Basic has no extension
        }

        [Fact]
        public void CreateContract_PremiumServiceLevel_ExtendsEndDate()
        {
            // Arrange - Premium contracts get 1 month extension
            string serviceLevel = "Premium";
            int clientId = 1;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(1);
            DateTime expectedEndDate = endDate.AddMonths(1);

            // Act
            var contract = _factory.CreateContract(serviceLevel, clientId, startDate, endDate);

            // Assert
            Assert.Equal(serviceLevel, contract.ServiceLevel);
            Assert.Equal(expectedEndDate, contract.EndDate);
        }

        [Fact]
        public void CreateContract_EnterpriseServiceLevel_ExtendsEndDateByThreeMonths()
        {
            // Arrange - Enterprise contracts get 3 months extension
            string serviceLevel = "Enterprise";
            int clientId = 1;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(1);
            DateTime expectedEndDate = endDate.AddMonths(3);

            // Act
            var contract = _factory.CreateContract(serviceLevel, clientId, startDate, endDate);

            // Assert
            Assert.Equal(serviceLevel, contract.ServiceLevel);
            Assert.Equal(expectedEndDate, contract.EndDate);
        }

        [Fact]
        public void CreateContract_CaseInsensitiveServiceLevel_WorksCorrectly()
        {
            // Arrange - Service level should be case-insensitive
            string serviceLevel = "premium";
            int clientId = 1;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(1);

            // Act
            var contract = _factory.CreateContract(serviceLevel, clientId, startDate, endDate);

            // Assert
            Assert.Equal("premium", contract.ServiceLevel);
            Assert.Equal(endDate.AddMonths(1), contract.EndDate);
        }

        [Fact]
        public void CreateContract_DefaultStatus_IsActive()
        {
            // Arrange
            string serviceLevel = "Basic";
            int clientId = 1;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddYears(1);

            // Act
            var contract = _factory.CreateContract(serviceLevel, clientId, startDate, endDate);

            // Assert
            Assert.Equal("Active", contract.Status);
        }
    }
}
// Refactoring Guru, 2026. [Factory] Design Pattern.[Online] Available at:
// https://refactoring.guru/design-patterns/factory-method

// Refactoring Guru, 2026. [Observer] Design Pattern.[Online] Available at:
//https://refactoring.guru/design-patterns/observer


// Refactoring Guru, 2026. [Strategy] Design Pattern.[Online] Available at:
//https://refactoring.guru/design-patterns/strategy