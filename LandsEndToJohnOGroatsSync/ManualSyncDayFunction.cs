using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LandsEndToJohnOGroatsSync
{
    public static class ManualSyncDayFunction
    {
        [FunctionName("ManualSyncDayFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "athletes/{athleteId}/sync/{date}")] HttpRequest req,
            [Queue(QueueNames.SyncDay)] IAsyncCollector<SyncDayRequest> syncDayRequests,
            int athleteId,
            DateTime date,
            ILogger log)
        {

            await syncDayRequests.AddAsync(new SyncDayRequest
            {
                DateTime = date.Date,
                AthleteId = athleteId
            });

            return new OkObjectResult("Ok");
        }
    }
}
