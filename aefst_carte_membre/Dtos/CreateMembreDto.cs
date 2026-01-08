namespace aefst_carte_membre.Dtos
{
    public class CreateMembreDto
    {
        public string Email { get; set; } = null!;
        public string Role { get; set; } = "MEMBRE";

        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public DateTime DateNaissance { get; set; }
        public string LieuNaissance { get; set; } = null!;
        public string Option { get; set; } = null!;
        public string Cycle { get; set; } = null!;
        public string Niveau { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public DateTime DateExpiration { get; set; }
        public IFormFile Photo { get; set; } = null!;
    }

}
