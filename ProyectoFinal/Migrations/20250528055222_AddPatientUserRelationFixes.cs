using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientUserRelationFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                newName: "IDX_Patient_UserId");

            migrationBuilder.RenameIndex(
                name: "IDX_Appointments_UserId",
                table: "Appointments",
                newName: "IX_Appointments_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IDX_Patient_UserId",
                table: "Patients",
                newName: "IX_Patients_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                newName: "IDX_Appointments_UserId");
        }
    }
}
