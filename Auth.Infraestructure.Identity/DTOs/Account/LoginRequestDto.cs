namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Represents the data transfer object (DTO) used for user login authentication.
    /// It contains the necessary information (username and password) for user login.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// The username of the user attempting to log in.
        /// </summary>
        /// <value>
        /// A string representing the user's username.
        /// </value>
        public required string UserName { get; set; }

        /// <summary>
        /// The password of the user attempting to log in.
        /// </summary>
        /// <value>
        /// A string representing the user's password.
        /// </value>
        public required string Password { get; set; }
    }
}