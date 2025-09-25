namespace Shomei.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// DTO for selecting a user profile.
    /// </summary>
    public class SelectProfileRequestDto
    {
        /// <summary>
        /// The ID of the profile to be selected.
        /// </summary>
        public required int Profile { get; set; }
    }
}
