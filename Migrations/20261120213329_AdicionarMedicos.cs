using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace ClinicaMedicaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMedicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Crm = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UfCrm = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    InicioExpediente = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    FimExpediente = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    DuracaoConsultaMin = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MedicoEspecialidade",
                columns: table => new
                {
                    EspecialidadesId = table.Column<int>(type: "int", nullable: false),
                    MedicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoEspecialidade", x => new { x.EspecialidadesId, x.MedicoId });
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidade_Especialidades_EspecialidadesId",
                        column: x => x.EspecialidadesId,
                        principalTable: "Especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidade_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MedicoEspecialidade_MedicoId",
                table: "MedicoEspecialidade",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_Crm_UfCrm",
                table: "Medicos",
                columns: new[] { "Crm", "UfCrm" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicoEspecialidade");

            migrationBuilder.DropTable(
                name: "Medicos");
        }
    }
}
