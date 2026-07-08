using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part3Providers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputeProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    DefaultRegion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsValidated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastValidatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputeProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComputeProviders_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ComputeProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderCredentials_ComputeProviders_ComputeProviderId",
                        column: x => x.ComputeProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderGpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ComputeProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuType = table.Column<int>(type: "int", nullable: false),
                    GpuId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    MemoryGb = table.Column<int>(type: "int", nullable: true),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderGpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderGpus_ComputeProviders_ComputeProviderId",
                        column: x => x.ComputeProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderHealth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ComputeProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderHealth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderHealth_ComputeProviders_ComputeProviderId",
                        column: x => x.ComputeProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderHealthHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ComputeProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderHealthHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderHealthHistory_ComputeProviders_ComputeProviderId",
                        column: x => x.ComputeProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderRegions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ComputeProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RegionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderRegions_ComputeProviders_ComputeProviderId",
                        column: x => x.ComputeProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ComputeProviders_OrganizationId_Name",
                table: "ComputeProviders",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCredentials_ComputeProviderId",
                table: "ProviderCredentials",
                column: "ComputeProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderGpus_ComputeProviderId_GpuId",
                table: "ProviderGpus",
                columns: new[] { "ComputeProviderId", "GpuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderHealth_ComputeProviderId",
                table: "ProviderHealth",
                column: "ComputeProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderHealthHistory_ComputeProviderId_CheckedAt",
                table: "ProviderHealthHistory",
                columns: new[] { "ComputeProviderId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRegions_ComputeProviderId_RegionId",
                table: "ProviderRegions",
                columns: new[] { "ComputeProviderId", "RegionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderCredentials");

            migrationBuilder.DropTable(
                name: "ProviderGpus");

            migrationBuilder.DropTable(
                name: "ProviderHealth");

            migrationBuilder.DropTable(
                name: "ProviderHealthHistory");

            migrationBuilder.DropTable(
                name: "ProviderRegions");

            migrationBuilder.DropTable(
                name: "ComputeProviders");
        }
    }
}
