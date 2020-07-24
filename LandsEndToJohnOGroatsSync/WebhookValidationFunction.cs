using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LandsEndToJohnOGroatsSync
{
    public static class WebhookValidationFunction
    {
        [FunctionName("WebhookValidationFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhook")] HttpRequest req,
            ILogger log)
        {
            var actualVerifyToken = Environment.GetEnvironmentVariable("strava_webhook_verify_token");
            var mode = req.Query["hub.mode"];
            var verifyToken = req.Query["hub.verify_token"];
            var challenge = req.Query["hub.challenge"];

            if (mode == "subscribe" && verifyToken == actualVerifyToken)
            {
                log.LogInformation("WEBHOOK_VERIFIED");

                return new JsonResult(
                    new ChallengeResponse { HubChallenge = challenge }
                    );
            }
            else
            {
                return new ForbidResult();
            }
        }
        public class ChallengeResponse
        {
            [JsonProperty("hub.challenge")]
            public string HubChallenge { get; set; }
        }
    }

}
