using Mango.MessageBus;
using Mango.Services.Email.DbContexts;
using Mango.Services.Email.Extensions;
using Mango.Services.Email.Messaging;
using Mango.Services.Email.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationServices(builder.Services);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

app.UseAuthorization();
app.UseAzureServiceBusConsumer();
app.MapControllers();

app.Run();


void ConfigurationServices(IServiceCollection services)
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

    optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
   

    services.AddSingleton(new EmailRepository(optionsBuilder.Options));
    services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

    services.AddScoped<IEmailRepository, EmailRepository>();
    services.AddSingleton<IMessageBus, AzureServiceMessageBus>();
    services.AddHostedService<RabbitMQEmailConsumer>();
}