using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonitoramento.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoVinculoSetorEquipamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SensorId",
                table: "Medicoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SetorEquipamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetorEquipamento", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SetorEquipamentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensor_SetorEquipamento_SetorEquipamentoId",
                        column: x => x.SetorEquipamentoId,
                        principalTable: "SetorEquipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Medicoes_SensorId",
                table: "Medicoes",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_SetorEquipamentoId",
                table: "Sensor",
                column: "SetorEquipamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicoes_Sensor_SensorId",
                table: "Medicoes",
                column: "SensorId",
                principalTable: "Sensor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicoes_Sensor_SensorId",
                table: "Medicoes");

            migrationBuilder.DropTable(
                name: "Sensor");

            migrationBuilder.DropTable(
                name: "SetorEquipamento");

            migrationBuilder.DropIndex(
                name: "IX_Medicoes_SensorId",
                table: "Medicoes");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "Medicoes");
        }
    }
}
