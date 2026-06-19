using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Models;

namespace PlaylistAPI.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Playlists.Any()) return;

            var mainUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var mainPlaylistId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var mainSongId = Guid.Parse("33333333-3333-3333-3333-333333333333");    

            var defaultUser = new User { Id = mainUserId, Name = "Ziad Abdou" };
            context.Users.Add(defaultUser);

            var song1 = new Song { Id = mainSongId, Title = "Bohemian Rhapsody" };
            var song2 = new Song { Id = Guid.NewGuid(), Title = "Hotel California" };
            context.Songs.AddRange(song1, song2);

            var initialPlaylist = new Playlist
            {
                Id = mainPlaylistId,
                Name = "Coding Focus Mix",
                UserId = mainUserId,
                Songs = new List<Song> { song1 }
            };

            context.Playlists.Add(initialPlaylist);
            context.SaveChanges();
        }
    }
}