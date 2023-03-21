using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.DbContexts;
using Mango.Services.OrderAPI.Extensions;
using Mango.Services.OrderAPI.Messaging;
using Mango.Services.OrderAPI.RabbitMQSender;
using Mango.Services.OrderAPI.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://localhost:7281/";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "mango");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mango.Services.CouponAPI", Version = "V1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Enter 'Bearer' [space] and your token",
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
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name="Bearer",
                In=ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
ConfigurationSerivce(builder.Services);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseAzureServiceBusConsumer();
app.Run();
void ConfigurationSerivce(IServiceCollection services)
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

    optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    services.AddHostedService<RabbitMQCheckoutConsumer>();
    services.AddSingleton(new OrderRepository(optionsBuilder.Options));
    services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
    services.AddSingleton<IMessageBus, AzureServiceMessageBus>();
    services.AddSingleton<IRabbitMQOrderMessageSender, RabbitMQOrderMessageSender>();
}
