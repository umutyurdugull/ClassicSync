using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


class Program
{
    static string clientId = "";
    static string clientSecret = "";
    static ClientCredentialsRequest request = new ClientCredentialsRequest(clientId, clientSecret);
    static SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();

    static OAuthClient oauth;
    static ClientCredentialsTokenResponse tokenResponse;
    static SpotifyClient spotify;

    static async Task Main()
    {
      
        oauth = new OAuthClient(config);
        tokenResponse = await oauth.RequestToken(request);
        spotify = new SpotifyClient(config.WithToken(tokenResponse.AccessToken));
        string playlistLink = "https://open.spotify.com/playlist/2ivJYhcthjbXzIVXg7RUmy?si=8fde7fcf4324424d";
        await DownloadSongsAsPlaylist(playlistLink);
    }

    static async Task DownloadSongsAsPlaylist(string playlistLink)
    {

        string playlistId = Regex.Match(playlistLink, @"(?:spotify(?::|\.com\/))(?:track|album|artist|playlist|show|episode)(?::|\/)([^?#\s]+)").Groups[1].Value;
        var playlist = await spotify.Playlists.Get(playlistId);
        int? totalTracks = playlist.Tracks.Total; // 1 

        // Only gets 100 songs, we need to use pagination
        // also downloads files into /bin folder, and we're not letting it pass into GitHub with .gitignore 
        if (playlist.Tracks?.Items != null && playlist.Tracks.Items.Count > 0)
        {
            for (int i = 0; i < playlist.Tracks.Items.Count; i++)
            {
                var item = playlist.Tracks.Items[i];
                if (item.Track is FullTrack track)
                {

                    string trackName = track.Name;
                    string artistName = track.Artists.Count > 0 ? track.Artists[0].Name : "Bilinmeyen Sanatci";
                    Console.WriteLine($"{i + 1}. song is {artistName} - {trackName}");
                    // youtube'a git, +-5 saniye olan videoyu bul, linki kullanıcıya göster 
                    await FindAndDownloadVideoAsync(artistName, trackName, track.DurationMs);
                }
            }
        }
    }

    static async Task FindAndDownloadVideoAsync(string artistName, string trackName, int spotifyDurationMs)
    {
        var youtube = new YoutubeClient();
        string searchQuery = $"{artistName} - {trackName}";
        var spotifyDuration = TimeSpan.FromMilliseconds(spotifyDurationMs);

        YoutubeExplode.Search.VideoSearchResult matchedVideo = null;

        await foreach (var video in youtube.Search.GetVideosAsync(searchQuery))
        {
            if (video.Duration.HasValue)
            {
                double diffSeconds = Math.Abs((video.Duration.Value - spotifyDuration).TotalSeconds);

                if (diffSeconds <= 10)
                {
                    // ya mesela Manifest - Toz Pembe'de 40 saniye falan mı ne delay var, gerçi bize music değil de direkt ses kısmı lazım.
                    matchedVideo = video;
                    break;
                }
            }
        }

        if (matchedVideo != null)
        {
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(matchedVideo.Id);
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (streamInfo != null)
            {
            
                string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                string downloadFolder = Path.Combine(musicFolder, "ClassicSync");

                    if (!Directory.Exists(downloadFolder))
                    {
                        Directory.CreateDirectory(downloadFolder);
                    }

            
                string rawFileName = $"{artistName} - {trackName}";
                string safeFileName = string.Join("_", rawFileName.Split(Path.GetInvalidFileNameChars()));
                
                
                string filePath = Path.Combine(downloadFolder, $"{safeFileName}.{streamInfo.Container}");

                Console.WriteLine($"İniyor: {filePath}");
                await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                Console.WriteLine("İndirdi");
            }
            }
            else
            {
                Console.WriteLine("video yok.");
            }
    }
}
