using System;

namespace LandsEndToJohnOGroatsSync
{
    public interface IStravaAuthorization
    {
        string AccessToken { get; set; }
        string RefreshToken { get; set; }
        DateTimeOffset AccessTokenExpiresAt { get; set; }
    }
}