using QuickConfig;
using SpotifyAPI.Web;
using SpotifyBlacklister.Configuration;
using SpotifyBlacklister.Helpers;

ConfigManager configManager = new ConfigManager(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
Config config = configManager.GetConfig<Config>("Config");

SpotifyClientConfig spotifyConfig = SpotifyClientConfig.CreateDefault();

var request = new ClientCredentialsRequest(config.AppInformation.client_id, config.AppInformation.client_secret);

if (!config.Token.IsTokenValid())
{
    var oauthClient = new OAuthClient();
    var tokenRequest = new AuthorizationCodeRefreshRequest(config.AppInformation.client_id, config.AppInformation.client_secret, config.Token.refresh_token);

    var response = await oauthClient.RequestToken(tokenRequest);

    if (response != null)
    {
        config.Token.access_token = response.AccessToken;
        config.Token.token_type = response.TokenType.ToString().ToLowerInvariant();
        if (!String.IsNullOrEmpty(response.RefreshToken))
        {
            config.Token.refresh_token = response.RefreshToken;
        }
        config.Token.expires_in = (uint)response.ExpiresIn;
        config.Token.created_at = (uint)((DateTimeOffset)response.CreatedAt).ToUnixTimeSeconds();
        config.Token.scope = response.Scope.ToString().ToLowerInvariant();

        config.Save();
    }
}

var spotify = new SpotifyClient(spotifyConfig.WithToken(config.Token.access_token));

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
    if (artists.Any(artist => config.BlacklistedArtists.Any(blacklistedArtist => blacklistedArtist.Name == artist.Name)))
    {
        foreach (var artist in artists)
        {
            var blacklistedArtist = config.BlacklistedArtists.FirstOrDefault(a => a.Name == artist.Name);
            if (blacklistedArtist != null && blacklistedArtist.ExceptedSongs.Contains(item.Track.Name))
            {
                Console.WriteLine($"Skipped {item.Track.Name} by {artist.Name} because it is in the whitelisted songs list for this artist.");
                continue;
            }
        }

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
    EmailHelper.SendEmailWithDeletedSongs(config, songsDeleted);
}

// Finished
Console.WriteLine("Finished processing liked songs.");