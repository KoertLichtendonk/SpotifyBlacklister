using SpotifyAPI.Web;
using SpotifyBlacklister.Configuration;
using SpotifyBlacklister.Helpers;

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
        if (!String.IsNullOrEmpty(response.RefreshToken))
        {
            ConfigManager.Instance.Data.Token.refresh_token = response.RefreshToken;
        }
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
List<string> songQueueForDeletion = new List<string>();
Dictionary<string, string> songsDeleted = new Dictionary<string, string>();
foreach (var item in likedSongs)
{
    var track = item.Track;
    var artists = track.Artists;

    // Check if any of the track's artists are in the targetArtists list
    if (artists.Any(artist => ConfigManager.Instance.Data.Artists.Contains(artist.Name)))
    {
        // Unlike the song if it matches any of the target artists
        songQueueForDeletion.Add(track.Id);
        songsDeleted.TryAdd(track.Name, String.Join(", ", artists.Select(artist => artist.Name)));

        Console.WriteLine($"Removed {track.Name} by {String.Join(", ", artists.Select(artist => artist.Name))}");
    }
}

// Check if any songs can be deleted
if (songQueueForDeletion.Count > 0)
{
    // Delete songs
    await spotify.Library.RemoveTracks(new LibraryRemoveTracksRequest(songQueueForDeletion));

    // Send e-mail
    EmailHelper.SendEmailWithDeletedSongs(songsDeleted);
}

// Finished
Console.WriteLine("Finished processing liked songs.");