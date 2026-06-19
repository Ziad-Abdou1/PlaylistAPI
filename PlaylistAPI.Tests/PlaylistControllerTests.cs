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

        // --- NEW TESTS START HERE ---
        [Fact]
        public async Task CreatePlaylist_ReturnsCreatedAtAction_WhenValidDataProvided()
        {
            var context = GetInMemoryDbContext();
            var controller = new PlaylistController(context);
            var userId = Guid.NewGuid();
            var newPlaylistDto = new CreatePlaylistDto { Name = "Workout Mix" };

            var result = await controller.CreatePlaylist(userId, newPlaylistDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            var createdPlaylist = Assert.IsType<PlaylistResponseDto>(createdResult.Value);

            Assert.Equal("Workout Mix", createdPlaylist.Name);
            Assert.Equal(userId, createdPlaylist.UserId);
            Assert.Single(context.Playlists);
        }


        [Fact]
        public async Task DeletePlaylist_ReturnsNotFound_WhenPlaylistDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var controller = new PlaylistController(context);
            var randomPlaylistId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var result = await controller.DeletePlaylist(randomPlaylistId, userId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Playlist not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_ReturnsNoContent_WhenSuccessful()
        {
            var context = GetInMemoryDbContext();
            var playlistId = Guid.NewGuid();
            var songId = Guid.NewGuid();
            var song = new Song { Id = songId, Title = "Stairway to Heaven" };
            var playlist = new Playlist { Id = playlistId, Name = "Classics", UserId = Guid.NewGuid() };

            playlist.Songs.Add(song);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);
            var result = await controller.RemoveSongFromPlaylist(playlistId, songId);
            Assert.IsType<NoContentResult>(result);

            var updatedPlaylist = await context.Playlists.Include(p => p.Songs).FirstAsync();
            Assert.Empty(updatedPlaylist.Songs);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_ReturnsNotFound_WhenSongNotInPlaylist()
        {
            var context = GetInMemoryDbContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist { Id = playlistId, Name = "Classics", UserId = Guid.NewGuid() };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var controller = new PlaylistController(context);
            var randomSongId = Guid.NewGuid();

            var result = await controller.RemoveSongFromPlaylist(playlistId, randomSongId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Song is not in this playlist.", notFoundResult.Value);
        }
    }
}