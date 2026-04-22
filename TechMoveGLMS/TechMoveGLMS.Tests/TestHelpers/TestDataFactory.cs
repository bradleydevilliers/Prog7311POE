
// (Microsoft,2026)

using TechMoveGLMS.Models.Entities;

namespace TechMoveGLMS.Tests.TestHelpers
{
    public static class TestDataFactory
    {
        public static Client CreateTestClient(int id = 1)
        {
            return new Client
            {
                Id = id,
                Name = $"Test Client {id}",
                ContactDetails = $"test{id}@example.com",
                Region = "Test Region"
            };
        }

        public static Contract CreateTestContract(int id = 1, int clientId = 1, string status = "Active")
        {
            return new Contract
            {
                Id = id,
                ClientId = clientId,
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now.AddDays(335),
                ServiceLevel = "Basic",
                Status = status,
                SignedAgreementPath = $"/uploads/test_{id}.pdf"
            };
        }

        public static ServiceRequest CreateTestServiceRequest(int id = 1, int contractId = 1)
        {
            return new ServiceRequest
            {
                Id = id,
                ContractId = contractId,
                Description = $"Test Service Request {id}",
                ServiceLevel = "Basic",
                Status = "Draft",
                LocalCost = 1850.00m,
                CreatedAt = DateTime.Now
            };
        }

        public static User CreateTestUser(int id = 1, string role = "Admin", int? clientId = null)
        {
            return new User
            {
                Id = id,
                Email = $"test{id}@techmove.com",
                PasswordHash = "hashedpassword",
                Role = role,
                ClientId = clientId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }
    }
}
// -Microsoft, 2026. Unit Testing in .NET Core with xUnit. Available at: 
//https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test 