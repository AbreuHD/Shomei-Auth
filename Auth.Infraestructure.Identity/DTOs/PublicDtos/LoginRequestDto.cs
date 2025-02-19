namespace Auth.Infraestructure.Identity.DTOs.PublicDtos
{
    public class LoginRequestDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
