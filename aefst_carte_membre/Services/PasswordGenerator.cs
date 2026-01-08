namespace aefst_carte_membre.Services
{
    public static class PasswordGenerator
    {
        public static string Generate()
            => $"AEFST@{Guid.NewGuid().ToString("N")[..8]}";
    }

}
