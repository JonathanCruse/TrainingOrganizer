using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TrainingOrganizer.Api.Endpoints;
using TrainingOrganizer.Api.Middleware;
using TrainingOrganizer.Application;
using TrainingOrganizer.Infrastructure;
using TrainingOrganizer.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS for Blazor WASM frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5200"];
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Schemes:Bearer:Authority"];
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            RoleClaimType = "roles",
            NameClaimType = "preferred_username"
        };
    });
builder.Services.AddScoped<IClaimsTransformation, MemberIdClaimsTransformation>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Trainer", policy => policy.RequireRole("Trainer", "Admin"));
});

// Dev-only: seed test members from Keycloak users
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevDataSeeder>();
}

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapMemberEndpoints();
app.MapTrainingEndpoints();
app.MapRecurringTrainingEndpoints();
app.MapSessionEndpoints();
app.MapLocationEndpoints();
app.MapBookingEndpoints();
app.MapScheduleEndpoints();
app.MapImportEndpoints();

app.Run();
