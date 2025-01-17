using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace MovieRating.API.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddLoggingConfiguration(this WebApplicationBuilder builder)
    {
        var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"];

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithCorrelationId()
            .Enrich.WithClientIp()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Only add Elasticsearch if configured
        if (!string.IsNullOrEmpty(elasticsearchUrl))
        {
            loggerConfig.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = $"movierating-{builder.Environment.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                CustomFormatter = new ElasticsearchJsonFormatter()
            });
        }

        Log.Logger = loggerConfig.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}