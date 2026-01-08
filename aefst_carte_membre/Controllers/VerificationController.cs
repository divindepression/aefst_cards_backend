using aefst_carte_membre.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aefst_carte_membre.Controllers
{
    [ApiController]
    [Route("api/verification")]
    public class VerificationController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VerificationController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{matricule}")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string matricule)
        {
            var membre = await _db.membres
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Matricule == matricule);

            if (membre == null)
                return Content(Html("Carte invalide", false));

            var isExpired = membre.DateExpiration < DateTime.UtcNow;

            return Content(
                Html(
                    $"{membre.Prenom} {membre.Nom}",
                    !isExpired,
                    membre
                ),
                "text/html"
            );
        }

        private string Html(string title, bool isValid, Membre? membre = null)
        {
            var statusColor = isValid ? "green" : "red";
            var statusText = isValid ? "CARTE VALIDE" : "CARTE EXPIRÉE";

            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Vérification carte AEFST</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: #f4f6f8;
            padding: 20px;
        }}
        .card {{
            max-width: 400px;
            margin: auto;
            background: white;
            padding: 20px;
            border-radius: 10px;
            text-align: center;
            box-shadow: 0 0 10px rgba(0,0,0,.1);
        }}
        .status {{
            color: {statusColor};
            font-size: 20px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h2>AEFST</h2>
        <div class='status'>{statusText}</div>

        {(membre == null ? "" : $@"
            <p><strong>Nom :</strong> {membre.Nom}</p>
            <p><strong>Prénom :</strong> {membre.Prenom}</p>
            <p><strong>Matricule :</strong> {membre.Matricule}</p>
            <p><strong>Option :</strong> {membre.Option}</p>
            <p><strong>Expiration :</strong> {membre.DateExpiration:dd/MM/yyyy}</p>
        ")}

        <hr/>
        <small>Vérification officielle AEFST</small>
    </div>
</body>
</html>";
        }
    }
}
