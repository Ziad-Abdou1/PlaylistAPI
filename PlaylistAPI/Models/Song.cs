public class Song
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<Playlist> Playlists { get; set; } = new List<Playlist>();
}