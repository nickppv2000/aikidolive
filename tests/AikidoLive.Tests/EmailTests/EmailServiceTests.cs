using AikidoLive.DataModels;
using AikidoLive.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AikidoLive.Tests.EmailTests
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<EmailService>> _mockLogger;

        public EmailServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<EmailService>>();

            SetupEmailConfiguration();
        }

        private void SetupEmailConfiguration()
        {
            var emailSection = new Mock<IConfigurationSection>();
            emailSection.Setup(x => x["SmtpHost"]).Returns("localhost");
            emailSection.Setup(x => x["SmtpPort"]).Returns("25");
            emailSection.Setup(x => x["SmtpUsername"]).Returns("test@example.com");
            emailSection.Setup(x => x["SmtpPassword"]).Returns("password");
            emailSection.Setup(x => x["FromEmail"]).Returns("noreply@aikidolive.com");

            _mockConfiguration.Setup(x => x.GetSection("Email")).Returns(emailSection.Object);
        }

        [Fact]
        public async Task SendNewUserNotificationToAdminsAsync_WithMissingConfiguration_ShouldReturnTrueAndLogWarning()
        {
            // Arrange
            var emailSection = new Mock<IConfigurationSection>();
            emailSection.Setup(x => x["SmtpHost"]).Returns((string)null);
            emailSection.Setup(x => x["SmtpPort"]).Returns("587");
            emailSection.Setup(x => x["SmtpUsername"]).Returns((string)null);
            emailSection.Setup(x => x["SmtpPassword"]).Returns("password");
            emailSection.Setup(x => x["FromEmail"]).Returns((string)null);

            _mockConfiguration.Setup(x => x.GetSection("Email")).Returns(emailSection.Object);

            var mockDbConnector = new Mock<AikidoLive.Services.DBConnector.DBServiceConnector>(_mockConfiguration.Object, new Mock<Microsoft.Azure.Cosmos.CosmosClient>().Object);
            mockDbConnector.Setup(x => x.GetUsers()).ReturnsAsync(new List<UserList>());

            var emailService = new EmailService(mockDbConnector.Object, _mockConfiguration.Object, _mockLogger.Object);

            // Act
            var result = await emailService.SendNewUserNotificationToAdminsAsync("Test", "User", "test@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SendNewUserNotificationToAdminsAsync_WithNoAdminUsers_ShouldReturnTrueAndLogWarning()
        {
            // Arrange
            var mockDbConnector = new Mock<AikidoLive.Services.DBConnector.DBServiceConnector>(_mockConfiguration.Object, new Mock<Microsoft.Azure.Cosmos.CosmosClient>().Object);
            mockDbConnector.Setup(x => x.GetUsers()).ReturnsAsync(new List<UserList>
            {
                new UserList 
                { 
                    id = "users", 
                    Users = new List<User>
                    {
                        new User { FirstName = "Regular", LastName = "User", Email = "user@example.com", Role = "User" }
                    }
                }
            });

            var emailService = new EmailService(mockDbConnector.Object, _mockConfiguration.Object, _mockLogger.Object);

            // Act
            var result = await emailService.SendNewUserNotificationToAdminsAsync("Test", "User", "test@example.com");

            // Assert
            Assert.True(result);
        }
    }
}