namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class CreateUserProfileRequestDto
    {
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
