using Common.Logging;

using Microsoft.EntityFrameworkCore;

using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration for Serilog sinks to ElasticSearch
builder.Host.UseSerilog((SeriLogger.Configure));

var configuration = builder.Configuration;
 
// Add services to the container.
var services = builder.Services;

// logging for http requests
services.AddTransient<LoggingDelegatingHanddler>();

services.AddApplicationServices();
services.AddInfrastructureServices(configuration);

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Migrate Database
using(var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
    dataContext.Database.Migrate();
}
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
