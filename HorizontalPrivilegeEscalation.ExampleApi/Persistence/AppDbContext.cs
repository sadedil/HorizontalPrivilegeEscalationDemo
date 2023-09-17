using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HorizontalPrivilegeEscalation.ExampleApi.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<HpeUser, HpeRole, int>(options)
{
    public virtual DbSet<HpeEmail> Emails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<HpeEmail>()
            .HasKey(e => e.EmailId);

        builder.Entity<HpeEmail>()
            .HasOne<HpeUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        base.OnModelCreating(builder);
    }
}