using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LandsEndToJohnOGroatsSync
{
    public static class DailyTimer
    {
        [FunctionName("DailyTimer")]
        public static void Run([TimerTrigger("0 0 3 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
