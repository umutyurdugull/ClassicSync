using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


class Program
{



    static async Task Main()
    {

        // Spotify
        string clientId = "d7609632e93247de8795b7d53886abcf";
        string clientSecret = "66286958d9f74faeb18d8d8885393299";
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(clientId, clientSecret);
        var oauth = new OAuthClient(config);
        var tokenResponse = await oauth.RequestToken(request);
        var spotify = new SpotifyClient(config.WithToken(tokenResponse.AccessToken));
        string playlistLink = "https://open.spotify.com/playlist/2ivJYhcthjbXzIVXg7RUmy?si=8fde7fcf4324424d";
        string playlistId = Regex.Match(playlistLink, @"(?:spotify(?::|\.com\/))(?:track|album|artist|playlist|show|episode)(?::|\/)([^?#\s]+)").Groups[1].Value;
        var playlist = await spotify.Playlists.Get(playlistId);
        int? totalTracks = playlist.Tracks.Total; // 1 






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

                if (diffSeconds <= 5)
                {
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
                string safeFileName = string.Join("_", trackName.Split(System.IO.Path.GetInvalidFileNameChars()));
                string filePath = $"{safeFileName}.{streamInfo.Container}";

                Console.WriteLine($"iniyor: {filePath}...");
                await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                Console.WriteLine("indirdi!");
            }
        }
        else
        {
            Console.WriteLine("video yok.");
        }
    }
}
