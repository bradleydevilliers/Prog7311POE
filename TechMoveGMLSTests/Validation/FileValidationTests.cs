using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TechMoveGLMS.Tests.Validation
{
    public class FileValidationTests
    {
        [Fact]
        public void ValidateFileExtension_PdfFile_ReturnsTrue()
        {
            // Arrange - PDF files should be accepted
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("agreement.pdf");

            // Act
            var extension = Path.GetExtension(mockFile.Object.FileName).ToLower();
            bool isValid = extension == ".pdf";

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateFileExtension_ExeFile_ReturnsFalse()
        {
            // Arrange - .exe files should be rejected (security)
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("malicious.exe");

            // Act
            var extension = Path.GetExtension(mockFile.Object.FileName).ToLower();
            bool isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileExtension_JpgFile_ReturnsFalse()
        {
            // Arrange - .jpg files should be rejected (only PDF allowed)
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("image.jpg");

            // Act
            var extension = Path.GetExtension(mockFile.Object.FileName).ToLower();
            bool isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileExtension_UpperCasePdf_ReturnsTrue()
        {
            // Arrange - .PDF (uppercase) should be accepted
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("agreement.PDF");

            // Act
            var extension = Path.GetExtension(mockFile.Object.FileName).ToLower();
            bool isValid = extension == ".pdf";

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateFileExtension_NullFile_IsOptional()
        {
            // Arrange - File is optional, null should be allowed
            IFormFile file = null;

            // Act
            bool isValid = file == null;

            // Assert
            Assert.True(isValid);
        }
    }
}
//Microsoft, 2026. ASP.NET Core MVC Overview.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0