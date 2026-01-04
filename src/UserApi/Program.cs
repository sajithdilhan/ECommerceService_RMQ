using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Authentication;
using Shared.Contracts;
using Shared.Middlewares;
using UserApi.Data;
using UserApi.Events;
using UserApi.Services;
using static Shared.Common.Constants;

var builder = WebApplication.CreateBuilder(args);

AddDependencies(builder);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();

static void AddDependencies(WebApplicationBuilder builder)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "Enter your API Key",
            Type = SecuritySchemeType.ApiKey,
            Name = ApiKeyHeaderName,   // Same header you validate
            In = ParameterLocation.Header,
            Scheme = "ApiKeyScheme"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
    });

    var databaseName = builder.Configuration.GetConnectionString("UserDatabase") ?? "UserDatabase";
    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseInMemoryDatabase(databaseName));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUsersService, UsersService>();
    builder.Services.AddSingleton<IKafkaProducerWrapper, KafkaProducerWrapper>();
    builder.Services.AddHostedService<OrderConsumerService>();
    builder.Services.AddOptions<AuthenticationOptions>().Bind(builder.Configuration.GetSection(AuthenticationSectionName));
    builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "UserApi_";
    });
    builder.Services.AddHealthChecks();
    builder.Services.AddControllers();
}