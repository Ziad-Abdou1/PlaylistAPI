using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs;
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

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> CreatePlaylist(Guid userId,CreatePlaylistDto request)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                UserId = userId,
                Songs = new List<Song>()
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            var response = new PlaylistResponseDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                UserId = playlist.UserId
            };

            return CreatedAtAction(nameof(GetPlaylistsByUserId), new { userId = playlist.UserId }, response);
        }


        [HttpPost("{playlistId}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(Guid playlistId,AddSongDto request)
        {
            var playlist = await _context.Playlists
                .Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found");
            }

            var songToAdd = await _context.Songs
                .FirstOrDefaultAsync(s => s.Title == request.Title);

           
            if (songToAdd == null)
            {
                songToAdd = new Song
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title
                };
                _context.Songs.Add(songToAdd);
            }

            if (!playlist.Songs.Any(s => s.Id == songToAdd.Id))
            {
                playlist.Songs.Add(songToAdd);
                await _context.SaveChangesAsync();
            }

            return Ok(new SongResponseDto { Id = songToAdd.Id, Title = songToAdd.Title });
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


        [HttpPut("{playlistId}")]
        public async Task<IActionResult> UpdatePlaylist(Guid playlistId, CreatePlaylistDto request)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null)
            {
                return NotFound("Playlist not found.");
            }

            playlist.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok(new PlaylistResponseDto { Id = playlist.Id, Name = playlist.Name, UserId = playlist.UserId });
        }

        [HttpDelete("{playlistId}/user/{userId}")]
        public async Task<IActionResult> DeletePlaylist(Guid playlistId, Guid userId)
        {
            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
            {
                return NotFound("Playlist not found.");
            }
            if (playlist.UserId != userId)
            {
                return Forbid("You do not have permission to delete this playlist.");
            }

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(Guid playlistId, Guid songId)
        { 
            var playlist = await _context.Playlists
        .Include(p => p.Songs)
        .FirstOrDefaultAsync(p => p.Id == playlistId);
            if (playlist == null)
            {
                return NotFound("Playlist not found.");
            }
            var songToRemove = playlist.Songs.FirstOrDefault(s => s.Id == songId);

            if (songToRemove == null)
            {
                return NotFound("Song is not in this playlist.");
            }
            playlist.Songs.Remove(songToRemove);
            await _context.SaveChangesAsync();

            return NoContent();

        }
        }
}