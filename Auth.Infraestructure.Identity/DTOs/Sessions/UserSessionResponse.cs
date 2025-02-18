using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.Sessions
{
    public class UserSessionResponse
    {
        public required int Id { get; set; }
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
