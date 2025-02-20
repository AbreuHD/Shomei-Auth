namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class ConfirmEmailRequestDto
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
    }
}
