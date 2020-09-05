using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LandsEndToJohnOGroatsSync
{
    public class LandsEnd3FireBaseAppClient
    {
        
        private static readonly Random Random = new Random();
        private static readonly HttpClient Client = new HttpClient();

        static LandsEnd3FireBaseAppClient()
        {
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");
            Client.DefaultRequestHeaders.Add("referer", "https://lejog-3.endtoend.run/");
        }

        public async Task SubmitData(ILandsEnd3FireBaseAppAthleteData data, DateTime dateTime, double miles)
        {
            var url =
                $"https://script.google.com/macros/s/AKfycbydBsopnqrdufjuTDZLEPm5xbjdChngj9kl36xEXLrifHVRUs8/exec?callback=jQuery33109242062575738661_1595280391802&primary[0][]={data.Name}&primary[0][]={data.Pin}&primary[0][]={data.Bib}&primary[0][]={dateTime:dd'/'MM'/'yyyy}&primary[0][]={miles}&primary[0][]=&action=submitMileage&_={Random.Next()}";
    
            var responseMessage = await Client.GetAsync(url);
            responseMessage.EnsureSuccessStatusCode();
            
            var json = await responseMessage.Content.ReadAsStringAsync();
            if (json.Contains("Error"))
            {
                throw new Exception("Error Submitting Data: \n" + json);
            }
            var formResponse = JsonSerializer.Deserialize<FormResponse>(json.Substring(json.IndexOf('(') + 1).TrimEnd(')'));
            
            await UpdateLeaderboardDetails(data, dateTime, miles, formResponse);

            var totalMiles = await CalculateTotalMilesFromLeaderboardDetails(data, formResponse);

            await UpdateLeaderboard(data, totalMiles, formResponse);
        }

        private static async Task UpdateLeaderboard(ILandsEnd3FireBaseAppAthleteData data, decimal totalMiles,
             FormResponse formResponse)
        {
            var totalMilesLeft = 874 - totalMiles;
            var putLeaderboardResponse = await Client.PutAsync(
                $"https://lands-end-3.firebaseio.com/leaderboard/{data.Bib}/.json?auth={formResponse.Key}",
                new StringContent(@$"[
    ""{data.Name}"",
    ""{data.Bib}"",
    ""{totalMiles}"",
    ""{totalMilesLeft}"",
    99999
]", Encoding.UTF8, "application/json"));

            putLeaderboardResponse.EnsureSuccessStatusCode();
        }

        private static async Task<decimal> CalculateTotalMilesFromLeaderboardDetails(ILandsEnd3FireBaseAppAthleteData data,
            FormResponse formResponse)
        {
            var leaderboardDetailsResponse = await Client.GetAsync(
                $"https://lands-end-3.firebaseio.com/leaderboardDetails/{data.Bib}/.json?auth={formResponse.Key}");

            leaderboardDetailsResponse.EnsureSuccessStatusCode();
            var leadboardDetailsJson = await leaderboardDetailsResponse.Content.ReadAsStringAsync();

            var total = JsonDocument.Parse(leadboardDetailsJson).RootElement.EnumerateObject()
                .SelectMany(year => year.Value.EnumerateObject())
                .SelectMany(day => day.Value.ValueKind switch
                {
                    JsonValueKind.Array => day.Value.EnumerateArray(),
                    JsonValueKind.Object => day.Value.EnumerateObject().Select(x => x.Value),
                    _ => throw new ArgumentOutOfRangeException()
                })
                .Where(leaf => leaf.ValueKind == JsonValueKind.String)
                .Select(leaf => decimal.Parse(leaf.GetString()))
                .Sum();

            return total;
        }

        private static async Task UpdateLeaderboardDetails(ILandsEnd3FireBaseAppAthleteData data, DateTime dateTime,
            double miles, FormResponse formResponse)
        {
            var putLeaderboardDetailsResponse = await Client.PutAsync(
                $"https://lands-end-3.firebaseio.com/leaderboardDetails/{data.Bib}/{dateTime.Year}/{dateTime.Month}/{dateTime.Day}/.json?auth={formResponse.Key}",
                new StringContent(@$"""{miles}""", Encoding.UTF8, "application/json"));
            putLeaderboardDetailsResponse.EnsureSuccessStatusCode();
        }

        class FormResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }
            [JsonPropertyName("key")]
            public string Key { get; set; }
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
    }
}