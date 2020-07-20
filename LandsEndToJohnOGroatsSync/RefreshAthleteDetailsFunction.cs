using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;

namespace LandsEndToJohnOGroatsSync
{
    public static class RefreshAthleteDetailsFunction
    {
        [FunctionName("RefreshAthleteDetailsFunction")]
        public static async Task Run([QueueTrigger(QueueNames.RefreshAthletesDetails)]RefreshAthletesDetailsRequest request,
            [Table(TableNames.AthletesTable, "{AthleteId}", "")] AthleteTableEntity athlete,
            [Table(TableNames.AthletesTable)] CloudTable athletesTable,
            ILogger log)
        {
            var client = new LittleStravaClient();

            var result = await client.GetAuthenticatedAthlete(athlete);

            athlete.FirstName = result.Firstname;
            athlete.LastName = result.Lastname;

            await athletesTable.ExecuteAsync(TableOperation.InsertOrReplace(athlete));
        }
    }
}
