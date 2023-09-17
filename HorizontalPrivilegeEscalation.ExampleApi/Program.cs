using HorizontalPrivilegeEscalation.ExampleApi.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string databaseFileName = "hpe.db";

if (File.Exists(databaseFileName))
{
    // Demo purposes we need fresh DB for every run
    File.Delete(databaseFileName);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

// Optional: You can use InMemoryDb as well
// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//     options.UseInMemoryDatabase("HorizontalPrivilegeEscalationDatabase");
// });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite($"Data Source={databaseFileName};");
});

builder.Services
    .AddIdentityCore<HpeUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.CreateDbAndMigrate();
    await app.SeedDemoData();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.EnableTryItOutByDefault());
}

app.MapIdentityApi<HpeUser>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();