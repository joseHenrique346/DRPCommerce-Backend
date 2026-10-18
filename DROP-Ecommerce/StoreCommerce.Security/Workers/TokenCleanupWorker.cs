using Commerce.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Security.Workers;

/// <summary>
/// Executa a limpeza periódica dos controles de identidade sem interromper a API quando uma execução falha.
/// </summary>
public sealed class TokenCleanupWorker(IServiceScopeFactory scopes, TimeProvider time, IOptions<SecurityOptions> options, ILogger<TokenCleanupWorker> logger) : BackgroundService
{
    /// <summary>
    /// Agenda a limpeza de tokens antigos respeitando a retenção, evitando acúmulo sem eliminar evidências de reutilização cedo demais.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var sessions = scope.ServiceProvider.GetRequiredService<IAuthenticationSessionRepository>();
                var before = time.GetUtcNow().UtcDateTime.AddDays(-options.Value.TokenRetentionDays);
                await sessions.CleanupAsync(before, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Limpeza dos controles de identidade indisponível. Será tentada no próximo ciclo.");
            }
        }
    }
}
