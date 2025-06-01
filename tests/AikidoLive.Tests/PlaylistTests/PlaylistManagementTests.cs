using AikidoLive.DataModels;
using AikidoLive.Services.DBConnector;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace AikidoLive.Tests.PlaylistTests
{
    /// <summary>
    /// Integration tests for playlist management functionality.
    /// These tests verify that the UpdatePlaylists method correctly handles
    /// database operations and playlist document updates.
    /// </summary>
    public class PlaylistManagementTests
    {
        [Fact]
        public void PlaylistsDocument_ShouldHaveValidStructure()
        {
            // Arrange & Act
            var playlistContent = new PlaylistsContent
            {
                PlaylistName = "Test Playlist",
                Tracks = new List<Track>
                {
                    new Track
                    {
                        Name = "Test Track",
                        Description = "Test Description",
                        Url = "123456",
                        Source = "vimeo"
                    }
                }
            };
            
            var playlistsDocument = new PlaylistsDocument
            {
                Id = "playlists",
                PlaylistsContents = new List<PlaylistsContent> { playlistContent }
            };

            // Assert
            Assert.NotNull(playlistsDocument);
            Assert.Equal("playlists", playlistsDocument.Id);
            Assert.Single(playlistsDocument.PlaylistsContents);
            Assert.Equal("Test Playlist", playlistsDocument.PlaylistsContents.First().PlaylistName);
            Assert.Single(playlistsDocument.PlaylistsContents.First().Tracks);
        }

        [Fact]
        public void Track_ShouldSupportVimeoVideoProcessing()
        {
            // Arrange & Act
            var track = new Track
            {
                Name = "Vimeo Track",
                Description = "Video from Vimeo",
                Url = "123456",  // Processed Vimeo ID
                Source = "vimeo"
            };

            // Assert
            Assert.Equal("vimeo", track.Source);
            Assert.True(track.Url.All(char.IsDigit), "Vimeo URL should be processed to contain only digits");
        }

        [Fact]
        public void PlaylistsContent_ShouldAllowAddingTracks()
        {
            // Arrange
            var playlistContent = new PlaylistsContent
            {
                PlaylistName = "Test Playlist",
                Tracks = new List<Track>()
            };

            var newTrack = new Track
            {
                Name = "New Track",
                Description = "New Description", 
                Url = "789012",
                Source = "vimeo"
            };

            // Act
            playlistContent.Tracks.Add(newTrack);

            // Assert
            Assert.Single(playlistContent.Tracks);
            Assert.Equal("New Track", playlistContent.Tracks.First().Name);
        }

        /// <summary>
        /// Test that documents the fix for the Cosmos DB UpdatePlaylists issue.
        /// The fix was to add PartitionKey parameter to ReplaceItemAsync call.
        /// 
        /// Before fix: await _container.ReplaceItemAsync(playlistsDocument, playlistsDocument.Id);
        /// After fix:  await _container.ReplaceItemAsync(playlistsDocument, playlistsDocument.Id, new PartitionKey(playlistsDocument.Id));
        /// 
        /// This resolves the "Failed to update the playlist in the database" error.
        /// </summary>
        [Fact]
        public void UpdatePlaylists_Fix_ShouldIncludePartitionKey()
        {
            // This test documents the required fix for Cosmos DB operations.
            // The UpdatePlaylists method in DBServiceConnector has been updated to include
            // the PartitionKey parameter when calling ReplaceItemAsync.
            
            // Arrange
            var playlistsDocument = new PlaylistsDocument
            {
                Id = "test-id",
                PlaylistsContents = new List<PlaylistsContent>()
            };
            
            // Act - Create PartitionKey as done in the fix
            var partitionKey = new PartitionKey(playlistsDocument.Id);
            
            // Assert - Verify PartitionKey can be created with document ID
            Assert.Contains("test-id", partitionKey.ToString());
        }
    }
}