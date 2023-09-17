using HorizontalPrivilegeEscalation.ExampleApi.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HorizontalPrivilegeEscalation.ExampleApi.Extensions;

namespace HorizontalPrivilegeEscalation.ExampleApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class EmailController(AppDbContext appDbContext) : ControllerBase
{
    private readonly AppDbContext _appDbContext = appDbContext;

    [HttpGet("{emailId}/vulnerable-example")]
    public async Task<HpeEmail?> GetVulnerable(int emailId)
    {
        return await _appDbContext.Emails.FindAsync(emailId);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Security",
    "HPE001:The controller method is not calling GetLoggedInUserId. It could be dangerous. Please suppress this message with a meaningful message if you know this is a false positive.",
    Justification = "This is how should we suppress analyzer warnings for a method if we sure this is not a security issue")]
    [HttpGet("{emailId}/vulnerable-but-suppressed-example")]
    public async Task<HpeEmail?> GetVulnerableButSuppressed(int emailId)
    {
        return await _appDbContext.Emails.FindAsync(emailId);
    }

    [HttpGet("{emailId}/safe-example")]
    public async Task<HpeEmail?> GetSafe(int emailId)
    {
        var loggedInUserId = this.GetLoggedInUserId();

        return await _appDbContext
            .Emails
            .FirstOrDefaultAsync(e =>
                e.UserId == loggedInUserId
                && e.EmailId == emailId);
    }
}