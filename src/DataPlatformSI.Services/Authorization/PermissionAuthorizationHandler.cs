using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Authorization
{
    // This class contains logic for determining whether MinimumAgeRequirements in authorizaiton
    // policies are satisfied or not
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        // Check whether a given MinimumAgeRequirement is satisfied or not for a particular context
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Log as a warning so that it's very clear in sample output which authorization policies 
            // (and requirements/handlers) are in use
            _logger.LogWarning("Evaluating authorization requirement for having permission: {permission}", requirement.Permission);

            // Check the user's permission
            foreach (var permissionClaim in context.User.FindAll(c => c.Type == ClaimTypes.Authentication))
            {
                if (permissionClaim.Value == requirement.Permission)
                {
                    _logger.LogInformation("permission authorization requirement {permission} satisfied", requirement.Permission);
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
