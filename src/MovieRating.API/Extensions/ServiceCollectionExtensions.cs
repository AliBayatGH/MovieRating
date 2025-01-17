using FluentValidation;

namespace MovieRating.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidatorsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        var assembly = typeof(T).Assembly;
        var validatorType = typeof(IValidator<>);

        var validatorTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType));

        foreach (var validator in validatorTypes)
        {
            var interfaceType = validator.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType);
            services.AddScoped(interfaceType, validator);
        }

        return services;
    }
}