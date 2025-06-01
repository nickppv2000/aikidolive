using AikidoLive.DataModels;
using AikidoLive.Services.Authentication;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services.Email;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AikidoLive.Tests.AuthenticationTests
{
    public class UserRegistrationTests
    {
        private readonly Mock<CosmosClient> _mockCosmosClient;
        private readonly Mock<Container> _mockContainer;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<FeedIterator<UserList>> _mockFeedIterator;
        private readonly Mock<FeedResponse<UserList>> _mockFeedResponse;
        private readonly Mock<IEmailService> _mockEmailService;
        
        public UserRegistrationTests()
        {
            // Setup mocks
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockFeedIterator = new Mock<FeedIterator<UserList>>();
            _mockFeedResponse = new Mock<FeedResponse<UserList>>();
            _mockEmailService = new Mock<IEmailService>();
            
            // Setup email service mock to return success by default
            _mockEmailService.Setup(x => x.SendNewUserNotificationToAdminsAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            
            // Setup configuration sections
            Mock<IConfigurationSection> mockLibrarySection = new Mock<IConfigurationSection>();
            mockLibrarySection.Setup(x => x["document"]).Returns("library");
            
            Mock<IConfigurationSection> mockUsersSection = new Mock<IConfigurationSection>();
            mockUsersSection.Setup(x => x["document"]).Returns("users");
            
            Mock<IConfigurationSection> mockPlaylistsSection = new Mock<IConfigurationSection>();
            mockPlaylistsSection.Setup(x => x["document"]).Returns("playlists");
            
            _mockConfiguration.Setup(x => x.GetSection("libraryDB")).Returns(mockLibrarySection.Object);
            _mockConfiguration.Setup(x => x.GetSection("usersDB")).Returns(mockUsersSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("playlistsDB")).Returns(mockPlaylistsSection.Object);
            
            // Setup container mock to return feed iterator
            _mockContainer.Setup(c => c.GetItemQueryIterator<UserList>(It.IsAny<QueryDefinition>(), 
                    It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(_mockFeedIterator.Object);
            
            // Setup client to return container
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_mockContainer.Object);
            
            // Setup database query
            Mock<FeedIterator<DatabaseProperties>> mockDatabaseIterator = new Mock<FeedIterator<DatabaseProperties>>();
            Mock<FeedResponse<DatabaseProperties>> mockDatabaseResponse = new Mock<FeedResponse<DatabaseProperties>>();
            
            DatabaseProperties dbProps = new DatabaseProperties();
            dbProps.Id = "testdb";
            
            mockDatabaseResponse.Setup(r => r.GetEnumerator())
                .Returns(new List<DatabaseProperties> { dbProps }.GetEnumerator());
            
            mockDatabaseIterator.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);
            
            mockDatabaseIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDatabaseResponse.Object);
            
            _mockCosmosClient.Setup(c => c.GetDatabaseQueryIterator<DatabaseProperties>(
                    It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(mockDatabaseIterator.Object);
            
            // Setup container query
            Mock<Database> mockDatabase = new Mock<Database>();
            Mock<FeedIterator<ContainerProperties>> mockContainerIterator = new Mock<FeedIterator<ContainerProperties>>();
            Mock<FeedResponse<ContainerProperties>> mockContainerResponse = new Mock<FeedResponse<ContainerProperties>>();
            
            ContainerProperties containerProps = new ContainerProperties();
            containerProps.Id = "testcontainer";
            
            mockContainerResponse.Setup(r => r.GetEnumerator())
                .Returns(new List<ContainerProperties> { containerProps }.GetEnumerator());
            
            mockContainerIterator.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);
            
            mockContainerIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockContainerResponse.Object);
            
            mockDatabase.Setup(d => d.GetContainerQueryIterator<ContainerProperties>(
                    It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(mockContainerIterator.Object);
            
            _mockCosmosClient.Setup(c => c.GetDatabase(It.IsAny<string>()))
                .Returns(mockDatabase.Object);
        }
        
        [Fact]
        public async Task RegisterUser_WithValidData_ShouldCreateNewUser()
        {
            // Arrange
            var userList = new UserList
            {
                id = "users",
                Users = new List<AikidoLive.DataModels.User>()
            };
            
            _mockFeedIterator.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);
                
            _mockFeedResponse.Setup(r => r.GetEnumerator())
                .Returns(new List<UserList> { userList }.GetEnumerator());
                
            _mockFeedIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockFeedResponse.Object);
                
            _mockContainer.Setup(c => c.ReplaceItemAsync(
                    It.IsAny<UserList>(),
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey?>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ItemResponse<UserList>>().Object);
            
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object);
            
            var registerModel = new RegisterModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "Password123!"
            };
            
            // Act
            var result = await authService.RegisterUserAsync(registerModel);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("User", result.LastName);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("User", result.Role);
            Assert.NotEqual("Password123!", result.Password); // Password should be hashed
            
            // Verify the user was added to the list
            _mockContainer.Verify(c => c.ReplaceItemAsync(
                It.Is<UserList>(ul => ul.Users.Any(u => u.Email == "test@example.com")),
                It.IsAny<string>(),
                It.IsAny<PartitionKey?>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegisterUser_WithExistingEmail_ShouldReturnNull()
        {
            // Arrange
            var existingUser = new AikidoLive.DataModels.User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "test@example.com",
                Password = "hashedpassword",
                Role = "User"
            };
            
            var userList = new UserList
            {
                id = "users",
                Users = new List<AikidoLive.DataModels.User> { existingUser }
            };
            
            _mockFeedIterator.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);
                
            _mockFeedResponse.Setup(r => r.GetEnumerator())
                .Returns(new List<UserList> { userList }.GetEnumerator());
                
            _mockFeedIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockFeedResponse.Object);
            
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object);
            
            var registerModel = new RegisterModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com", // Same email as existing user
                Password = "Password123!"
            };
            
            // Act
            var result = await authService.RegisterUserAsync(registerModel);
            
            // Assert
            Assert.Null(result);
            
            // Verify that ReplaceItemAsync was not called
            _mockContainer.Verify(c => c.ReplaceItemAsync(
                It.IsAny<UserList>(),
                It.IsAny<string>(),
                It.IsAny<PartitionKey?>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ShouldSendAdminNotification()
        {
            // Arrange
            var userList = new UserList
            {
                id = "users",
                Users = new List<AikidoLive.DataModels.User>()
            };
            
            _mockFeedIterator.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);
                
            _mockFeedResponse.Setup(r => r.GetEnumerator())
                .Returns(new List<UserList> { userList }.GetEnumerator());
                
            _mockFeedIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockFeedResponse.Object);
            
            _mockContainer.Setup(c => c.GetItemQueryIterator<UserList>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(_mockFeedIterator.Object);
            
            _mockContainer.Setup(c => c.ReplaceItemAsync(
                    It.IsAny<UserList>(),
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey?>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ItemResponse<UserList>>().Object);
            
            var dbConnector = new DBServiceConnector(_mockConfiguration.Object, _mockCosmosClient.Object);
            var authService = new AuthService(dbConnector, _mockEmailService.Object);
            
            var registerModel = new RegisterModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "Password123!"
            };
            
            // Act
            var result = await authService.RegisterUserAsync(registerModel);
            
            // Wait a bit for the background task to complete
            await Task.Delay(100);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            
            // Verify that email notification was called
            _mockEmailService.Verify(e => e.SendNewUserNotificationToAdminsAsync(
                "Test", "User", "test@example.com"), Times.Once);
        }
    }
}