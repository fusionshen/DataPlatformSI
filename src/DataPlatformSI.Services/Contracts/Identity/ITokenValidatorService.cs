using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface ITokenValidatorService
    {
        Task ValidateAsync(TokenValidatedContext context);
    }
}
