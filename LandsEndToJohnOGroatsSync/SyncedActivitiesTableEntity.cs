using Microsoft.Azure.Cosmos.Table;

namespace LandsEndToJohnOGroatsSync
{
    public class SyncedActivitiesTableEntity : TableEntity
    {
        public string ActivityType { get; set; }
        public double Meters { get; set; }
        public double Miles { get; set; }
    }
}