using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class ExchangeTokenFunction
    {
        [FunctionName("ExchangeTokenFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "exchange_token")] HttpRequest req,
            [Table(TableNames.AthletesTable)] CloudTable athletesTable,
            [Queue(QueueNames.RefreshAthletesDetails)] IAsyncCollector<RefreshAthletesDetailsRequest> refreshAthletesDetailsRequests,
            ILogger log)
        {
            string code = req.Query["code"];
            var pinbib = req.Query["pinbib"].ToString().Split(",") ?? new string[0];
            var pin = pinbib.ElementAtOrDefault(0);
            var bib = pinbib.ElementAtOrDefault(1);

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(bib))
            {
                return new BadRequestResult();
            }

            var queryString = new Dictionary<string, string>();
            queryString.Add("client_id", Environment.GetEnvironmentVariable("strava_client_id"));
            queryString.Add("client_secret", Environment.GetEnvironmentVariable("client_secret_id"));
            queryString.Add("code", code);
            queryString.Add("grant_type", "authorization_code");

            var url = QueryHelpers.AddQueryString("https://www.strava.com/oauth/token", queryString);

            using var client = new HttpClient();
            using var httpResponseMessage = await client.PostAsync(url, new StringContent(""));
            httpResponseMessage.EnsureSuccessStatusCode();
            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            var authTokenResponse= JsonSerializer.Deserialize<OAuthTokenResponse>(json);

            var athleteTableEntity = await GetAthleteTableEntity(athletesTable, authTokenResponse.Athlete.Id)
                                        ?? new AthleteTableEntity { PartitionKey = authTokenResponse.Athlete.Id.ToString() };

            athleteTableEntity.Pin = pin;
            athleteTableEntity.Bib = bib;
            athleteTableEntity.AccessToken = authTokenResponse.AccessToken;
            athleteTableEntity.RefreshToken = authTokenResponse.RefreshToken;
            athleteTableEntity.AccessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(authTokenResponse.ExpiresAt);
            await athletesTable.ExecuteAsync(TableOperation.InsertOrReplace(athleteTableEntity));

            await refreshAthletesDetailsRequests.AddAsync(new RefreshAthletesDetailsRequest()
                {AthleteId = authTokenResponse.Athlete.Id});

            return new OkObjectResult($@"Hello, {authTokenResponse.Athlete.Username}
access_token: {authTokenResponse.AccessToken}
refresh_token: {authTokenResponse.RefreshToken}");
        }


        private static async Task<AthleteTableEntity> GetAthleteTableEntity(CloudTable athletesTable, int athleteId)
        {
            TableQuery<AthleteTableEntity> itemStockQuery = new TableQuery<AthleteTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, athleteId.ToString())
            );

            var segmented = await athletesTable.ExecuteQuerySegmentedAsync(itemStockQuery, null);
            return segmented.Results.SingleOrDefault();
        }


        public class OAuthTokenResponse
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
            [JsonPropertyName("athlete")]
            public Athlete Athlete { get; set; }
        }

        public class Athlete
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("username")]
            public string Username { get; set; }
        }


    }
}
