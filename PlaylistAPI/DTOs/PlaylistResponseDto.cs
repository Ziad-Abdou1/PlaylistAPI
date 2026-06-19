namespace PlaylistAPI.DTOs
{
    public class PlaylistResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public List<SongResponseDto> Songs { get; set; } = new List<SongResponseDto>();
    }
}
