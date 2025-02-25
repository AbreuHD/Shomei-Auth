namespace Auth.Infraestructure.Identity.DTOs.Profiles
{
    /// <summary>
    /// Response model for getting user profiles.
    /// </summary>
    public class GetProfilesResponse
    {
        /// <summary>
        /// The unique identifier for the profile.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name associated with the profile.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The URL of the profile's avatar image, if available.
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}
