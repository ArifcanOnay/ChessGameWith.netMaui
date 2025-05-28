using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatrancApi.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Oyuncular",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    isim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    renk = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oyuncular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oyunlar",
                columns: table => new
                {
                    OyunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeyazOyuncuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiyahOyuncuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BeyazSkor = table.Column<int>(type: "int", nullable: false),
                    SiyahSkor = table.Column<int>(type: "int", nullable: false),
                    BeyazKalanSure = table.Column<TimeSpan>(type: "time", nullable: true),
                    SiyahKalanSure = table.Column<TimeSpan>(type: "time", nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: true),
                    SiraKimin = table.Column<int>(type: "int", nullable: false),
                    ToplamHamleSayisi = table.Column<int>(type: "int", nullable: false),
                    SahDurumu = table.Column<bool>(type: "bit", nullable: false),
                    KazananOyuncu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BitisNedeni = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oyunlar", x => x.OyunId);
                    table.ForeignKey(
                        name: "FK_Oyunlar_Oyuncular_BeyazOyuncuId",
                        column: x => x.BeyazOyuncuId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oyunlar_Oyuncular_SiyahOyuncuId",
                        column: x => x.SiyahOyuncuId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Taslar",
                columns: table => new
                {
                    TasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OyunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OyuncuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    renk = table.Column<int>(type: "int", nullable: false),
                    turu = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    HicHareketEtmediMi = table.Column<bool>(type: "bit", nullable: false),
                    SonHareketTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnPassantTuru = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taslar", x => x.TasId);
                    table.ForeignKey(
                        name: "FK_Taslar_Oyuncular_OyuncuId",
                        column: x => x.OyuncuId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Taslar_Oyunlar_OyunId",
                        column: x => x.OyunId,
                        principalTable: "Oyunlar",
                        principalColumn: "OyunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hamleler",
                columns: table => new
                {
                    HamleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OyunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OyuncuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    turu = table.Column<int>(type: "int", nullable: false),
                    BaslangicX = table.Column<int>(type: "int", nullable: false),
                    BaslangicY = table.Column<int>(type: "int", nullable: false),
                    HedefX = table.Column<int>(type: "int", nullable: false),
                    HedefY = table.Column<int>(type: "int", nullable: false),
                    RokMu = table.Column<bool>(type: "bit", nullable: false),
                    EnPassantMi = table.Column<bool>(type: "bit", nullable: false),
                    TerfiEdildigiTas = table.Column<int>(type: "int", nullable: true),
                    SahMi = table.Column<bool>(type: "bit", nullable: false),
                    SahMatMi = table.Column<bool>(type: "bit", nullable: false),
                    Notasyon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YenilenTasId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HamleTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hamleler", x => x.HamleId);
                    table.ForeignKey(
                        name: "FK_Hamleler_Oyuncular_OyuncuId",
                        column: x => x.OyuncuId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hamleler_Oyunlar_OyunId",
                        column: x => x.OyunId,
                        principalTable: "Oyunlar",
                        principalColumn: "OyunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hamleler_Taslar_TasId",
                        column: x => x.TasId,
                        principalTable: "Taslar",
                        principalColumn: "TasId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hamle_OyunId_Tarih",
                table: "Hamleler",
                columns: new[] { "OyunId", "HamleTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Hamleler_OyuncuId",
                table: "Hamleler",
                column: "OyuncuId");

            migrationBuilder.CreateIndex(
                name: "IX_Hamleler_TasId",
                table: "Hamleler",
                column: "TasId");

            migrationBuilder.CreateIndex(
                name: "IX_Oyunlar_BeyazOyuncuId",
                table: "Oyunlar",
                column: "BeyazOyuncuId");

            migrationBuilder.CreateIndex(
                name: "IX_Oyunlar_SiyahOyuncuId",
                table: "Oyunlar",
                column: "SiyahOyuncuId");

            migrationBuilder.CreateIndex(
                name: "IX_Tas_OyunId_Aktif",
                table: "Taslar",
                columns: new[] { "OyunId", "AktifMi" });

            migrationBuilder.CreateIndex(
                name: "IX_Taslar_OyuncuId",
                table: "Taslar",
                column: "OyuncuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hamleler");

            migrationBuilder.DropTable(
                name: "Taslar");

            migrationBuilder.DropTable(
                name: "Oyunlar");

            migrationBuilder.DropTable(
                name: "Oyuncular");
        }
    }
}
