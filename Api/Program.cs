using System.Text;
using Api.Security;
using Application;
using Application.Common.Security;
using Application.Features.Bootstrap;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration section 'Jwt' was not found.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || jwtOptions.SecretKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey must be at least 32 characters.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IAuthTokenSettings, AuthTokenSettings>();

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

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
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoTallerManager API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token only. Do not write Bearer."
    });

    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>());

        return requirement;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("BootstrapAdmin");

    var bootstrapSettings = app.Configuration
        .GetSection("BootstrapAdmin")
        .Get<BootstrapAdminSettings>();

    if (bootstrapSettings is null)
    {
        logger.LogWarning(
            "BootstrapAdmin configuration section was not found. Skipping bootstrap admin initialization.");
    }
    else
    {
        var bootstrapService = scope.ServiceProvider.GetRequiredService<IDevelopmentAdminBootstrapService>();
        var bootstrapResult = await bootstrapService.EnsureBootstrapAdminAsync(bootstrapSettings);

        if (bootstrapResult.IsFailure)
        {
            logger.LogWarning(
                "Bootstrap admin was not created. Code: {Code}. Message: {Message}",
                bootstrapResult.Error.Code,
                bootstrapResult.Error.Message);
        }
        else
        {
            var result = bootstrapResult.Value!;

            if (result.Created)
            {
                logger.LogInformation(
                    "Bootstrap admin created. UserId: {UserId}. PersonId: {PersonId}. Email: {Email}. Message: {Message}",
                    result.UserId,
                    result.PersonId,
                    result.Email,
                    result.Message);
            }
            else
            {
                logger.LogInformation(
                    "Bootstrap admin skipped. Message: {Message}",
                    result.Message);
            }
        }
    }
}

app.UseHttpsRedirection();

app.UseCors("FrontendCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
