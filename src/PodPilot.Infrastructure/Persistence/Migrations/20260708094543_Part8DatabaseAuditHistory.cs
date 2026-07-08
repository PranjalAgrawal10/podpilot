using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part8DatabaseAuditHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Tag = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Family = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Quantization = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ContextLength = table.Column<int>(type: "int", nullable: true),
                    Parameters = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    License = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiModels_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiModels_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DatabaseMigrationHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    MigrationId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ProductVersion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseMigrationHistory", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DatabaseSeedHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    SeederName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Details = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSeedHistory", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ModelDownloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DownloadSpeed = table.Column<long>(type: "bigint", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelDownloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelDownloads_AiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ModelHealthHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: true),
                    LastChecked = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelHealthHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelHealthHistory_AiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_OrganizationId_PodId_IsDefault",
                table: "AiModels",
                columns: new[] { "OrganizationId", "PodId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_OrganizationId_PodId_Name_Tag",
                table: "AiModels",
                columns: new[] { "OrganizationId", "PodId", "Name", "Tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_PodId",
                table: "AiModels",
                column: "PodId");

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_Status",
                table: "AiModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseMigrationHistory_AppliedAt",
                table: "DatabaseMigrationHistory",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseMigrationHistory_MigrationId",
                table: "DatabaseMigrationHistory",
                column: "MigrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseSeedHistory_AppliedAt",
                table: "DatabaseSeedHistory",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseSeedHistory_SeederName_Version_AppliedAt",
                table: "DatabaseSeedHistory",
                columns: new[] { "SeederName", "Version", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModelDownloads_ModelId_Status",
                table: "ModelDownloads",
                columns: new[] { "ModelId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ModelHealthHistory_ModelId_LastChecked",
                table: "ModelHealthHistory",
                columns: new[] { "ModelId", "LastChecked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseMigrationHistory");

            migrationBuilder.DropTable(
                name: "DatabaseSeedHistory");

            migrationBuilder.DropTable(
                name: "ModelDownloads");

            migrationBuilder.DropTable(
                name: "ModelHealthHistory");

            migrationBuilder.DropTable(
                name: "AiModels");
        }
    }
}
