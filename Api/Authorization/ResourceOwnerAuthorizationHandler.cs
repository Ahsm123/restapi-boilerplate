using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResourceOwnerAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement)
    {
        var currentUserId = context.User.FindFirst(Claims.UserId)?.Value;
        var userRole = context.User.FindFirst(Claims.Role)?.Value;

        //Get the resource ID from the route
        var httpContext = _httpContextAccessor.HttpContext;
        var resourceId = httpContext?.Request.RouteValues["id"]?.ToString();

        //Admin can access any resource
        if (Enum.TryParse<UserRole>(userRole, out var role) && role == UserRole.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        //User can only access their own resource
        if (currentUserId == resourceId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
