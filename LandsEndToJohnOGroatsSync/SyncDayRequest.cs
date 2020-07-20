using System;

namespace LandsEndToJohnOGroatsSync
{
    public class SyncDayRequest
    {
        public int AthleteId { get; set; }
        public DateTimeOffset DateTime { get; set; }
    }
}