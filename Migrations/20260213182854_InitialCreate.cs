using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect_ASPDOTNET.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanieId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nume = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CUI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataInregistrare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companii", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Depozite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepozitId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nume = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanieId = table.Column<int>(type: "int", nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitudine = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitudine = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapacitateMaxima = table.Column<int>(type: "int", nullable: false),
                    DataDeschidere = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depozite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depozite_Companii_CompanieId",
                        column: x => x.CompanieId,
                        principalTable: "Companii",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Marfuri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarfaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepozitId = table.Column<int>(type: "int", nullable: false),
                    CapacitateCurenta = table.Column<int>(type: "int", nullable: false),
                    UnitateMasura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PretUnitar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Etaj = table.Column<int>(type: "int", nullable: false),
                    Raft = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pozitie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAdaugare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaModificare = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marfuri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marfuri_Depozite_DepozitId",
                        column: x => x.DepozitId,
                        principalTable: "Depozite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumeComplet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    CompanieId = table.Column<int>(type: "int", nullable: true),
                    DepozitId = table.Column<int>(type: "int", nullable: true),
                    DataCreare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activ = table.Column<bool>(type: "bit", nullable: false),
                    PuncteReward = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companii_CompanieId",
                        column: x => x.CompanieId,
                        principalTable: "Companii",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Depozite_DepozitId",
                        column: x => x.DepozitId,
                        principalTable: "Depozite",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogActivitati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Actiune = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalii = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataOra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdresaIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepozitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogActivitati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogActivitati_Depozite_DepozitId",
                        column: x => x.DepozitId,
                        principalTable: "Depozite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LogActivitati_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sarcini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Titlu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataLimita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Finalizata = table.Column<bool>(type: "bit", nullable: false),
                    DataFinalizare = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PuncteReward = table.Column<int>(type: "int", nullable: false),
                    Prioritate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sarcini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sarcini_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tranzactii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranzactieId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    MarfaId = table.Column<int>(type: "int", nullable: false),
                    Cantitate = table.Column<int>(type: "int", nullable: false),
                    DepozitSursaId = table.Column<int>(type: "int", nullable: true),
                    DepozitDestinatieId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DataTranzactie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observatii = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValoareTotala = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tranzactii", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tranzactii_Depozite_DepozitDestinatieId",
                        column: x => x.DepozitDestinatieId,
                        principalTable: "Depozite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tranzactii_Depozite_DepozitSursaId",
                        column: x => x.DepozitSursaId,
                        principalTable: "Depozite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tranzactii_Marfuri_MarfaId",
                        column: x => x.MarfaId,
                        principalTable: "Marfuri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tranzactii_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companii_CompanieId",
                table: "Companii",
                column: "CompanieId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Depozite_CompanieId",
                table: "Depozite",
                column: "CompanieId");

            migrationBuilder.CreateIndex(
                name: "IX_Depozite_DepozitId",
                table: "Depozite",
                column: "DepozitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogActivitati_DepozitId",
                table: "LogActivitati",
                column: "DepozitId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActivitati_UserId",
                table: "LogActivitati",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Marfuri_DepozitId",
                table: "Marfuri",
                column: "DepozitId");

            migrationBuilder.CreateIndex(
                name: "IX_Sarcini_UserId",
                table: "Sarcini",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactii_DepozitDestinatieId",
                table: "Tranzactii",
                column: "DepozitDestinatieId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactii_DepozitSursaId",
                table: "Tranzactii",
                column: "DepozitSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactii_MarfaId",
                table: "Tranzactii",
                column: "MarfaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactii_UserId",
                table: "Tranzactii",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanieId",
                table: "Users",
                column: "CompanieId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepozitId",
                table: "Users",
                column: "DepozitId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogActivitati");

            migrationBuilder.DropTable(
                name: "Sarcini");

            migrationBuilder.DropTable(
                name: "Tranzactii");

            migrationBuilder.DropTable(
                name: "Marfuri");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Depozite");

            migrationBuilder.DropTable(
                name: "Companii");
        }
    }
}
