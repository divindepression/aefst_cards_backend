using aefst_carte_membre.DbContexts;
using aefst_carte_membre.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aefst_carte_membre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificateursController : ControllerBase
    {
        private readonly AppDbContext _db;
        public VerificateursController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("verification/{matricule}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifierCarte(string matricule)
        {
            var membre = await _db.membres
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Matricule == matricule);

            if (membre == null)
                return NotFound(new { message = "Carte inexistante" });

            var maintenant = DateTime.UtcNow;

            var estExpiree = membre.DateExpiration < maintenant;

            var statutFinal = estExpiree ? "EXPIRE" : membre.Statut;

            var estValide =
                statutFinal == "ACTIF" &&
                !estExpiree;


            return Ok(new VerificationResultDto
            {
                Matricule = membre.Matricule,
                NomComplet = $"{membre.Prenom} {membre.Nom}",
                Option = membre.Option,
                Cycle = membre.Cycle,
                DateExpiration = membre.DateExpiration,
                Statut = membre.Statut,
                EstValide = estValide
            });
        }

    }
}
