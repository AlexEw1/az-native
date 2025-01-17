using FoodApp;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
IConfiguration Configuration = builder.Configuration;
builder.Services.AddSingleton<IConfiguration>(Configuration);
var cfg = Configuration.Get<AppConfig>();

// Service Bus
var eb = new EventBus(cfg.ServiceBus.ConnectionString, cfg.ServiceBus.QueueName);
builder.Services.AddSingleton<EventBus>(eb);

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<AILogger>();

// Add Repositories for OrderAggregates and OrderEvents
OrderAggregates orderAggregates = new OrderAggregates(cfg.CosmosDB.GetConnectionString(), cfg.CosmosDB.DBName, cfg.CosmosDB.OrderAggregatesContainer);
builder.Services.AddSingleton<IOrderAggregates>(orderAggregates);

OrderEventsStore orderEventsStore = new OrderEventsStore(cfg.CosmosDB.GetConnectionString(), cfg.CosmosDB.DBName, cfg.CosmosDB.OrderEventsContainer);
builder.Services.AddSingleton<IOrderEventsStore>(orderEventsStore);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders-Api", Version = "v1" });
});

// Cors
builder.Services.AddCors(o => o.AddPolicy("nocors", builder =>
{
    builder
        .SetIsOriginAllowed(host => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food-Api");
    c.RoutePrefix = string.Empty;
});

app.UseCors("nocors");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();