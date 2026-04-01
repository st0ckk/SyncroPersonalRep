using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Auth;
using SyncroBE.Infrastructure.Data;
using SyncroBE.Infrastructure.Repositories;
using SyncroBE.Infrastructure.Services;
using SyncroBE.Infrastructure.Services.Hacienda;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// =========================
// DB CONTEXT
// =========================
builder.Services.AddDbContext<SyncroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// =========================
// REPOSITORIOS
// =========================
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IDistributorRepository, DistributorRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IAuditService, NoOpAuditService>();
builder.Services.AddScoped<IVacationService, VacationService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteTemplateRepository, RouteTemplateRepository>();
builder.Services.AddScoped<IClientAccountRepository, ClientAccountRepository>();

// =========================
// PDF SERVICE
// =========================
builder.Services.AddScoped<IPdfService, PdfService>();

// =========================
// HACIENDA (Electronic Invoice)
// =========================
builder.Services.Configure<HaciendaSettings>(
    builder.Configuration.GetSection(HaciendaSettings.SectionName));

builder.Services.AddHttpClient<IHaciendaTokenService, HaciendaTokenService>();
builder.Services.AddHttpClient<IHaciendaApiService, HaciendaApiService>();
builder.Services.AddHttpClient<IHaciendaLookupService, HaciendaLookupService>();
builder.Services.AddScoped<IClaveGeneratorService, ClaveGeneratorService>();
builder.Services.AddScoped<IConsecutiveService, ConsecutiveService>();
builder.Services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();
builder.Services.AddScoped<IXmlSignerService, XmlSignerService>();
builder.Services.AddScoped<IElectronicInvoiceService, ElectronicInvoiceService>();
builder.Services.AddScoped<IInvoiceValidationService, InvoiceValidationService>();

// =========================
// HACIENDA STATUS POLLING (Background Service)
// =========================
builder.Services.AddHostedService<HaciendaStatusPollingService>();

// =========================
// EMAIL SERVICE
// =========================
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IInvoiceEmailService, InvoiceEmailService>();

// =========================
// JWT SERVICE
// =========================
builder.Services.AddScoped<JwtService>();

// =========================
// CORS
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",                 // Vite dev
                "http://localhost:52802",                // optional local
                "https://syncro-fe-3dfd.vercel.app"       // Vercel prod
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =========================
// JWT AUTHENTICATION
// =========================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("Jwt:Key no está configurado en appsettings.json");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// =========================
// CONTROLLERS + SWAGGER
// =========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SyncroBE API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
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

var uploadsDirectory = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "route-deliveries");
Directory.CreateDirectory(uploadsDirectory);

// =========================
// SEED
// =========================
await SyncroBE.API.Seed.UserSeeder.SeedAsync(app.Services);

// =========================
// MIDDLEWARE
// =========================
// Swagger always enabled (useful for testing on Render)
app.UseSwagger();
app.UseSwaggerUI();


app.UseRouting();
app.UseStaticFiles();

app.UseCors("AllowReact");  

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();