using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace LandsEndToJohnOGroatsSync
{
    public static class CreateNewUserFunction
    {
        [FunctionName("CreateNewUserFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "create_new_user")] HttpRequest req,
            [Table("Athletes")] CloudTable athletesTable,
            ILogger log)
        {
            string pin = req.Query["pin"];
            string bib = req.Query["bib"];

            if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(bib))
            {
                return new BadRequestResult();
            }

            var queryString = new Dictionary<string, string>();
            queryString.Add("client_id", Environment.GetEnvironmentVariable("strava_client_id"));
            queryString.Add("response_type", "code");
            queryString.Add("redirect_uri", $"http://localhost:7071/api/exchange_token?pinbib={pin},{bib}");
            queryString.Add("approval_prompt", "force");
            queryString.Add("scope", "read,activity:read_all");

            var url = QueryHelpers.AddQueryString("http://www.strava.com/oauth/authorize", queryString);

            return new RedirectResult(url);
        }

    }
}