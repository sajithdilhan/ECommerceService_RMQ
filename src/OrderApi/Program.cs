using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderApi.Data;
using OrderApi.Events;
using OrderApi.Services;
using Shared.Authentication;
using Shared.Middlewares;
using Shared.Models;
using static Shared.Common.Constants;

var builder = WebApplication.CreateBuilder(args);

AddDependencies(builder);
AddMassTransit(builder);

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
            Name = ApiKeyHeaderName,
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
    var databaseName = builder.Configuration.GetConnectionString("OrderDatabase") ?? "OrderDatabase";
    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseInMemoryDatabase(databaseName));
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IOrdersService, OrdersService>();
    builder.Services.AddHealthChecks();
    builder.Services.AddOptions<AuthenticationOptions>().Bind(builder.Configuration.GetSection(AuthenticationSectionName));
    builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "OrderApi_";
    });
}

static void AddMassTransit(WebApplicationBuilder builder)
{
    var rabbitSettings = builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>();

    if (rabbitSettings == null || string.IsNullOrEmpty(rabbitSettings.Host) || string.IsNullOrEmpty(rabbitSettings.Username) || string.IsNullOrEmpty(rabbitSettings.Password))
    {
        throw new InvalidOperationException("RabbitMq settings are not properly configured.");
    }

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<UserCreatedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitSettings.Host, rabbitSettings.VirtualHost, h =>
            {
                h.Username(rabbitSettings.Username);
                h.Password(rabbitSettings.Password);
            });

            cfg.ReceiveEndpoint("order-service.user-created", e =>
            {
                e.ConfigureConsumer<UserCreatedConsumer>(context);
            });
        });
    });
}
