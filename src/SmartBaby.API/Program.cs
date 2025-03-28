using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using SmartBaby.Infrastructure.Data;
using SmartBaby.Application.Services;
using SmartBaby.Application.Mappings;
using SmartBaby.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();

// Szolgáltatások konfigurálása
void ConfigureServices(WebApplicationBuilder builder)
{
    ConfigureKestrel(builder);
    ConfigureSwagger(builder);
    ConfigureDatabase(builder);
    ConfigureIdentity(builder);
    ConfigureAuthentication(builder);
    ConfigureApplicationServices(builder);
    ConfigureCors(builder);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAutoMapper(typeof(MappingProfile));
}

// Kestrel szerver beállítása
void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(55363);
        serverOptions.ListenAnyIP(55362, listenOptions => listenOptions.UseHttps());
    });
}

// Swagger konfigurálása JWT autentikációval
void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SmartBaby API",
            Version = "v1",
            Description = "SmartBaby alkalmazás API"
        });

        // JWT autentikáció hozzáadása Swagger-hez
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header (Bearer séma). Példa: \"Authorization: Bearer {token}\"",
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
}

// Adatbázis beállítása
void ConfigureDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Identity beállítása
void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
}

// JWT autentikáció beállítása
void ConfigureAuthentication(WebApplicationBuilder builder)
{
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
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });
}

// Alkalmazás szolgáltatások regisztrálása
void ConfigureApplicationServices(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IBabyService, BabyService>();
    builder.Services.AddScoped<ICryingService, CryingService>();
    builder.Services.AddScoped<IFeedingService, FeedingService>();
    builder.Services.AddScoped<ISleepService, SleepService>();
    builder.Services.AddScoped<INoteService, NoteService>();
    builder.Services.AddScoped<IDailyRoutineService, DailyRoutineService>();
}

// CORS beállítása
void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
            policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

// Middleware-ek beállítása
void ConfigureMiddleware(WebApplication app)
{
    // Swagger mindig elérhető, de a UI csak developer módban
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartBaby API v1");
        c.RoutePrefix = string.Empty; // Swagger UI alapértelmezetté tétele a gyökér URL-en
    });

    // HTTPS átirányítás csak nem fejlesztői környezetben
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Elérhető URL-ek kiírása
    var urls = app.Urls.ToList();
    Console.WriteLine("Elérhető URL-ek:");
    foreach (var url in urls)
    {
        Console.WriteLine($"- {url}");
    }
} 