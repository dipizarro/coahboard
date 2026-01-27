using System.Runtime.CompilerServices;
using System.IdentityModel.Tokens.Jwt;
// Middlewares (si tienes el de excepciones)
using CoachBoard.Api.Extensions;
using CoachBoard.Api.Services;
using CoachBoard.Application.Services;
using CoachBoard.API.Middlewares;
// Repositorios / App
using CoachBoard.Application.Interfaces;
// AutoMapper
using CoachBoard.Application.Mapping; // MappingProfile
using CoachBoard.Application.Validators; // CoachCreateDtoValidator
using CoachBoard.Infrastructure.Persistence;
using CoachBoard.Infrastructure.Repositories;
using CoachBoard.Infrastructure.Payment;
// FluentValidation
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

[assembly: InternalsVisibleTo("CoachBoard.Api.Tests")]

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/coachboard-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ------------ EF Core ------------
builder.Services.AddDbContext<CoachBoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------ Controllers ------------
builder.Services.AddControllers();

// ------------ FluentValidation (nuevo API, sin obsoletos) ------------
// Habilita la validación automática y adapters del lado cliente
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Registra validators buscando en el ensamblado donde está CoachCreateDtoValidator
builder.Services.AddValidatorsFromAssemblyContaining<CoachCreateDtoValidator>();
// (Alternativa equivalente)
// builder.Services.AddValidatorsFromAssembly(typeof(CoachCreateDtoValidator).Assembly);

builder.Services.AddAutoMapper(cfg => { }, typeof(CoachBoard.Application.Mapping.MappingProfile).Assembly);

// ------------ Swagger ------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------ DI Repositorios ------------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICoachRepository, CoachRepository>();
builder.Services.AddScoped<CoachBoard.Application.Interfaces.IUserRepository,
                           CoachBoard.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<CoachBoard.Application.Interfaces.IJwtService,
                           CoachBoard.Infrastructure.Auth.JwtService>();
builder.Services.AddScoped<CoachBoard.Application.Interfaces.IClientRepository,
                           CoachBoard.Infrastructure.Repositories.ClientRepository>();
builder.Services.AddScoped<CoachBoard.Application.Interfaces.IExerciseRepository,
                           CoachBoard.Infrastructure.Repositories.ExerciseRepository>();
builder.Services.AddScoped<CoachBoard.Application.Interfaces.IRoutineRepository,
                           CoachBoard.Infrastructure.Repositories.RoutineRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();


// ------------ ModelState -> ProblemDetails (opcional, pero útil) ------------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var problem = new ProblemDetails
        {
            Title = "Errores de validación",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Revisa los campos enviados.",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["errors"] = errors;
        return new BadRequestObjectResult(problem);
    };
});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Swagger Bearer
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer. Ej: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var MyCors = "_coachboardCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyCors, p =>
        p.WithOrigins(
            "http://localhost:5173", // dev local
            "https://brave-tree-0a0b0830f.3.azurestaticapps.net" // front en Azure
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CoachBoard API", Version = "v1" });
    c.CustomOperationIds(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

// Rate limiting básico
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;              // 60 req/min por IP
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();
builder.Services.AddScoped<IPlanLimitsProvider, PlanLimitsProvider>();
builder.Services.AddScoped<IFeatureFlags, FeatureFlagsService>();

builder.Services.Configure<MercadoPagoOptions>(builder.Configuration.GetSection(MercadoPagoOptions.SectionName));
builder.Services.AddHttpClient<IMercadoPagoClient, MercadoPagoClient>();

var app = builder.Build();

//await app.SeedAdminAsync();
//await app.SeedAsync();

app.UseCors(MyCors);
// ------------ Middleware global de excepciones (si lo tienes) ------------
app.UseGlobalExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication(); // <-- importante antes de Authorization
app.UseAuthorization();

app.MapControllers();


app.MapHealthChecks("/health");    // básico
app.MapHealthChecks("/health/db"); // DB

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "CoachBoard.Api" }));

app.Run();

public partial class Program { }
