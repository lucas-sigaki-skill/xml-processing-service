using Microsoft.Extensions.Logging.EventLog;
using XmlProcessingService.Models;
using XmlProcessingService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddEventLog(new EventLogSettings
    {
        SourceName = "XML Processing Service"
    });
});

// Configure service configuration
builder.Services.Configure<ServiceConfiguration>(
    builder.Configuration.GetSection("ServiceConfiguration"));

// Register service configuration as singleton
builder.Services.AddSingleton(provider =>
{
    var config = new ServiceConfiguration();
    builder.Configuration.GetSection("ServiceConfiguration").Bind(config);
    return config;
});

// Register services
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddSingleton<XmlProcessor>();
builder.Services.AddHostedService<XmlFileWatcherService>();

// Configure as Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "XML Processing Service";
});

var host = builder.Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Application terminated unexpectedly");
    throw;
}
