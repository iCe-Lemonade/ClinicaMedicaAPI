using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicaAPI.Migrations
{
    /// <inheritdoc />
    public partial class VincularPacienteConvenio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvenioId",
                table: "Pacientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_ConvenioId",
                table: "Pacientes",
                column: "ConvenioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Convenios_ConvenioId",
                table: "Pacientes",
                column: "ConvenioId",
                principalTable: "Convenios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Convenios_ConvenioId",
                table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_ConvenioId",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "ConvenioId",
                table: "Pacientes");
        }
    }
}
