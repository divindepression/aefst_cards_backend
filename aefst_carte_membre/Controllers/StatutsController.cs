using aefst_carte_membre.DbContexts;
using aefst_carte_membre.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aefst_carte_membre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatutsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public StatutsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPatch("{id}/statut")]
        [Authorize(Roles = "ADMIN,BUREAU")]
        public async Task<IActionResult> ChangerStatut(
     Guid id,
     [FromBody] ChangerStatutDto dto)
        {
            if (!new[] { "ACTIF", "SUSPENDU", "EXPIRE" }.Contains(dto.Statut))
                return BadRequest("Statut invalide");

            var membre = await _db.membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            membre.Statut = dto.Statut;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                membre.Matricule,
                membre.Statut
            });
        }

    }
}
