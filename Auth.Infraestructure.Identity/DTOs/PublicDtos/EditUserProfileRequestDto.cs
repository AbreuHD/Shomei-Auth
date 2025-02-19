using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.PublicDtos
{
    public class EditUserProfileRequestDto
    {
        public required int Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string Name { get; set; }
    }
}
