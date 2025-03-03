namespace Auth.Infraestructure.Identity.DTOs.Users
{
    public class BanUserRequestDto
    {
        public required string UserId { get; set; }
        public required TimeSpan Duration { get; set; }
    }
}
