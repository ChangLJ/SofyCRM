using System.Text;
using System.Text.Json.Serialization;
using Audit.Core;
using Audit.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;
using SofyCRM.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOpt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

// ---------------------------------------------------------------
// DbContext + EFCore.NamingConventions (snake_case) + Npgsql Enums
// ---------------------------------------------------------------
var connStr = builder.Configuration.GetConnectionString("Default")
              ?? "Host=localhost;Port=5432;Database=sofycrm;Username=sofycrm;Password=sofycrm";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
dataSourceBuilder.MapEnum<UserRole>("user_role");
dataSourceBuilder.MapEnum<UserStatus>("user_status");
dataSourceBuilder.MapEnum<CustomerStatus>("customer_status");
dataSourceBuilder.MapEnum<FollowupType>("followup_type");
dataSourceBuilder.MapEnum<OpportunityStatus>("opportunity_status");
dataSourceBuilder.MapEnum<QuotationStatus>("quotation_status");
dataSourceBuilder.MapEnum<ProjectStatus>("project_status");
dataSourceBuilder.MapEnum<SofyCRM.Api.Entities.TaskStatus>("task_status");
dataSourceBuilder.MapEnum<TicketStatus>("ticket_status");
dataSourceBuilder.MapEnum<TicketPriority>("ticket_priority");
dataSourceBuilder.MapEnum<ExpenseCategory>("expense_category");
dataSourceBuilder.MapEnum<ExpenseStatus>("expense_status");
dataSourceBuilder.MapEnum<PaymentStatus>("payment_status");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(dataSource)
       .UseSnakeCaseNamingConvention());

// ---------------------------------------------------------------
// Audit.NET — 寫進 audit_logs
// ---------------------------------------------------------------
Audit.Core.Configuration.Setup()
    .UseEntityFramework(ef => ef
        .AuditTypeMapper(_ => typeof(AuditLog))
        .AuditEntityAction<AuditLog>((evt, entry, entity) =>
        {
            entity.Module     = entry.EntityType.Name;
            entity.Action     = entry.Action;
            entity.EntityId   = entry.PrimaryKey.Values.FirstOrDefault()?.ToString();
            entity.BeforeData = System.Text.Json.JsonSerializer.Serialize(entry.Changes);
            entity.AfterData  = System.Text.Json.JsonSerializer.Serialize(entry.ColumnValues);
            entity.CreatedAt  = DateTime.UtcNow;
            return true;
        })
        .IgnoreMatchedProperties(true));

// ---------------------------------------------------------------
// AuthN / AuthZ
// ---------------------------------------------------------------
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtOpt.Issuer,
            ValidAudience            = jwtOpt.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.Secret)),
            ClockSkew                = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// ---------------------------------------------------------------
// CORS
// ---------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(o => o.AddPolicy("default", p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// ---------------------------------------------------------------
// MVC + Swagger
// ---------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SofyCRM API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Description = "Bearer {token}",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        Reference   = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [jwtScheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ---------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "SofyCRM.Api", version = "1.0.0" }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

// ---------------------------------------------------------------
// 自動初始化
// ---------------------------------------------------------------
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.InitializeAsync(app.Services, logger);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database initialization failed.");
}

app.Run();
