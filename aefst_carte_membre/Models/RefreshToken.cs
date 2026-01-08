using aefst_carte_membre.Identity;

namespace aefst_carte_membre.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }

}
