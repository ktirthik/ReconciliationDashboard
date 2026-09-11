using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Events;
using ReconciliationDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddPolicy("AppPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var dbProvider = builder.Configuration["DATABASE_PROVIDER"] ?? Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? "sqlserver";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite("Data Source=/data/reconciliation.db");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAiInsightService, AiInsightService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IEventDispatcher, LoggingEventDispatcher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Always migrate and seed — on SQLite this creates the DB file on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbProvider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        db.Database.EnsureCreated();
    await SeedData.SeedAsync(db);
}

app.UseCors("AppPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
