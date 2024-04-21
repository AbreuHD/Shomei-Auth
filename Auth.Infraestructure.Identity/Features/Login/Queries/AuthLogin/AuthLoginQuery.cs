using Auth.Core.Application.DTOs.Account;
using Auth.Core.Application.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Core.Application.Features.Login.Queries.AuthLogin
{
    public class AuthLoginQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AuthLoginQueryHandler : IRequestHandler<AuthLoginQuery, GenericApiResponse<AuthenticationResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Task<GenericApiResponse<AuthenticationResponse>> Handle(AuthLoginQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
