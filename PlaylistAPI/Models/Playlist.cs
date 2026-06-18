namespace PlaylistAPI.Models
{
    public class Playlist
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public List<Song> Songs { get; set; }


    }
}
