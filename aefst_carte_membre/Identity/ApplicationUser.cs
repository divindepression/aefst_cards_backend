using Microsoft.AspNetCore.Identity;

namespace aefst_carte_membre.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public bool MustChangePassword { get; set; } = false;
        //public bool IsActive { get; set; } = true;
    }
}
