using Mango.MessageBus;
using Mango.Services.PaymentAPI.Extensions;
using Mango.Services.PaymentAPI.Messaging;
using PaymentProcessor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationServices(builder.Services);

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
    services.AddSingleton<IProcessPayment, ProcessPayment>();
    services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
    services.AddSingleton<IMessageBus, AzureServiceMessageBus>();
}
