using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Extra;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Otp
{
    internal static class ValidateOtpEmail
    {
        internal static async Task<GenericApiResponse<bool>> ValidateOtpWithEmail(GenericApiResponse<bool> response, string otp, string userId, string purpose, IdentityContext _identityContext)
        {
            var otpRecord = await _identityContext.Set<MailOtp>()
                .Where(x => x.UserId == userId
                    && x.Otp == ExtraMethods.HashToken(otp)
                    && x.Purpose == purpose)
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