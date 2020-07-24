using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class DailyTimer
    {
        [Disable]
        [FunctionName("DailyTimer")]
        public static async Task Run([TimerTrigger("0 0 3 * * *")]TimerInfo myTimer,
            [Table(TableNames.AthletesTable)] CloudTable athletesTable,
            [Queue(QueueNames.SyncDay)] IAsyncCollector<SyncDayRequest> syncDayRequests,
            ILogger log)
        {
            var athletes = athletesTable.ExecuteQuery(new TableQuery<AthleteTableEntity>()).ToList();

            foreach (var athlete in athletes)
            {
                await syncDayRequests.AddAsync(new SyncDayRequest
                {
                    DateTime = DateTimeOffset.UtcNow.Date.AddDays(-1),
                    AthleteId = athlete.GetAthleteId()
                });
            }

        }
    }
}
