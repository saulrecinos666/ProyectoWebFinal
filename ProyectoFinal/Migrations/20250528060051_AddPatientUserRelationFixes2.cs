using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientUserRelationFixes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_UserId1",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_UserId1",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Patients");

            migrationBuilder.AddForeignKey(
                name: "FK__Patients__User__666",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Patients__User__666",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId1",
                table: "Patients",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId1",
                table: "Patients",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
