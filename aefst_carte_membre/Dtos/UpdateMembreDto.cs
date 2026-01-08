namespace aefst_carte_membre.Dtos
{
    public class UpdateMembreDto
    {
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public string Option { get; set; } = null!;
        public string Cycle { get; set; } = null!;
        public string Niveau { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public DateTime DateExpiration { get; set; }
    }
}
