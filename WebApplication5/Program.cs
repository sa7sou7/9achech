using Microsoft.AspNetCore.Authentication.JwtBearer; // Ajouté ici
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApplication5.Data;
using WebApplication5.Repository;
using WebApplication5.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient with IHttpClientFactory
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApi:BaseUrl"] ?? "https://cmc.crm-edi.info/apisif/public/api/");
    client.Timeout = TimeSpan.FromSeconds(
        int.TryParse(builder.Configuration["ExternalApi:TimeoutInSeconds"], out var timeout) ? timeout : 30);
});

// Register repositories and services
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IArticleSyncService, ArticleSyncService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<ICommercialTaskRepository, CommercialTaskRepository>();
builder.Services.AddHostedService<VisitNotificationService>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new ArgumentNullException("JWT Key is missing")))
    };
});

// Add authorization policies for roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("CommercialOnly", policy => policy.RequireRole("Commercial"));
    options.AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "Manager"));
});
// Add this to your services configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Add JWT Authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles, admin user, and sync commercials
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var syncService = services.GetRequiredService<ISyncService>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed admin user
        await SeedAdminUserAsync(userManager);

        // Sync commercials (assuming this is implemented in SyncService)
        var syncResult = await syncService.SynchronizeCommercialsAsync();
        logger.LogInformation(syncResult);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during startup seeding/syncing.");
    }
}

app.Run();

// Method to seed roles
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "Manager", "Commercial" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// Method to seed admin user
async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
{
    var adminUser = await userManager.FindByEmailAsync("admin@company.com");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@company.com",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}