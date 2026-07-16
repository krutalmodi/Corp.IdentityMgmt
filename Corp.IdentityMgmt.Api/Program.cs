using Corp.IdentityMgmt.Application.Interfaces;
using Corp.IdentityMgmt.Infrastructure.Persistence;
using Corp.IdentityMgmt.Infrastructure.Security;
using Corp.IdentityMgmt.Infrastructure.ExternalProviders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. REGISTER SWAGGER SERVICES
builder.Services.AddEndpointsApiExplorer(); // Required for endpoint discovery
builder.Services.AddSwaggerGen();

// Add HttpClient factory for OAuth providers
builder.Services.AddHttpClient();

#region Database Configuration

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connectionString));

#endregion

#region OAuth Configuration

// Register OAuth providers with dependency injection
var oauthConfig = builder.Configuration.GetSection("OAuth");

var googleClientId = oauthConfig["Google:ClientId"];
var googleClientSecret = oauthConfig["Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddScoped<GoogleOAuthProvider>(sp =>
        new GoogleOAuthProvider(sp.GetRequiredService<HttpClient>(), googleClientId, googleClientSecret));
}

var githubClientId = oauthConfig["GitHub:ClientId"];
var githubClientSecret = oauthConfig["GitHub:ClientSecret"];
if (!string.IsNullOrEmpty(githubClientId) && !string.IsNullOrEmpty(githubClientSecret))
{
    builder.Services.AddScoped<GitHubOAuthProvider>(sp =>
        new GitHubOAuthProvider(sp.GetRequiredService<HttpClient>(), githubClientId, githubClientSecret));
}

var microsoftClientId = oauthConfig["Microsoft:ClientId"];
var microsoftClientSecret = oauthConfig["Microsoft:ClientSecret"];
if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
{
    builder.Services.AddScoped<MicrosoftOAuthProvider>(sp =>
        new MicrosoftOAuthProvider(sp.GetRequiredService<HttpClient>(), microsoftClientId, microsoftClientSecret));
}

#endregion

#region Dependency Injection

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PassowordHasher>();

#endregion

var app = builder.Build();

// 2. ENABLE SWAGGER MIDDLEWARE (Best practice: Restrict to Development environment)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Serves the generated OpenAPI spec as a JSON endpoint
    app.UseSwaggerUI(); // Serves the interactive web UI
}

#region Database Initialization

// Ensure database is created and migrations are applied
// Wrapped in try-catch to allow app to start even if database migration fails
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
