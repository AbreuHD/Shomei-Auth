using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class CreateUserProfileRequestDto
    {
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
