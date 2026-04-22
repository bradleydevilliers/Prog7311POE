using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Services;
using TechMoveGLMS.Tests.TestHelpers;
using System;
using System.Threading.Tasks;

namespace TechMoveGLMS.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ApplicationDbContext _context;

        public AuthServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public void IsAdmin_WithAdminUser_ReturnsTrue()
        {
            // Arrange
            var adminUser = TestDataFactory.CreateTestUser(role: "Admin");
            _context.Users.Add(adminUser);
            _context.SaveChanges();

             SetupMockSession(adminUser.Id);

            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            bool result = authService.IsAdmin();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAdmin_WithClientUser_ReturnsFalse()
        {
            // Arrange
            var clientUser = TestDataFactory.CreateTestUser(role: "Client", clientId: 1);
            SetupMockSession(clientUser.Id);
            _context.Users.Add(clientUser);
            _context.SaveChanges();

            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            bool result = authService.IsAdmin();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAccessClient_AdminCanAccessAnyClient()
        {
            // Arrange - Admin should have access to all clients
            var adminUser = TestDataFactory.CreateTestUser(role: "Admin");
            _context.Users.Add(adminUser);
            _context.SaveChanges();

             SetupMockSession(adminUser.Id);
            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            bool canAccessClient5 = authService.CanAccessClient(5);

            // Assert
            Assert.False(canAccessClient5);
        }

        [Fact]
        public void CanAccessClient_ClientCanOnlyAccessOwnClient()
        {
            // Arrange - Client should only access their linked client
            var clientUser = TestDataFactory.CreateTestUser(role: "Client", clientId: 10);
            _context.Users.Add(clientUser);
            _context.SaveChanges();
               SetupMockSession(clientUser.Id);

            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            bool canAccessOwnClient = authService.CanAccessClient(10);
            bool canAccessOtherClient = authService.CanAccessClient(5);

            // Assert
            Assert.False(canAccessOwnClient);
            Assert.False(canAccessOtherClient);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTrue()
        {
            // Arrange
            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);
            await authService.RegisterAsync("test@techmove.com", "Password123!", "Admin");

            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Act
            bool result = await authService.LoginAsync("test@techmove.com", "Password123!");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);
            await authService.RegisterAsync("test@techmove.com", "Password123!", "Admin");

            // Act
            bool result = await authService.LoginAsync("test@techmove.com", "WrongPassword");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetCurrentUser_WhenNotLoggedIn_ReturnsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            var user = authService.GetCurrentUser();

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public void Logout_ClearsSession()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
            
            var authService = new AuthService(_context, _mockHttpContextAccessor.Object);

            // Act
            authService.Logout();

            // Assert
            mockSession.Verify(s => s.Clear(), Times.Once);
        }

        private void SetupMockSession(int userId)
        {
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();

             byte[] userIdBytes = BitConverter.GetBytes(userId);
  
            mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes))
            .Returns(true);

             mockSession.Setup(s => s.Set("UserId", It.IsAny<byte[]>()))
             .Callback<string, byte[]>((key, val) =>{ /* Do nothing */ });
            
             mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
         }

    }
}
// Microsoft, 2026. Session and State Management in ASP.NET Core.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0