using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Data;
using PlaylistAPI.Models;

namespace PlaylistAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlaylistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlaylists()
        {
            var playlists = await _context.Playlists
                .Include(p => p.Songs)
                .ToListAsync();

            return Ok(playlists); 
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPlaylistsByUserId(Guid userId)
        {
            var playlists = await _context.Playlists
                .Include(p => p.Songs)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (playlists == null || playlists.Count == 0)
            {
                return NotFound("No playlist found for this user.");
            }

            return Ok(playlists);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> CreatePlaylist(Guid userId, string playlistName)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = playlistName,
                UserId = userId,
                Songs = new List<Song>()
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlaylistsByUserId), new { userId = playlist.UserId }, playlist);
        }

        [HttpPost("{playlistId}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(Guid playlistId, [FromBody] Song newSong)
        {
            var playlist = await _context.Playlists
                .Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found.");
            }

            newSong.Id = Guid.NewGuid();
            playlist.Songs.Add(newSong);
            await _context.SaveChangesAsync();

            return Ok(newSong);
        }
    }
}