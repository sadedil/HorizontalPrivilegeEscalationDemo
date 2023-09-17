using HorizontalPrivilegeEscalation.ExampleApi.Persistence;
using Microsoft.AspNetCore.Identity;

public static class StartupExtensions
{
    public static async Task CreateDbAndMigrate(this WebApplication webApplication)
    {
        using var serviceScope = webApplication.Services.CreateScope();

        var services = serviceScope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }


    public static async Task SeedDemoData(this WebApplication webApplication)
    {
        using var serviceScope = webApplication.Services.CreateScope();

        var services = serviceScope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<HpeUser>>();
        var emailStore = (IUserEmailStore<HpeUser>)services.GetRequiredService<IUserStore<HpeUser>>();

        var usersToBeRegistered = new[]
        {
            new { Id = 1, UserName = "user1", Email = "email1@example.com", Password = "P@ssword1" },
            new { Id = 2, UserName = "user2", Email = "email2@example.com", Password = "P@ssword2" }
        }.ToList();

        foreach (var userToBeRegistered in usersToBeRegistered)
        {
            // Taken from the source code of MapIdentityApi
            var user = new HpeUser() { Id = userToBeRegistered.Id };
            await userManager.SetUserNameAsync(user, userToBeRegistered.Email);
            await emailStore.SetEmailAsync(user, userToBeRegistered.Email, CancellationToken.None);
            await userManager.CreateAsync(user, userToBeRegistered.Password);
        }

        var emails = new List<HpeEmail>()
        {
            new() { EmailId = 1, UserId = 1, Subject = "Subject-1", Body = "This email belongs to user1" },
            new() { EmailId = 2, UserId = 2, Subject = "Subject-2", Body = "This email belongs to user2" },
        };

        await dbContext.Emails.AddRangeAsync(emails);
        await dbContext.SaveChangesAsync();
    }
}
