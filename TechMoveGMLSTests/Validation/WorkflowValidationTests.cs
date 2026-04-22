using Xunit;
using System;
using TechMoveGLMS.Models.Entities;
using TechMoveGLMS.Tests.TestHelpers;

namespace TechMoveGLMS.Tests.Validation
{
    public class WorkflowValidationTests
    {
        [Fact]
        public void Contract_WithFutureEndDate_IsNotExpired()
        {
            // Arrange - Active contract with future end date
            var contract = new Contract
            {
                Status = "Active",
                EndDate = DateTime.Now.AddDays(30)
            };

            // Act
            bool isExpired = contract.IsExpired;
            bool isActive = contract.IsActive;

            // Assert
            Assert.False(isExpired);
            Assert.True(isActive);
        }

        [Fact]
        public void Contract_WithPastEndDate_IsExpired()
        {
            // Arrange - Contract with past end date should be expired
            var contract = new Contract
            {
                Status = "Active",
                EndDate = DateTime.Now.AddDays(-1)
            };

            // Act
            bool isExpired = contract.IsExpired;
            bool isActive = contract.IsActive;

            // Assert
            Assert.True(isExpired);
            Assert.False(isActive);
        }

        [Fact]
        public void Contract_DraftStatus_IsNotActive()
        {
            // Arrange - Draft contracts should not be considered active
            var contract = new Contract
            {
                Status = "Draft",
                EndDate = DateTime.Now.AddDays(30)
            };

            // Act
            bool isActive = contract.IsActive;

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void Contract_ExpiredStatus_IsNotActive()
        {
            // Arrange - Explicitly expired contracts should not be active
            var contract = new Contract
            {
                Status = "Expired",
                EndDate = DateTime.Now.AddDays(30)
            };

            // Act
            bool isActive = contract.IsActive;

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void ServiceRequest_CanOnlyBeCreatedForActiveContract()
        {
            // Arrange - Validate business rule: ServiceRequest requires Active contract
            var activeContract = TestDataFactory.CreateTestContract(status: "Active");
            var expiredContract = TestDataFactory.CreateTestContract(status: "Expired");
            var draftContract = TestDataFactory.CreateTestContract(status: "Draft");

            // Act
            bool canCreateForActive = activeContract.Status == "Active" && !activeContract.IsExpired;
            bool canCreateForExpired = expiredContract.Status == "Active" && !expiredContract.IsExpired;
            bool canCreateForDraft = draftContract.Status == "Active" && !draftContract.IsExpired;

            // Assert
            Assert.True(canCreateForActive);
            Assert.False(canCreateForExpired);
            Assert.False(canCreateForDraft);
        }
    }
}
// Microsoft, 2026. Entity Framework Core Documentation.[Online]  Available at:
//  https://learn.microsoft.com/en-us/ef/core/