using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AikidoLive.DataModels;
using AikidoLive.Services.Authentication;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using User = AikidoLive.DataModels.User;

namespace AikidoLive.Tests.AuthenticationTests
{
    public class EmailFunctionalityTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<CosmosClient> _mockCosmosClient;
        private readonly Mock<Container> _mockContainer;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockRequest;

        public EmailFunctionalityTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockEmailService = new Mock<IEmailService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockRequest = new Mock<HttpRequest>();

            // Setup basic configuration
            var cosmosSection = new Mock<IConfigurationSection>();
            cosmosSection.Setup(x => x["DatabaseName"]).Returns("testdb");
            _mockConfiguration.Setup(x => x.GetSection("CosmosDb")).Returns(cosmosSection.Object);

            // Setup HTTP context for URL generation
            _mockRequest.Setup(x => x.Scheme).Returns("https");
            _mockRequest.Setup(x => x.Host).Returns(new HostString("localhost:5001"));
            _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            // Setup database mock
            var database = new Mock<Database>();
            database.Setup(d => d.GetContainer(It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockCosmosClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(database.Object);

            // Setup user data
            var userList = new UserList
            {
                Users = new List<User>
                {
                    new User
                    {
                        FirstName = "Test",
                        LastName = "User",
                        Email = "test@example.com",
                        Role = "User",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        IsEmailConfirmed = false,
                        EmailConfirmationToken = "test-token-123",
                        EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(7)
                    }
                }
            };

            var mockContainerIterator = new Mock<FeedIterator<UserList>>();
            mockContainerIterator.SetupSequence(x => x.HasMoreResults).Returns(true).Returns(false);
            mockContainerIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<FeedResponse<UserList>>().Object);

            var feedResponse = new Mock<FeedResponse<UserList>>();
            feedResponse.Setup(x => x.GetEnumerator()).Returns(new List<UserList> { userList }.GetEnumerator());
            mockContainerIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponse.Object);

            _mockContainer.Setup(c => c.GetItemQueryIterator<UserList>(
                    It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(mockContainerIterator.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldSendConfirmationEmail()
        {
            // Arrange
            _mockEmailService.Setup(x => x.SendConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object, 
                _mockHttpContextAccessor.Object, _mockConfiguration.Object);

            var registerModel = new RegisterModel
            {
                FirstName = "New",
                LastName = "User",
                Email = "newuser@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await authService.RegisterUserAsync(registerModel);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsEmailConfirmed);
            Assert.NotNull(result.EmailConfirmationToken);
            Assert.NotNull(result.EmailConfirmationTokenExpiry);

            // Verify email was sent
            _mockEmailService.Verify(x => x.SendConfirmationEmailAsync(
                "newuser@example.com", 
                "New", 
                It.Is<string>(link => link.Contains("ConfirmEmail") && link.Contains("token="))), 
                Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithValidToken_ShouldConfirmEmail()
        {
            // Arrange
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object, 
                _mockHttpContextAccessor.Object, _mockConfiguration.Object);

            // Act
            var result = await authService.ConfirmEmailAsync("test-token-123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithInvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object, 
                _mockHttpContextAccessor.Object, _mockConfiguration.Object);

            // Act
            var result = await authService.ConfirmEmailAsync("invalid-token");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_WithValidEmail_ShouldSendResetEmail()
        {
            // Arrange
            _mockEmailService.Setup(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // First confirm the user's email
            var userList = new UserList
            {
                Users = new List<User>
                {
                    new User
                    {
                        FirstName = "Test",
                        LastName = "User",
                        Email = "test@example.com",
                        Role = "User",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        IsEmailConfirmed = true // Email is confirmed
                    }
                }
            };

            var mockContainerIterator = new Mock<FeedIterator<UserList>>();
            mockContainerIterator.SetupSequence(x => x.HasMoreResults).Returns(true).Returns(false);
            var feedResponse = new Mock<FeedResponse<UserList>>();
            feedResponse.Setup(x => x.GetEnumerator()).Returns(new List<UserList> { userList }.GetEnumerator());
            mockContainerIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponse.Object);

            _mockContainer.Setup(c => c.GetItemQueryIterator<UserList>(
                    It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(mockContainerIterator.Object);

            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object, 
                _mockHttpContextAccessor.Object, _mockConfiguration.Object);

            // Act
            var result = await authService.SendPasswordResetEmailAsync("test@example.com");

            // Assert
            Assert.True(result);

            // Verify reset email was sent
            _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
                "test@example.com", 
                "Test", 
                It.Is<string>(link => link.Contains("ResetPassword") && link.Contains("token="))), 
                Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithUnconfirmedEmail_ShouldReturnNull()
        {
            // Arrange
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object, 
                _mockHttpContextAccessor.Object, _mockConfiguration.Object);

            // Act
            var result = await authService.AuthenticateAsync("test@example.com", "Password123!");

            // Assert
            Assert.Null(result); // Should return null because email is not confirmed
        }
    }
}