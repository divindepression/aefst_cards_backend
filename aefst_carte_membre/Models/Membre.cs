using System.ComponentModel.DataAnnotations.Schema;
using aefst_carte_membre.Identity;

public class Membre
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string Matricule { get; set; } = null!;

    public string Nom { get; set; } = null!;
    public string Prenom { get; set; } = null!;
    public DateTime DateNaissance { get; set; }
    public string LieuNaissance { get; set; } = null!;
    public string Option { get; set; } = null!;
    public string Cycle { get; set; } = null!;
    public string Niveau { get; set; } = null!;
    public string Telephone { get; set; } = null!;
    public string PhotoUrl { get; set; } = null!;
    public DateTime DateCreation { get; set; }
    public DateTime DateExpiration { get; set; }
    public string Statut { get; set; } = "ACTIF";
    public string? CartePdfUrl { get; set; }


    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

}
