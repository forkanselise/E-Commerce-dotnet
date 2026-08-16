using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NexusBakery.Agents.Agents;
using NexusBakery.Agents.Core;
using NexusBakery.Agents.Tools;
using NexusBakery.Api.Hubs;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Interfaces;
using NexusBakery.Infrastructure.Persistence;
using NexusBakery.Infrastructure.Persistence.Repositories;
using NexusBakery.Infrastructure.Persistence.Seeders;

// 1. Load .env file from root directory
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add Configuration from Environment Variables
builder.Configuration.AddEnvironmentVariables();

// 2. Add Services to DI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// 3. Configure CORS for React/Vite Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 4. Register MongoDB Persistence Layer
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ITutorialRepository, TutorialRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();
builder.Services.AddScoped<DatabaseSeeder>();

// 5. Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITutorialService, TutorialService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 6. Register Multi-Agent AI Engine & Tools
builder.Services.AddScoped<SearchProductsTool>();
builder.Services.AddScoped<CheckStockTool>();
builder.Services.AddScoped<AddToCartTool>();
builder.Services.AddScoped<SearchTutorialsTool>();
builder.Services.AddScoped<AdjustInventoryTool>();

builder.Services.AddScoped<StorefrontInventoryAgent>();
builder.Services.AddScoped<BakingMasterclassAgent>();
builder.Services.AddScoped<WarehouseOpsAgent>();
builder.Services.AddScoped<RouterConciergeAgent>();
builder.Services.AddScoped<AgentOrchestrator>();

// 7. Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "NexusBakerySuperSecretKey2026_ArtisanTechBakeryPro!";

var issuer = builder.Configuration["JWT_ISSUER"] ?? "NexusBakeryApi";
var audience = builder.Configuration["JWT_AUDIENCE"] ?? "NexusBakeryClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Support JWT Token over SignalR WebSocket queries
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 8. Auto-Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    try
    {
        await seeder.SeedAsync();
        Console.WriteLine("✅ Database indexes created & sample catalog data seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database seeding skipped or failed: {ex.Message}");
    }
}

// 9. HTTP Middleware Pipeline
app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 10. Map SignalR Real-Time WebSocket Hubs
app.MapHub<VisitorHub>("/hubs/visitor");
app.MapHub<AgentHub>("/hubs/agent");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    platform = "Nexus Bakery & Tech API",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.Run();
