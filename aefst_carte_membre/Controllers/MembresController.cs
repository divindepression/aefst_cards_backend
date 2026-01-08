using System.Security.Claims;
using aefst_carte_membre.DbContexts;
using aefst_carte_membre.Dtos;
using aefst_carte_membre.Identity;
using aefst_carte_membre.Pdf;
using aefst_carte_membre.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aefst_carte_membre.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/membres")]
    public class MembresController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public MembresController(
            AppDbContext db,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
            _emailService = emailService;
        }

        // --------------------------------------------------------------------
        // UTIL
        // --------------------------------------------------------------------
        private async Task<string> GenerateUniqueMatriculeAsync()
        {
            var year = DateTime.UtcNow.Year;
            string matricule;

            do
            {
                matricule = $"AEFST-{year}-{Random.Shared.Next(1000, 9999)}";
            }
            while (await _db.membres.AnyAsync(m => m.Matricule == matricule));

            return matricule;
        }

        // --------------------------------------------------------------------
        // CREATE
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMembreDto dto)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath!, "photos");
            Directory.CreateDirectory(uploadsPath);

            var photoFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Photo.FileName)}";
            var photoPath = Path.Combine(uploadsPath, photoFileName);

            await using (var stream = new FileStream(photoPath, FileMode.Create))
                await dto.Photo.CopyToAsync(stream);

            var matricule = await GenerateUniqueMatriculeAsync();
            var passwordTemp = PasswordGenerator.Generate();

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(user, passwordTemp);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            var membre = new Membre
            {
                Matricule = matricule,
                Nom = dto.Nom,
                Prenom = dto.Prenom,
                DateNaissance = DateTime.SpecifyKind(dto.DateNaissance, DateTimeKind.Utc),
                LieuNaissance = dto.LieuNaissance,
                Option = dto.Option,
                Cycle = dto.Cycle,
                Niveau = dto.Niveau,
                Telephone = dto.Telephone,
                PhotoUrl = $"photos/{photoFileName}",
                DateExpiration = DateTime.SpecifyKind(dto.DateExpiration, DateTimeKind.Utc),
                Statut = "ACTIF",
                UserId = user.Id
            };

            _db.membres.Add(membre);
            await _db.SaveChangesAsync();

            // Génération carte PDF
            var pdfBytes = CartePdfGenerator.Generate(membre, _env);
            var cartesDir = Path.Combine(_env.WebRootPath!, "cartes");
            Directory.CreateDirectory(cartesDir);

            var pdfFileName = $"{membre.Matricule}.pdf";
            await System.IO.File.WriteAllBytesAsync(
                Path.Combine(cartesDir, pdfFileName),
                pdfBytes
            );

            membre.CartePdfUrl = $"cartes/{pdfFileName}";
            await _db.SaveChangesAsync();

            await _emailService.SendAccountCreatedEmail(
                dto.Email,
                passwordTemp,
                pdfBytes,
                pdfFileName
            );

            return Ok(new
            {
                membre.Id,
                membre.Matricule,
                dto.Email,
                Role = dto.Role
            });
        }

        // --------------------------------------------------------------------
        // READ — LISTE
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN,BUREAU")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var membres = await _db.membres
                .Include(m => m.User)
                .Select(m => new
                {
                    m.Id,
                    m.Matricule,
                    m.Nom,
                    m.Prenom,
                    m.Option,
                    m.Cycle,
                    m.Niveau,
                    m.Telephone,
                    m.PhotoUrl,
                    m.DateExpiration,
                    m.Statut,
                    Role = _db.UserRoles
                        .Where(ur => ur.UserId == m.UserId)
                        .Join(
                            _db.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => r.Name
                        )
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(membres);
        }

        // --------------------------------------------------------------------
        // READ — DÉTAIL
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN,BUREAU")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var membre = await _db.membres
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membre == null)
                return NotFound();

            var role = await _db.UserRoles
                .Where(ur => ur.UserId == membre.UserId)
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                membre.Id,
                membre.Matricule,
                membre.Nom,
                membre.Prenom,
                membre.DateNaissance,
                membre.LieuNaissance,
                membre.Option,
                membre.Cycle,
                membre.Niveau,
                membre.Telephone,
                membre.PhotoUrl,
                membre.CartePdfUrl,
                membre.DateExpiration,
                membre.Statut,
                Role = role
            });
        }

        // --------------------------------------------------------------------
        // UPDATE
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN,BUREAU")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateMembreDto dto)
        {
            var membre = await _db.membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            membre.Nom = dto.Nom;
            membre.Prenom = dto.Prenom;
            membre.Option = dto.Option;
            membre.Cycle = dto.Cycle;
            membre.Niveau = dto.Niveau;
            membre.Telephone = dto.Telephone;
            membre.DateExpiration = DateTime.SpecifyKind(dto.DateExpiration, DateTimeKind.Utc);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // --------------------------------------------------------------------
        // STATUT (SOFT DELETE)
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}/statut")]
        public async Task<IActionResult> ToggleStatut(Guid id)
        {
            var membre = await _db.membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            membre.Statut = membre.Statut == "ACTIF" ? "INACTIF" : "ACTIF";
            await _db.SaveChangesAsync();

            return Ok(new { membre.Statut });
        }

        // --------------------------------------------------------------------
        // CARTE PDF
        // --------------------------------------------------------------------
        [Authorize]
        [HttpGet("{id:guid}/carte")]
        public async Task<IActionResult> DownloadCarte(Guid id)
        {
            var membre = await _db.membres.FindAsync(id);
            if (membre == null || string.IsNullOrEmpty(membre.CartePdfUrl))
                return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath!, membre.CartePdfUrl);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(
                fullPath,
                "application/pdf",
                Path.GetFileName(fullPath)
            );
        }

        // --------------------------------------------------------------------
        // ME
        // --------------------------------------------------------------------
        [Authorize(Roles = "ADMIN,BUREAU,MEMBRE")]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var membre = await _db.membres
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (membre == null)
                return NotFound("Aucun membre lié à ce compte");

            return Ok(membre);
        }
    }
}
