namespace Auth.Infraestructure.Identity.DTOs.Sessions
{
    public class UserSessionResponse
    {
        public required int Id { get; set; }
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
        public required string Country { get; set; }
        public required string City { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
