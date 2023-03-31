using SpotifyAPI.Web;
using SpotifyBlacklister.Configuration;

var config = SpotifyClientConfig.CreateDefault();

var request = new ClientCredentialsRequest(ConfigManager.Instance.Data.AppInformation.client_id, ConfigManager.Instance.Data.AppInformation.client_secret);

if (!ConfigManager.Instance.Data.Token.IsTokenValid())
{
    var oauthClient = new OAuthClient();
    var tokenRequest = new AuthorizationCodeRefreshRequest(ConfigManager.Instance.Data.AppInformation.client_id, ConfigManager.Instance.Data.AppInformation.client_secret, ConfigManager.Instance.Data.Token.refresh_token);

    var response = await oauthClient.RequestToken(tokenRequest);

    if (response != null)
    {
        ConfigManager.Instance.Data.Token.access_token = response.AccessToken;
        ConfigManager.Instance.Data.Token.token_type = response.TokenType.ToString().ToLowerInvariant();
        ConfigManager.Instance.Data.Token.refresh_token = response.RefreshToken;
        ConfigManager.Instance.Data.Token.expires_in = (uint)response.ExpiresIn;
        ConfigManager.Instance.Data.Token.created_at = (uint)((DateTimeOffset)response.CreatedAt).ToUnixTimeSeconds();
        ConfigManager.Instance.Data.Token.scope = response.Scope.ToString().ToLowerInvariant();

        ConfigManager.Instance.Save();
    }
}

var spotify = new SpotifyClient(config.WithToken(ConfigManager.Instance.Data.Token.access_token));

// Get the user's liked songs
int Count = 50;
List<SavedTrack> likedSongs = new List<SavedTrack>();
for(int Offset = 0; Offset < Count; Offset += 50)
{
    var likedSongsPage = await spotify.Library.GetTracks(new LibraryTracksRequest { Limit = 50, Offset = Offset });

    if (likedSongsPage.Items != null)
    {
        likedSongs.AddRange(likedSongsPage.Items);
    }

    if (likedSongsPage.Total != null && likedSongsPage.Total.HasValue)
    {
        Count = (int)likedSongsPage.Total.Value;
    }
}

// Go through songs and queue them for deletion
List<int> songQueueForDeletion = new List<int>();
foreach (var item in likedSongs)
{
    var track = item.Track;
    var artists = track.Artists;

    // Check if any of the track's artists are in the targetArtists list
    if (artists.Any(artist => ConfigManager.Instance.Data.Artists.Contains(artist.Name)))
    {
        // Unlike the song if it matches any of the target artists
        if (int.TryParse(track.Id, out int trackId))
        {
            songQueueForDeletion.Add(trackId);
        }

        Console.WriteLine($"Removed {track.Name} by {string.Join(", ", artists.Select(artist => artist.Name))}");
    }
}

// await spotify.Library.RemoveTracks(new LibraryRemoveTracksRequest() { }

Console.WriteLine("Finished processing liked songs.");