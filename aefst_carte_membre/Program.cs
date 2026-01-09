using System.Security.Claims;
using System.Text;
using aefst_carte_membre.DbContexts;
using aefst_carte_membre.Identity;
using aefst_carte_membre.Models;
using aefst_carte_membre.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔥 OBLIGATOIRE POUR RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------------
// DATABASE
// ---------------------
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

//    if (string.IsNullOrWhiteSpace(databaseUrl))
//    {
//        throw new Exception("DATABASE_URL is missing. App cannot start.");
//    }

//    var uri = new Uri(databaseUrl);
//    var userInfo = uri.UserInfo.Split(':');

//    var connectionString =
//        $"Host={uri.Host};" +
//        $"Port={uri.Port};" +
//        $"Database={uri.AbsolutePath.TrimStart('/')};" +
//        $"Username={userInfo[0]};" +
//        $"Password={userInfo[1]};" +
//        $"SSL Mode=Require;Trust Server Certificate=true";

//    options.UseNpgsql(connectionString);
//});


var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));



// ---------------------
// CONTROLLERS & OPENAPI
// ---------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();



// ---------------------
// BACKGROUND JOB
// ---------------------
builder.Services.AddHostedService<ExpirationJob>();

// ---------------------
// IDENTITY
// ---------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),

        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    };
});




var app = builder.Build();

// ---------------------
// SEED ROLES + ADMIN
// ---------------------
async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "ADMIN", "BUREAU", "MEMBRE" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@aefst.org";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    //var isActive = true;

    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@2026!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "ADMIN");
    }
}


// Exécution du seed
// 1️⃣ Appliquer migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 2️⃣ Seed rôles + admin
using (var scope = app.Services.CreateScope())
{
    await SeedRolesAndAdminAsync(scope.ServiceProvider);
}


// ---------------------
// PIPELINE HTTP
// ---------------------


//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "AEFST – Carte Membre API";
        options.Theme = ScalarTheme.Moon;
    });
//}

app.Run();
