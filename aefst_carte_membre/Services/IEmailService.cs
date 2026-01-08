namespace aefst_carte_membre.Services
{
    public interface IEmailService
    {
        //Task SendAccountCreatedEmail(string toEmail, string passwordTemp);

        Task SendAccountCreatedEmail(
    string toEmail,
    string passwordTemp,
    byte[] cartePdfBytes,
    string carteFileName
);

    }




}
