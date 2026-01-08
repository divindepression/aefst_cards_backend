namespace aefst_carte_membre.Dtos
{
    public class VerificationResultDto
    {
        public string Matricule { get; set; } = null!;
        public string NomComplet { get; set; } = null!;
        public string Option { get; set; } = null!;
        public string Cycle { get; set; } = null!;
        public DateTime DateExpiration { get; set; }
        public string Statut { get; set; } = null!;
        public bool EstValide { get; set; }
    }
}
