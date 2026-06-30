using EfCoreDemo.Infrastructure.Common;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCoreDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Serviços de apoio aos interceptors.
        services.AddScoped<ICurrentUserService, SystemCurrentUserService>();
        services.AddSingleton<IClock, SystemClock>();

        // Sink de captura de SQL: escopo por requisição (cada chamada começa limpa).
        services.AddScoped<SqlCaptureSink>();

        // Interceptors.
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SqlCaptureInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options
                .UseSqlite(connectionString)
                // Lazy Loading via proxies dinâmicos (navegações virtual).
                .UseLazyLoadingProxies()
                // Interceptors resolvidos do contêiner (escopo por requisição).
                .AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<SqlCaptureInterceptor>())
                // Facilita a leitura do SQL capturado durante a demonstração.
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                // Os filtros de soft delete em entidades com navegações obrigatórias
                // geram este aviso; aqui é intencional e compreendido.
                .ConfigureWarnings(w => w.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId
                        .PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        });

        return services;
    }
}
