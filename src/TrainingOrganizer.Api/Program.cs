using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TrainingOrganizer.Adapter.EasyVerein;
using TrainingOrganizer.Adapter.Keycloak;
using TrainingOrganizer.Api.Endpoints;
using TrainingOrganizer.Api.Middleware;
using TrainingOrganizer.Membership;
using TrainingOrganizer.Membership.Infrastructure.Seeding;
using TrainingOrganizer.SharedKernel;
using TrainingOrganizer.Training;
using TrainingOrganizer.Facility;

var builder = WebApplication.CreateBuilder(args);

// Add shared kernel (MongoDB, UnitOfWork, pipeline behaviors)
builder.Services.AddSharedKernel(builder.Configuration);

// Add bounded contexts
builder.Services.AddMembership();
builder.Services.AddTraining();
builder.Services.AddFacility();

// Add adapters
builder.Services.AddEasyVereinAdapter(builder.Configuration);
builder.Services.AddKeycloakAdapter(builder.Configuration);

// MediatR — scan all slice assemblies
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(TrainingOrganizer.Membership.DependencyInjection).Assembly,
    typeof(TrainingOrganizer.Training.DependencyInjection).Assembly,
    typeof(TrainingOrganizer.Facility.DependencyInjection).Assembly));

// FluentValidation — scan all slice assemblies
builder.Services.AddValidatorsFromAssemblies([
    typeof(TrainingOrganizer.Membership.DependencyInjection).Assembly,
    typeof(TrainingOrganizer.Training.DependencyInjection).Assembly,
    typeof(TrainingOrganizer.Facility.DependencyInjection).Assembly]);

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
