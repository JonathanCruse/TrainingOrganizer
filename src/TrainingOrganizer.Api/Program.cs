using TrainingOrganizer.Api.Endpoints;
using TrainingOrganizer.Api.Middleware;
using TrainingOrganizer.Application;
using TrainingOrganizer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add auth
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Trainer", policy => policy.RequireRole("Trainer", "Admin"));
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.Run();
