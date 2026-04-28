using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Data.Repositories;
using ITAMS.Domain.Interfaces;
using ITAMS.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/itams-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Kestrel to accept large files
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ITAMSDbContext>(options =>
{
    var sharedConnStr = builder.Configuration.GetConnectionString("SharedSqlServer");
    var localConnStr = builder.Configuration.GetConnectionString("DefaultConnection");

    string connectionString = localConnStr ?? "Data Source=ITAMS.db"; // default fallback

    if (!string.IsNullOrEmpty(sharedConnStr))
    {
        // Try to reach the shared server — if reachable, use it; otherwise fall back to local
        try
        {
            var builder2 = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sharedConnStr);
            using var testConn = new Microsoft.Data.SqlClient.SqlConnection(sharedConnStr);
            testConn.Open();
            testConn.Close();
            connectionString = sharedConnStr;
        }
        catch
        {
            // Shared server unreachable — fall back to local
            connectionString = localConnStr ?? "Data Source=ITAMS.db";
        }
    }

    if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Server="))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString ?? "Data Source=ITAMS.db");
    }
});

// Add JWT Authentication
var jwtSecretKey = "your-super-secret-key-that-should-be-in-configuration-and-at-least-32-characters-long";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Add services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IBulkUploadService, BulkUploadService>();
builder.Services.AddScoped<IAssetIdGeneratorService, AssetIdGeneratorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add RBAC services
builder.Services.AddScoped<ITAMS.Services.RBAC.IPermissionResolver, ITAMS.Services.RBAC.PermissionResolver>();
builder.Services.AddScoped<ITAMS.Services.RBAC.IRbacAuditService, ITAMS.Services.RBAC.RbacAuditService>();
builder.Services.AddScoped<ITAMS.Services.RBAC.IRoleManagementService, ITAMS.Services.RBAC.RoleManagementService>();

// Add background services
builder.Services.AddHostedService<ITAMS.Services.SessionCleanupService>();
builder.Services.AddHostedService<ITAMS.Services.AutomationEngineService>();
// Temporarily disabled due to missing column in ApprovalRequests table
// builder.Services.AddHostedService<ITAMS.Services.ApprovalEscalationService>();

// Add memory cache for permission caching
builder.Services.AddMemoryCache();

// Add HTTP context accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("*");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// CORS must be before other middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable static files

// Add authentication and authorization
app.UseAuthentication();

// Add maintenance mode middleware (must be after authentication)
app.UseMiddleware<ITAMS.Middleware.MaintenanceModeMiddleware>();

// Add activity tracking middleware (must be after authentication)
app.UseMiddleware<ITAMS.Middleware.ActivityTrackingMiddleware>();

// Add project access control middleware (must be after authentication)
app.UseMiddleware<ITAMS.Middleware.ProjectAccessControlMiddleware>();

app.UseAuthorization();

// Add default route that redirects to Angular frontend
app.MapGet("/", () => Results.Redirect("http://localhost:4200"));

app.MapControllers();

// Ensure database is created (but don't migrate automatically)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log the error but continue - database might already exist
        Log.Warning("Database creation/connection issue: {Error}", ex.Message);
    }
}

app.Run();
