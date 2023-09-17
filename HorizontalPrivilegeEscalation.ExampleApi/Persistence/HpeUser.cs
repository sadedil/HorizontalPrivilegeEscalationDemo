using Microsoft.AspNetCore.Identity;

namespace HorizontalPrivilegeEscalation.ExampleApi.Persistence;

public class HpeUser : IdentityUser<int>
{
    public int Age { get; set; }
}