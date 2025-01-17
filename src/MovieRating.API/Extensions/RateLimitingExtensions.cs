using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace MovieRating.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Token bucket algorithm (similar to Bitbucket)
            options.AddTokenBucketLimiter("API", config =>
            {
                config.TokenLimit = configuration.GetValue<int>("RateLimiting:TokenLimit", 100); // Tokens per bucket
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 2); // Max queue size
                config.ReplenishmentPeriod = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:ReplenishmentPeriodSeconds", 1));
                config.TokensPerPeriod = configuration.GetValue<int>("RateLimiting:TokensPerPeriod", 10);
                config.AutoReplenishment = true;
            });
        });

        return services;
    }
}