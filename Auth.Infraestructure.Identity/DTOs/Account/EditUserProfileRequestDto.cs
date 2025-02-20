namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class EditUserProfileRequestDto
    {
        public required int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string Name { get; set; }
    }
}
