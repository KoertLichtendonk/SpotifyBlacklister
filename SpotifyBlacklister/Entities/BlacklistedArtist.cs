namespace SpotifyBlacklister.Entities
{
    public class BlacklistedArtist
    {
        public string Name { get; set; }
        public List<string> ExceptedSongs { get; set; }

        public BlacklistedArtist()
        {
            this.Name = String.Empty;
            this.ExceptedSongs = new List<string>();
        }
    }
}