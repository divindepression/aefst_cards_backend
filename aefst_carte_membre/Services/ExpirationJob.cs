using aefst_carte_membre.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace aefst_carte_membre.Services
{
    public class ExpirationJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpirationJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var maintenant = DateTime.UtcNow;

                var membresExpirés = await db.membres
                    .Where(m => m.Statut == "ACTIF" && m.DateExpiration < maintenant)
                    .ToListAsync(stoppingToken);

                foreach (var membre in membresExpirés)
                    membre.Statut = "EXPIRE";

                await db.SaveChangesAsync(stoppingToken);

                // sommeil 24h
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

}
