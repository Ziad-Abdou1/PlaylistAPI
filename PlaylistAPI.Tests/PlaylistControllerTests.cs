using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Controllers;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs;
using PlaylistAPI.Models;

namespace PlaylistAPI.Tests
{
    public class PlaylistControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // ==========================================
        // 1. GET PLAYLISTS TESTS
        // ==========================================
        [Fact]
        public async Task GetPlaylistsByUserId_ReturnsOk_WhenPlaylistsExist()
        {
            var context = GetInMemoryDbContext();
            var userId = Guid.NewGuid();
            context.Playlists.Add(new Playlist { Id = Guid.NewGuid(), Name = "Chill Vibes", UserId = userId });
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);

            var result = await controller.GetPlaylistsByUserId(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylists = Assert.IsAssignableFrom<List<Playlist>>(okResult.Value);
            Assert.Single(returnedPlaylists);
        }

        [Fact]
        public async Task GetPlaylistsByUserId_ReturnsNotFound_WhenUserHasNoPlaylists()
        {
            var context = GetInMemoryDbContext();
            var controller = new PlaylistController(context);
            var randomUserId = Guid.NewGuid();

            var result = await controller.GetPlaylistsByUserId(randomUserId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No playlist found for this user.", notFoundResult.Value);
        }

        // ==========================================
        // 2. ADD SONG TESTS
        // ==========================================

        [Fact]
        public async Task AddSongToPlaylist_ReturnsNotFound_WhenPlaylistDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var controller = new PlaylistController(context);
            var request = new AddSongDto { Title = "Bohemian Rhapsody" };
            var result = await controller.AddSongToPlaylist(Guid.NewGuid(), request);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Playlist not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddSongToPlaylist_AddsSong_WhenPlaylistExists()
        {
            var context = GetInMemoryDbContext();
            var playlistId = Guid.NewGuid();
            context.Playlists.Add(new Playlist { Id = playlistId, Name = "Rock", UserId = Guid.NewGuid() });
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);
            var request = new AddSongDto { Title = "Hotel California" };

            var result = await controller.AddSongToPlaylist(playlistId, request);

            Assert.IsType<OkObjectResult>(result);
            var updatedPlaylist = await context.Playlists.Include(p => p.Songs).FirstAsync();
            Assert.Single(updatedPlaylist.Songs);
            Assert.Equal("Hotel California", updatedPlaylist.Songs.First().Title);
        }

        // ==========================================
        // 3. DELETE PLAYLIST TESTS (Testing Edge Cases)
        // ==========================================

        [Fact]
        public async Task DeletePlaylist_ReturnsForbid_WhenWrongUserTriesToDelete()
        {
            var context = GetInMemoryDbContext();
            var playlistId = Guid.NewGuid();
            var actualOwnerId = Guid.NewGuid();
            var maliciousUserId = Guid.NewGuid(); 

            context.Playlists.Add(new Playlist { Id = playlistId, Name = "My Private Playlist", UserId = actualOwnerId });
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);
            var result = await controller.DeletePlaylist(playlistId, maliciousUserId);
            var forbidResult = Assert.IsType<ForbidResult>(result);
            Assert.Single(context.Playlists);
        }

        [Fact]
        public async Task DeletePlaylist_ReturnsNoContent_WhenSuccessful()
        {
            var context = GetInMemoryDbContext();
            var playlistId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            context.Playlists.Add(new Playlist { Id = playlistId, Name = "Delete Me", UserId = userId });
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);
            var result = await controller.DeletePlaylist(playlistId, userId);
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Playlists);
        }
    }
}