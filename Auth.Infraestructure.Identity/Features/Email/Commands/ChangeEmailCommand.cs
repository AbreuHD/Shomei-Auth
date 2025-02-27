using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Extra;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangeEmailRequestDto Dto { get; set; }
        public required string UserId { get; set; }
        public bool UseOtp { get; set; } = false;
    }

    internal class ChangeEmailCommandHandler(UserManager<ApplicationUser> userManager,
        IdentityContext identityContext) : IRequestHandler<ChangeEmailCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    response.Message = "User not found.";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }

                var emailExists = await _userManager.FindByEmailAsync(request.Dto.NewEmail);
                if (emailExists != null)
                {
                    response.Message = $"Email {request.Dto.NewEmail} is already taken";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }

                if (request.Dto.Otp is not null && request.UseOtp)
                {
                    var otpCheckResponse = await ValidateOtpWithEmail(response, request);
                    if (!otpCheckResponse.Success)
                    {
                        return otpCheckResponse;
                    }
                }
                else
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Dto.Password!);
                    if (!passwordCheck)
                    {
                        response.Message = "Password is incorrect";
                        response.Statuscode = StatusCodes.Status406NotAcceptable;
                        return response;
                    }
                }

                var result = await _userManager.SetEmailAsync(user, request.Dto.NewEmail);
                if (result.Succeeded)
                {
                    response.Payload = true;
                    response.Success = true;
                    response.Statuscode = StatusCodes.Status200OK;
                    response.Message = "Email changed successfully.";
                }
                else
                {
                    response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        private async Task<GenericApiResponse<bool>> ValidateOtpWithEmail(GenericApiResponse<bool> response, ChangeEmailCommand request)
        {
            var otpRecord = await _identityContext.Set<MailOtp>()
                .Where(x => x.UserId == request.UserId 
                    && x.Otp == ExtraMethods.HashToken(request.Dto.Otp!) 
                    && x.Purpose == OtpPurpose.ChangeEmail.ToString())
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                response.Message = "Invalid OTP";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            if (otpRecord.Expiration < DateTime.UtcNow)
            {
                response.Message = "OTP has expired";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            if (otpRecord.Used == true)
            {
                response.Message = "OTP has already been used";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            otpRecord.Used = true;
            _identityContext.Entry(otpRecord).Property(o => o.Used).IsModified = true;
            await _identityContext.SaveChangesAsync();

            response.Success = true;
            return response;
        }
    }
}
