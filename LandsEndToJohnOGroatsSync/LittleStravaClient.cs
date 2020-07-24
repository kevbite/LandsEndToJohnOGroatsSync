using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

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


        public async Task<SummaryActivity[]> GetLoggedInAthleteActivities(IStravaAuthorization stravaAuthorization, long before, long after)
        {
            if (stravaAuthorization.AccessTokenExpiresAt >= DateTimeOffset.UtcNow)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
            }

            using var httpResponseMessage1 = await SendLoggedInAthleteActivitiesRequest(stravaAuthorization, before, after);

            string json;
            if (httpResponseMessage1.StatusCode == HttpStatusCode.Unauthorized)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
                using var httpResponseMessage2 = await SendLoggedInAthleteActivitiesRequest(stravaAuthorization, before, after); ;
                httpResponseMessage2.EnsureSuccessStatusCode();
                json = await httpResponseMessage2.Content.ReadAsStringAsync();
            }
            else
            {
                httpResponseMessage1.EnsureSuccessStatusCode();
                json = await httpResponseMessage1.Content.ReadAsStringAsync();
            }

            return JsonSerializer.Deserialize<SummaryActivity[]>(json);
        }

        private async Task<HttpResponseMessage> SendLoggedInAthleteActivitiesRequest(IStravaAuthorization stravaAuthorization, long before, long after)
        {

            var queryString = new Dictionary<string, string>();
            queryString.Add("before", before.ToString());
            queryString.Add("after", after.ToString());

            var url = QueryHelpers.AddQueryString("https://www.strava.com/api/v3/athlete/activities", queryString);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stravaAuthorization.AccessToken);

            return await _client.SendAsync(httpRequestMessage);
        }

        public async Task<DetailedActivity> GetActivityById(IStravaAuthorization stravaAuthorization, long activityId)
        {
            if (stravaAuthorization.AccessTokenExpiresAt >= DateTimeOffset.UtcNow)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
            }

            using var httpResponseMessage1 = await SendActivityByIdRequest(stravaAuthorization, activityId);

            string json;
            if (httpResponseMessage1.StatusCode == HttpStatusCode.Unauthorized)
            {
                stravaAuthorization = await RefreshAccessToken(stravaAuthorization);
                using var httpResponseMessage2 = await SendActivityByIdRequest(stravaAuthorization, activityId); ;
                httpResponseMessage2.EnsureSuccessStatusCode();
                json = await httpResponseMessage2.Content.ReadAsStringAsync();
            }
            else
            {
                httpResponseMessage1.EnsureSuccessStatusCode();
                json = await httpResponseMessage1.Content.ReadAsStringAsync();
            }

            return JsonSerializer.Deserialize<DetailedActivity>(json);
        }

        private async Task<HttpResponseMessage> SendActivityByIdRequest(IStravaAuthorization stravaAuthorization, long activityId)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/activities/{activityId}");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stravaAuthorization.AccessToken);

            return await _client.SendAsync(httpRequestMessage);
        }

        public class DetailedActivity
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("start_date")]
            public DateTimeOffset StartDate { get; set; }
        }



        public class SummaryActivity
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            [JsonPropertyName("distance")]
            public float Distance { get; set; }

            [JsonPropertyName("type")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ActivityType Type { get; set; }
        }

        public enum ActivityType
        {
            AlpineSki,
            BackcountrySki,
            Canoeing,
            Crossfit,
            EBikeRide,
            Elliptical,
            Golf,
            Handcycle,
            Hike,
            IceSkate,
            InlineSkate,
            Kayaking,
            Kitesurf,
            NordicSki,
            Ride,
            RockClimbing,
            RollerSki,
            Rowing,
            Run,
            Sail,
            Skateboard,
            Snowboard,
            Snowshoe,
            Soccer,
            StairStepper,
            StandUpPaddling,
            Surfing,
            Swim,
            Velomobile,
            VirtualRide,
            VirtualRun,
            Walk,
            WeightTraining,
            Wheelchair,
            Windsurf,
            Workout,
            Yoga
        }


    }
}