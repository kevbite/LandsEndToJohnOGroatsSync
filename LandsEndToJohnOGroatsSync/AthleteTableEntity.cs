using System;
using System.IO;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace LandsEndToJohnOGroatsSync
{
    public class AthleteTableEntity : TableEntity, IStravaAuthorization
    {
        public AthleteTableEntity()
        {
            RowKey = string.Empty;
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
    }
}
