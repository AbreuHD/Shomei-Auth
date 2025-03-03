namespace Auth.Infraestructure.Identity.DTOs.Users
{
    public class GetAllUsersResponseDto
    {
        public required string Id { get; set; }
        public string? UserName { get; set; }
        public required string Email { get; set; }
        public required bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public required bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public required int AccessFailedCount { get; set; }
        public List<string>? Roles { get; set; }
        public required bool isBanned { get; set; }
    }

}
