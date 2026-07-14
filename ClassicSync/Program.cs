using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{



    static async Task Main()
    {
        string clientId = "d7609632e93247de8795b7d53886abcf";
        string clientSecret = "66286958d9f74faeb18d8d8885393299";
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(clientId, clientSecret);
        var oauth = new OAuthClient(config);
        var tokenResponse = await oauth.RequestToken(request);
        var spotify = new SpotifyClient(config.WithToken(tokenResponse.AccessToken));

        string playlistLink = "https://open.spotify.com/playlist/3FaFbueqz8JuP975z0kciu?si=dc73ecfe806b401b";
        string playlistId = Regex.Match(playlistLink, @"(?:spotify(?::|\.com\/))(?:track|album|artist|playlist|show|episode)(?::|\/)([^?#\s]+)").Groups[1].Value;

        var playlist = await spotify.Playlists.Get(playlistId);
        int? totalTracks = playlist.Tracks.Total; // 33

        if (playlist.Tracks?.Items != null && playlist.Tracks.Items.Count > 0)
        {
            for (int i = 0; i < playlist.Tracks.Items.Count; i++)
            {
                var item = playlist.Tracks.Items[i];
                if (item.Track is FullTrack track)
                {

                    string trackName = track.Name;
                    string artistName = track.Artists.Count > 0 ? track.Artists[0].Name : "Bilinmeyen Sanatçı";
                    Console.WriteLine($"{i + 1}. song is {artistName} - {trackName}");

                }
            }
        }


    }
}