using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HorizontalPrivilegeEscalation.ExampleApi.Extensions;

public static class ControllerBaseExtensions
{
    public static int GetLoggedInUserId(this ControllerBase controllerBase)
    {
        var userIdAsString = controllerBase
            .User?
            .Claims?
            .FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.Ordinal))?
            .Value;

        if (int.TryParse(userIdAsString, out var userId))
        {
            return userId;
        }

        throw new AuthenticationException("User is not logged in");
    }
}