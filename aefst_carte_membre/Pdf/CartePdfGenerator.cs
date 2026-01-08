using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using aefst_carte_membre.Models;
using aefst_carte_membre.QrCode;

namespace aefst_carte_membre.Pdf
{
    public static class CartePdfGenerator
    {
        public static byte[] Generate(Membre membre, IWebHostEnvironment env)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var qrBytes = QrCodeGenerator.Generate(
                $"https://api.aefst.org/api/verification/{membre.Matricule}"
            );

            var logoPath = Path.Combine(env.WebRootPath, "assets/logo_aefst.png");
            var photoPath = Path.Combine(env.WebRootPath, membre.PhotoUrl);

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A6.Landscape());
                    page.Margin(6);

                    page.Content().Row(row =>
                    {
                        // ===== GAUCHE =====
                        row.ConstantItem(90).Column(col =>
                        {
                            col.Spacing(6);

                            col.Item()
                                .Height(30)
                                .Row(r =>
                                {
                                    r.ConstantItem(30)
                                        .Image(logoPath)
                                        .FitArea();

                                    r.RelativeItem()
                                        .AlignMiddle()
                                        .Text("AEFST")
                                        .FontSize(14)
                                        .Bold();
                                });

                            col.Item()
                                .Border(1)
                                .Padding(2)
                                .Image(photoPath)
                                .FitArea();
                        });

                        // ===== DROITE =====
                        row.RelativeItem().Column(col =>
                        {
                            col.Spacing(3);

                            col.Item().Text($"{membre.Prenom} {membre.Nom}")
                                .Bold().FontSize(10);

                            col.Item().Text($"Matricule : {membre.Matricule}").FontSize(8);
                            col.Item().Text($"Option : {membre.Option}").FontSize(8);
                            col.Item().Text($"Cycle : {membre.Cycle}").FontSize(8);
                            col.Item().Text($"Niveau : {membre.Niveau}").FontSize(8);
                            col.Item().Text($"Expire : {membre.DateExpiration:dd/MM/yyyy}")
                                .FontSize(7);

                            col.Item()
                                .PaddingTop(6)
                                .Image(qrBytes)
                                .FitArea();
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
