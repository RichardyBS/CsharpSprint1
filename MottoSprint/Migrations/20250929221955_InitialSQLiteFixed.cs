using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MottoSprint.Migrations
{
    /// <inheritdoc />
    public partial class InitialSQLiteFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlacaMoto = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    VagaId = table.Column<int>(type: "INTEGER", nullable: true),
                    NotificationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "GENERAL"),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "NORMAL")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpotNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsOccupied = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    OccupiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VehiclePlate = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLIENTE",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    NOME = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    EMAIL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CPF = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    ATIVO = table.Column<bool>(type: "INTEGER", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLIENTE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CONFIGURACAO_NOTIFICACAO",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    NOTIFICAR_ENTRADA = table.Column<bool>(type: "INTEGER", nullable: false),
                    NOTIFICAR_SAIDA = table.Column<bool>(type: "INTEGER", nullable: false),
                    EMAIL_NOTIFICACAO = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TELEFONE_NOTIFICACAO = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CONFIGURACAO_NOTIFICACAO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_ESTATISTICAS_ESTACIONAMENTO",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DATA_REFERENCIA = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TOTAL_VAGAS = table.Column<int>(type: "INTEGER", nullable: false),
                    VAGAS_OCUPADAS = table.Column<int>(type: "INTEGER", nullable: false),
                    VAGAS_LIVRES = table.Column<int>(type: "INTEGER", nullable: false),
                    TOTAL_ENTRADAS = table.Column<int>(type: "INTEGER", nullable: false),
                    TOTAL_SAIDAS = table.Column<int>(type: "INTEGER", nullable: false),
                    TEMPO_MEDIO_PERMANENCIA = table.Column<decimal>(type: "TEXT", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESTATISTICAS_ESTACIONAMENTO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_FILA_ENTRADA",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MOTO_PLACA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    VAGA_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TIMESTAMP_ENTRADA = table.Column<DateTime>(type: "TEXT", nullable: false),
                    STATUS = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_FILA_ENTRADA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_FILA_SAIDA",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MOTO_PLACA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    VAGA_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TIMESTAMP_SAIDA = table.Column<DateTime>(type: "TEXT", nullable: false),
                    STATUS = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_FILA_SAIDA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_LOG_MOVIMENTACAO",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MOTO_PLACA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    VAGA_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TIPO_MOVIMENTACAO = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TIMESTAMP_EVENTO = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DETALHES = table.Column<string>(type: "TEXT", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_LOG_MOVIMENTACAO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_NOTIFICACAO_MOTO",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MOTO_PLACA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    VAGA_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TIPO_MOVIMENTACAO = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TIMESTAMP_EVENTO = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MENSAGEM = table.Column<string>(type: "TEXT", nullable: false),
                    LIDA = table.Column<bool>(type: "INTEGER", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_NOTIFICACAO_MOTO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_MOTO",
                columns: table => new
                {
                    PLACA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CLIENTE_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MODELO = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    COR = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ANO = table.Column<int>(type: "INTEGER", nullable: true),
                    STATUS = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ATIVA = table.Column<bool>(type: "INTEGER", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_MOTO", x => x.PLACA);
                    table.ForeignKey(
                        name: "FK_TB_MOTO_TB_CLIENTE_CLIENTE_ID",
                        column: x => x.CLIENTE_ID,
                        principalTable: "TB_CLIENTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_VAGA",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    NUMERO = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LINHA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    COLUNA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OCUPADA = table.Column<bool>(type: "INTEGER", nullable: false),
                    ATIVA = table.Column<bool>(type: "INTEGER", nullable: false),
                    PLACA_MOTO = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_VAGA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_VAGA_TB_MOTO_PLACA_MOTO",
                        column: x => x.PLACA_MOTO,
                        principalTable: "TB_MOTO",
                        principalColumn: "PLACA");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationType",
                table: "Notifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PlacaMoto",
                table: "Notifications",
                column: "PlacaMoto");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VagaId",
                table: "Notifications",
                column: "VagaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_IsOccupied",
                table: "ParkingSpots",
                column: "IsOccupied");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_SpotNumber",
                table: "ParkingSpots",
                column: "SpotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MOTO_CLIENTE_ID",
                table: "TB_MOTO",
                column: "CLIENTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_VAGA_PLACA_MOTO",
                table: "TB_VAGA",
                column: "PLACA_MOTO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "TB_CONFIGURACAO_NOTIFICACAO");

            migrationBuilder.DropTable(
                name: "TB_ESTATISTICAS_ESTACIONAMENTO");

            migrationBuilder.DropTable(
                name: "TB_FILA_ENTRADA");

            migrationBuilder.DropTable(
                name: "TB_FILA_SAIDA");

            migrationBuilder.DropTable(
                name: "TB_LOG_MOVIMENTACAO");

            migrationBuilder.DropTable(
                name: "TB_NOTIFICACAO_MOTO");

            migrationBuilder.DropTable(
                name: "TB_VAGA");

            migrationBuilder.DropTable(
                name: "TB_MOTO");

            migrationBuilder.DropTable(
                name: "TB_CLIENTE");
        }
    }
}
