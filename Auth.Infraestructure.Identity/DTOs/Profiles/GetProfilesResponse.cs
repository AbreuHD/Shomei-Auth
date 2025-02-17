namespace Auth.Infraestructure.Identity.DTOs.Profiles
{
    public class GetProfilesResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
