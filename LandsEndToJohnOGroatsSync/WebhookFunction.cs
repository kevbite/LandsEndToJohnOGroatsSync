using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LandsEndToJohnOGroatsSync
{
    public static class WebhookFunction
    {
        [FunctionName("WebhookFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequest req,
            [Queue(QueueNames.ActivityUpdated)] IAsyncCollector<ActivityUpdatedMessage> activitiesUpdated,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<WebhookData>(requestBody);

            if (data.ObjectType == ObjectType.Activity)
            {
                var message = new ActivityUpdatedMessage
                {
                    AthleteId = data.OwnerId,
                    ActivityId = data.ObjectId
                };

                await activitiesUpdated.AddAsync(message);
            }

            return new OkResult();
        }

        public enum AspectType
        {
            Create,
            Update,
            Delete
        }

        public enum ObjectType
        {
            Activity,
            Athlete
        }

        public class WebhookData
        {
            [JsonPropertyName("aspect_type")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public AspectType AspectType { get; set; }
            [JsonPropertyName("event_time")]
            public int EventTime { get; set; }
            [JsonPropertyName("object_id")]
            public long ObjectId { get; set; }
            [JsonPropertyName("object_type")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ObjectType ObjectType { get; set; }
            [JsonPropertyName("owner_id")]
            public int OwnerId { get; set; }
            [JsonPropertyName("subscription_id")]
            public int SubscriptionId { get; set; }
        }


    }
}
