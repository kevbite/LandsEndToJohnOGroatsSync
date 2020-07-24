using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class ActivityUpdatedFunction
    {
        [FunctionName("ActivityUpdatedFunction")]
        public static async Task Run(
            [QueueTrigger(QueueNames.ActivityUpdated)]ActivityUpdatedMessage message,
            [Table(TableNames.AthletesTable, "{AthleteId}", "")] AthleteTableEntity athlete,
            [Table(TableNames.AthletesTable)] CloudTable athletesTable,
            [Queue(QueueNames.SyncDay)] IAsyncCollector<SyncDayRequest> syncDayRequests,
            ILogger log)
        {
            var littleStravaClient = new LittleStravaClient();
            var activity = await littleStravaClient.GetActivityById(athlete, message.ActivityId);

            await syncDayRequests.AddAsync(new SyncDayRequest
            {
                DateTime = activity.StartDate.Date,
                AthleteId = athlete.GetAthleteId()
            });

            await athletesTable.ExecuteAsync(TableOperation.InsertOrReplace(athlete));

        }
    }

    public class ActivityUpdatedMessage
    {
        public int AthleteId { get; set; }
        public long ActivityId { get; set; }
    }
}
