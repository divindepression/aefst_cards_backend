using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aefst_carte_membre.Migrations
{
    /// <inheritdoc />
    public partial class IdentityInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CartePdfUrl",
                table: "membres",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartePdfUrl",
                table: "membres");
        }
    }
}
