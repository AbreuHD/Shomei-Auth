namespace Shomei.Infraestructure.Identity.DTOs.Sessions
{
    /// <summary>
    /// Response model for user session information.
    /// </summary>
    public class UserSessionResponse
    {
        /// <summary>
        /// The unique identifier for the session.
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// The unique identifier of the user associated with the session.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// The token associated with the session.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// The IP address from which the session was created.
        /// </summary>
        public required string IpAddress { get; set; }

        /// <summary>
        /// The user agent string representing the client's browser or device.
        /// </summary>
        public required string UserAgent { get; set; }

        /// <summary>
        /// The country where the session was created.
        /// </summary>
        public required string Country { get; set; }

        /// <summary>
        /// The city where the session was created.
        /// </summary>
        public required string City { get; set; }

        /// <summary>
        /// The date and time when the session was created.
        /// </summary>
        public required DateTime CreatedAt { get; set; }
    }
}
