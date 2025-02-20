using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class SendValidationEmailAgainRequestDto
    {
        public required string Email { get; set; }
    }
}
