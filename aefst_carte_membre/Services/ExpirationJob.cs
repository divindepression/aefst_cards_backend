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

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        using var scope = _scopeFactory.CreateScope();
        //        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        //        var maintenant = DateTime.UtcNow;

        //        var membresExpirés = await db.membres
        //            .Where(m => m.Statut == "ACTIF" && m.DateExpiration < maintenant)
        //            .ToListAsync(stoppingToken);

        //        foreach (var membre in membresExpirés)
        //            membre.Statut = "EXPIRE";

        //        await db.SaveChangesAsync(stoppingToken);

        //        // sommeil 24h
        //        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        //    }
        //}


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ⏳ Attendre un peu au démarrage (Railway / Docker)
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // ✅ Vérifier que la table existe
                    var tableExists = await db.Database
                        .ExecuteSqlRawAsync(
                            "SELECT 1 FROM information_schema.tables WHERE table_name = 'membres';",
                            cancellationToken: stoppingToken
                        );

                    if (tableExists == 0)
                    {
                        Console.WriteLine("⚠️ Table membres inexistante, job ignoré");
                    }
                    else
                    {
                        var membres = await db.membres.ToListAsync(stoppingToken);
                        // logique métier ici
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ ExpirationJob ignoré : " + ex.Message);
                }

                // ⏲️ relance toutes les 24h
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

    }

}
