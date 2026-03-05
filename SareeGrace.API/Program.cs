using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SareeGrace.Application.Interfaces;
using SareeGrace.Infrastructure.Data;
using SareeGrace.Infrastructure.Repositories;
using SareeGrace.Infrastructure.Services;
using SareeGrace.Domain.Interfaces;
using SareeGrace.Domain.Entities;

// ──────────────────── Serilog Bootstrap ────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/sareegrace-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

try
{
    Log.Information("Starting SareeGrace API...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var config = builder.Configuration;

    // ──────────────────── Database ────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("SareeGrace.Infrastructure");
            }));

    // ──────────────────── Repository & UnitOfWork ────────────────────
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // ──────────────────── Application Services ────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IWishlistService, WishlistService>();
    builder.Services.AddScoped<IAddressService, AddressService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IImageService, LocalImageService>();

    // ──────────────────── JWT Authentication ────────────────────
    var jwtKey = config["Jwt:Key"] ?? "SareeGrace_SuperSecret_Key_2024_Must_Be_At_Least_32_Chars!";
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
            ValidIssuer = config["Jwt:Issuer"] ?? "SareeGrace",
            ValidAudience = config["Jwt:Audience"] ?? "SareeGraceApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // ──────────────────── CORS (allow React dev server) ────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "http://localhost:5174",
                    "https://sareegrace.netlify.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ──────────────────── Controllers & Swagger ────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            // Accept camelCase from the React client; map to PascalCase C# properties
            o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SareeGrace API",
            Version = "v1",
            Description = "Premium Indian Saree E-Commerce API"
        });

        // JWT Bearer token in Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ──────────────────── Auto-create database & apply migrations ────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrated successfully.");

        // ── Upsert Admin User with correct password hash ──
        const string adminEmail = "admin@sareegrace.com";
        const string adminPassword = "Admin@123";
        var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        var admin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
        if (admin == null)
        {
            db.Users.Add(new User
            {
                Id = adminId,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FirstName = "Super",
                LastName = "Admin",
                Phone = "9999999999",
                Role = "Admin",
                IsActive = true,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
            Log.Information("Admin user created.");
        }
        else if (!BCrypt.Net.BCrypt.Verify(adminPassword, admin.PasswordHash))
        {
            // Fix incorrect hash from migration seed
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            db.SaveChanges();
            Log.Information("Admin password hash corrected.");
        }
    }

    // ──────────────────── Create wwwroot/images directories ────────────────────
    var imagesPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "products");
    Directory.CreateDirectory(imagesPath);
    var categoryImagesPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "categories");
    Directory.CreateDirectory(categoryImagesPath);

    // ──────────────────── Middleware Pipeline ────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SareeGrace API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowReactApp");
    app.UseStaticFiles(); // Serves wwwroot/images for uploaded product images

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("SareeGrace API started on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SareeGrace API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
