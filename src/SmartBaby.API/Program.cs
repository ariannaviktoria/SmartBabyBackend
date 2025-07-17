using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using System.Text;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using SmartBaby.Infrastructure.Data;
using SmartBaby.Infrastructure.Repositories;
using SmartBaby.Application.Services;
using SmartBaby.Application.Mappings;
using SmartBaby.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(55363); // Engedélyezi az IPv4 címet minden interfészen
    serverOptions.ListenAnyIP(55362, listenOptions => listenOptions.UseHttps());
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Configure model binding for large files
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "File is required");
});

// Configure request size limits for file uploads
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 200 * 1024 * 1024; // 200MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Baby API",
        Version = "v1",
        Description = "API for Smart Baby application with baby analysis capabilities",
        Contact = new OpenApiContact
        {
            Name = "Smart Baby Team",
            Email = "support@smartbaby.com"
        }
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
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

    // Configure for file uploads
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    // Support for multiple file types
    c.MapType<IFormFileCollection>(() => new OpenApiSchema
    {
        Type = "array",
        Items = new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Custom operation filter to handle file uploads
    c.OperationFilter<FileUploadOperationFilter>();
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT
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

// Add Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBabyService, BabyService>();

// Add Baby Analysis Services
builder.Services.AddScoped<IBabyAnalysisService, BabyAnalysisService>();
builder.Services.AddScoped<IAnalysisHistoryService, AnalysisHistoryService>();
builder.Services.AddScoped<IRealtimeSessionService, RealtimeSessionService>();

// Add generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
            policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Baby API V1");
        c.RoutePrefix = "swagger";
        c.DefaultModelsExpandDepth(-1);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.SupportedSubmitMethods(Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
                                Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, 
                                Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, 
                                Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete);
    });
}

// Csak akkor használjuk a HTTPS átirányítást, ha nem development környezetben vagyunk
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<SmartBaby.API.Hubs.BabyAnalysisHub>("/hubs/babyanalysis");

// Kiírjuk az elérhető URL-eket
var urls = app.Urls.ToList();
Console.WriteLine("Elérhető URL-ek:");
foreach (var url in urls)
{
    Console.WriteLine($"- {url}");
}

app.Run(); 