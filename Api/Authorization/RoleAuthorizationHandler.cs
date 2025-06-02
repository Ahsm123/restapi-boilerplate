using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var userRole = context.User.FindFirst(Claims.Role)?.Value;

        if (Enum.TryParse<UserRole>(userRole, out var role) &&
            role >= requirement.RequiredRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
