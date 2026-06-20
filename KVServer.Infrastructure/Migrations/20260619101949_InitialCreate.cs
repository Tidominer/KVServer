using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KVServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Storages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StorageId = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Keys_Storages_StorageId",
                        column: x => x.StorageId,
                        principalTable: "Storages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    EncryptedValue = table.Column<string>(type: "TEXT", nullable: false),
                    IV = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Versions_Keys_KeyId",
                        column: x => x.KeyId,
                        principalTable: "Keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keys_StorageId",
                table: "Keys",
                column: "StorageId");

            migrationBuilder.CreateIndex(
                name: "IX_Keys_StorageId_KeyName",
                table: "Keys",
                columns: new[] { "StorageId", "KeyName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Storages_AccessToken",
                table: "Storages",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Storages_Name",
                table: "Storages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Versions_CreatedAt",
                table: "Versions",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Versions_KeyId",
                table: "Versions",
                column: "KeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_KeyId_VersionNumber",
                table: "Versions",
                columns: new[] { "KeyId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropTable(
                name: "Keys");

            migrationBuilder.DropTable(
                name: "Storages");
        }
    }
}
