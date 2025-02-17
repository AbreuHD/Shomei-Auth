﻿using System.Text.Json.Serialization;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class AuthenticationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ImageProfile { get; set; }

        public List<string> Roles { get; set; }
        public bool IsVerified { get; set; }

        public string JWToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }

    }
}
