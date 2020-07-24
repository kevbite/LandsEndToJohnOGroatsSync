using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class ActivityUpdatedFunction
    {
        [FunctionName("ActivityUpdatedFunction")]
        public static void Run([QueueTrigger(QueueNames.ActivityUpdated)]string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"Handle activity updated: {myQueueItem}");
        }
    }

    public class ActivityUpdatedMessage
    {
        public int AthleteId { get; set; }
        public int ActivityId { get; set; }
    }
}
