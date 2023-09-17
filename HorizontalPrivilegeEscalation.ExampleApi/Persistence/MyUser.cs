using Microsoft.AspNetCore.Identity;

namespace HorizontalPrivilegeEscalation.ExampleApi.Persistence;

public class MyUser : IdentityUser<int>
{
    public int Age { get; set; }
}