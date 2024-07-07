using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Eq.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable SYSLIB0013

namespace Eq;

public class SpotifyClient
{
    public const string ClientId = "4fef1cbb12fa458eabfc76ebe6954d1e";
    
    private const string ClientSecret = "a784b19e41f0464b8480226fed3af16a";
    
    public const string RedirectUri = "http://localhost:5000/api/callback";

    private readonly HttpClient _httpClient = new();

    public async Task<TrackModel?> SearchForTrack(string trackName)
    {
        var accessToken = await GetAccessToken();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.spotify.com/v1/search?type=track&limit=1&q={Uri.EscapeUriString(trackName)}");
        
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var trackResponse = await _httpClient.SendAsync(request);
        var responseContent = await trackResponse.Content.ReadAsStringAsync();

        dynamic? obj = JsonConvert.DeserializeObject(responseContent);

        if (obj == null || obj!.tracks == null || obj!.tracks.items == null || !obj!.tracks.items.HasValues)
        {
            return null;
        }
        
        string trackId = obj!.tracks.items[0].id;
        string imageUrl = obj.tracks.items[0].album.images[0].url;

        var response = new TrackModel { SpotifyId = trackId, ImageUrl = imageUrl };

        return response;
    }

    public async Task<string> CreatePlaylist(IEnumerable<TrackModel> tracks, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        dynamic obj = JsonConvert.DeserializeObject(responseContent)!;
        string userId = obj.id;
        
        var createPlaylistData = new 
        {
            name = $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}",
            desciption = "converted from vk playlist"
        };

        request = new HttpRequestMessage(HttpMethod.Post, $"https://api.spotify.com/v1/users/{userId}/playlists");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        request.Content = new StringContent(
            JsonConvert.SerializeObject(createPlaylistData), 
            Encoding.UTF8, 
            "application/json");
        
        response = await _httpClient.SendAsync(request);
        
        var createPlaylistResponseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        
        var playlistId = createPlaylistResponseData!.id.Value;
        var playlistUrl = createPlaylistResponseData.external_urls.spotify.Value;
        
        var addTracksUri = new Uri($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
        
        var trackUris = tracks.Select(t => "spotify:track:" + t.SpotifyId).ToList();

        const int chunkSize = 100;
        for (var i = 0; i < trackUris.Count; i += chunkSize)
        {
            var addTracksRequest = new HttpRequestMessage(HttpMethod.Post, addTracksUri);
            addTracksRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            
            var chunk = trackUris.Skip(i).Take(chunkSize).ToList();

            var addTracksData = new 
            {
                uris = chunk
            };

            addTracksRequest.Content = new StringContent(
                JsonConvert.SerializeObject(addTracksData), 
                Encoding.UTF8, 
                "application/json");

            await _httpClient.SendAsync(addTracksRequest);
            
            await Task.Delay(2000);
        }

        return playlistUrl;
    }

    public async Task<SpotifyTokenResponse> ObtainAccessToken(string authToken)
    {
        using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://accounts.spotify.com/api/token");
        
        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}"));
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

        request.Content = new StringContent($"grant_type=authorization_code&code={authToken}&redirect_uri={Uri.EscapeDataString(RedirectUri)}");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
    
        var response = await _httpClient.SendAsync(request);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        var spotifyTokenResponse = JsonConvert.DeserializeObject<SpotifyTokenResponse>(jsonResponse);

        return spotifyTokenResponse;
    }

    private async Task<string?> GetAccessToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"))}");

        var bodyData = new List<KeyValuePair<string?, string?>>
        {
            new("grant_type", "client_credentials")
        };

        request.Content = new FormUrlEncodedContent(bodyData);

        var accessTokenResponse = await _httpClient.SendAsync(request);
        var accessToken = JObject.Parse(await accessTokenResponse.Content.ReadAsStringAsync())["access_token"]
            ?.ToString();

        return accessToken;
    }
}