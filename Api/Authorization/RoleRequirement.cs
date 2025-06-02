using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public UserRole RequiredRole { get; }

    public RoleRequirement(UserRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}
