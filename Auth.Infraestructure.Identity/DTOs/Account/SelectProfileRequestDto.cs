using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class SelectProfileRequestDto
    {
        public required int Profile { get; set; }
    }
}
