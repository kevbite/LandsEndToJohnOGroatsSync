using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class SyncDayActivityFunction
    {
        [FunctionName("SyncDayActivityFunction")]
        public static async Task Run(
            [QueueTrigger("sync-day")]SyncDayRequest request,
            [Table(TableNames.AthletesTable, "{AthleteId}", "")] AthleteTableEntity athlete,
            [Table(TableNames.AthletesTable)] CloudTable athletesTable,
            [Table(TableNames.SyncedActivitiesTable)] CloudTable syncedActivitiesTable,
            ILogger log)
        {

            var client = new LittleStravaClient();

            var date = request.DateTime.Date;
            var before = new DateTimeOffset(date.AddDays(1)).ToUnixTimeSeconds();
            var after = new DateTimeOffset(date).ToUnixTimeSeconds();

            var activities = await client.GetLoggedInAthleteActivities(athlete, before, after);

            var types = athlete.TypesToSync.Split(",").Select(Enum.Parse<LittleStravaClient.ActivityType>).ToArray();
            var activitiesToSync = activities.Where(x => types.Contains(x.Type))
                .Select(x => new {x.Id, x.Type, Meters = x.Distance, Miles = ConvertToMiles(x.Distance)})
                .ToArray();

            var totalDaysMiles = activitiesToSync.Sum(x => x.Miles);

            var landsEnd3FireBaseAppClient = new LandsEnd3FireBaseAppClient();
            await landsEnd3FireBaseAppClient.SubmitData(athlete, date, totalDaysMiles);

            var tableBatchOperation = new TableBatchOperation();
            foreach (var sycnedActivity in activitiesToSync)
            {
                tableBatchOperation.InsertOrReplace(new SyncedActivitiesTableEntity
                {
                    PartitionKey = request.AthleteId.ToString(),
                    RowKey = sycnedActivity.Id.ToString(),
                    ActivityType = sycnedActivity.Type.ToString(),
                    Meters = sycnedActivity.Meters,
                    Miles = sycnedActivity.Miles,
                });
            }

            if (tableBatchOperation.Any())
            {
                syncedActivitiesTable.ExecuteBatch(tableBatchOperation);
            }

            await athletesTable.ExecuteAsync(TableOperation.InsertOrReplace(athlete));

        }

        private static double ConvertToMiles(float meters)
        {
            return meters * 0.000621371d;
        }
    }
}
