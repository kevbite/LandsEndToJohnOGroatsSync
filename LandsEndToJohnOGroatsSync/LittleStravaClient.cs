using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LandsEndToJohnOGroatsSync
{
    public class LittleStravaClient
    {
        private readonly HttpClient _client;

        public LittleStravaClient()
        {
            _client = new HttpClient();
        }

        public async Task<AuthenticatedAthleteResult> GetAuthenticatedAthlete(IStravaAuthorization stravaAuthorization)
        {
            if (stravaAuthorization.AccessTokenExpiresAt >= DateTimeOffset.UtcNow)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
            }

            using var httpResponseMessage1 = await SendAuthenticatedAthleteRequest(stravaAuthorization);

            string json;
            if (httpResponseMessage1.StatusCode == HttpStatusCode.Unauthorized)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
                using var httpResponseMessage2 = await SendAuthenticatedAthleteRequest(stravaAuthorization);
                httpResponseMessage2.EnsureSuccessStatusCode();
                json = await httpResponseMessage2.Content.ReadAsStringAsync();
            }
            else
            {
                httpResponseMessage1.EnsureSuccessStatusCode();
                json = await httpResponseMessage1.Content.ReadAsStringAsync();
            }

            return JsonSerializer.Deserialize<AuthenticatedAthleteResult>(json);
        }

        private async Task<HttpResponseMessage> SendAuthenticatedAthleteRequest(IStravaAuthorization stravaAuthorization)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://www.strava.com/api/v3/athlete");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stravaAuthorization.AccessToken);

            return await _client.SendAsync(httpRequestMessage);
        }

        private async Task<IStravaAuthorization> RefreshAccessToken(IStravaAuthorization stravaAuthorization)
        {
            var queryString = new Dictionary<string, string>();
            queryString.Add("client_id", Environment.GetEnvironmentVariable("strava_client_id"));
            queryString.Add("client_secret", Environment.GetEnvironmentVariable("client_secret_id"));
            queryString.Add("grant_type", "refresh_token");
            queryString.Add("refresh_token", stravaAuthorization.RefreshToken);

            using var httpResponseMessage = await _client.PostAsync("https://www.strava.com/api/v3/oauth/token", new FormUrlEncodedContent(queryString));
            httpResponseMessage.EnsureSuccessStatusCode();
            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            var authTokenResponse = JsonSerializer.Deserialize<OAuthRefreshTokenResponse>(json);

            stravaAuthorization.AccessToken = authTokenResponse.AccessToken;
            stravaAuthorization.RefreshToken = authTokenResponse.RefreshToken;
            stravaAuthorization.AccessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(authTokenResponse.ExpiresAt);

            return stravaAuthorization;
        }

        public class OAuthRefreshTokenResponse
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
            [JsonPropertyName("expires_at")]
            public int ExpiresAt { get; set; }
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
        }

        public class AuthenticatedAthleteResult
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            [JsonPropertyName("username")]
            public string Username { get; set; }
            [JsonPropertyName("firstname")]
            public string Firstname { get; set; }
            [JsonPropertyName("lastname")]
            public string Lastname { get; set; }
        }

       
    }
}