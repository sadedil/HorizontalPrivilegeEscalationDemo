using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HorizontalPrivilegeEscalation.ExampleApi.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<MyUser, MyRole, int>(options)
{
    public virtual DbSet<Email> Emails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Email>()
            .HasOne<MyUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId);
    
        base.OnModelCreating(builder);
    }
}