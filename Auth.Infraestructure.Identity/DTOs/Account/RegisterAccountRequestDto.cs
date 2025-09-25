namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Data Transfer Object (DTO) for user registration. 
    /// This class holds the necessary information required to register a new user account.
    /// </summary>
    public class RegisterAccountRequestDto
    {
        /// <summary>
        /// The first name of the user.
        /// </summary>
        /// <value>
        /// A string representing the user's first name.
        /// </value>
        public required string Name { get; set; }

        /// <summary>
        /// The last name of the user.
        /// </summary>
        /// <value>
        /// A string representing the user's last name.
        /// </value>
        public required string LastName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        /// <value>
        /// A string representing the user's email address.
        /// </value>
        public required string Email { get; set; }

        /// <summary>
        /// The username of the user, used for login.
        /// </summary>
        /// <value>
        /// A string representing the user's username.
        /// </value>
        public string? UserName { get; set; }

        /// <summary>
        /// The password chosen by the user to secure their account.
        /// </summary>
        /// <value>
        /// A string representing the user's password.
        /// </value>
        public required string Password { get; set; }

        /// <summary>
        /// A confirmation password field, used to ensure the user typed the correct password.
        /// </summary>
        /// <value>
        /// A string representing the confirmation of the user's password.
        /// </value>
        public required string ConfirmPassword { get; set; }

        /// <summary>
        /// The phone number of the user.
        /// </summary>
        /// <value>
        /// A string representing the user's phone number.
        /// </value>
        public string? Phone { get; set; }
    }
}
