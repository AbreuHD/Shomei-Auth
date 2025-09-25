namespace Shomei.Infraestructure.Identity.Settings
{
    public class JwtSettings
    {
        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required int DurationInMinutes { get; set; }
        public required bool UseDifferentProfiles { get; set; }
        public int? MaxFailedAccessAttempts { get; set; }
        public int? DefaultLockoutTimeSpan { get; set; }
    }
}
