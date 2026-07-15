using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part14PluginSystemAndMcp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ServerKind = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    AuthScheme = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EncryptedCredential = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastError = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpServers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Plugins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PackageId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PluginType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    Publisher = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsFirstParty = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EntryAssembly = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    EntryType = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    RequiredPermissionsJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    SettingsSchemaJson = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    IsListed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugins", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "McpPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    McpServerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ArgumentsJson = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpPrompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpPrompts_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "McpResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    McpServerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Uri = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    MimeType = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpResources_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "McpToolExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    McpServerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ToolName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Succeeded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpToolExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpToolExecutions_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "McpTools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    McpServerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    InputSchemaJson = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpTools_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PluginInstallations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PluginDefinitionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EnabledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastHealthCheckAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HealthMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsHealthy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GrantedPermissionsJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginInstallations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginInstallations_Plugins_PluginDefinitionId",
                        column: x => x.PluginDefinitionId,
                        principalTable: "Plugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PluginLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PluginInstallationId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Level = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    DetailsJson = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginLogs_PluginInstallations_PluginInstallationId",
                        column: x => x.PluginInstallationId,
                        principalTable: "PluginInstallations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PluginSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PluginInstallationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: false),
                    IsSecret = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginSettings_PluginInstallations_PluginInstallationId",
                        column: x => x.PluginInstallationId,
                        principalTable: "PluginInstallations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_McpPrompts_McpServerId_Name",
                table: "McpPrompts",
                columns: new[] { "McpServerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpResources_McpServerId_Uri",
                table: "McpResources",
                columns: new[] { "McpServerId", "Uri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_OrganizationId_Name",
                table: "McpServers",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpToolExecutions_McpServerId",
                table: "McpToolExecutions",
                column: "McpServerId");

            migrationBuilder.CreateIndex(
                name: "IX_McpToolExecutions_OrganizationId_ExecutedAt",
                table: "McpToolExecutions",
                columns: new[] { "OrganizationId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_McpTools_McpServerId_Name",
                table: "McpTools",
                columns: new[] { "McpServerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginInstallations_OrganizationId_PluginDefinitionId",
                table: "PluginInstallations",
                columns: new[] { "OrganizationId", "PluginDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginInstallations_PluginDefinitionId",
                table: "PluginInstallations",
                column: "PluginDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginLogs_OrganizationId_OccurredAt",
                table: "PluginLogs",
                columns: new[] { "OrganizationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginLogs_PluginInstallationId",
                table: "PluginLogs",
                column: "PluginInstallationId");

            migrationBuilder.CreateIndex(
                name: "IX_Plugins_PackageId",
                table: "Plugins",
                column: "PackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginSettings_PluginInstallationId_Key",
                table: "PluginSettings",
                columns: new[] { "PluginInstallationId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpPrompts");

            migrationBuilder.DropTable(
                name: "McpResources");

            migrationBuilder.DropTable(
                name: "McpToolExecutions");

            migrationBuilder.DropTable(
                name: "McpTools");

            migrationBuilder.DropTable(
                name: "PluginLogs");

            migrationBuilder.DropTable(
                name: "PluginSettings");

            migrationBuilder.DropTable(
                name: "McpServers");

            migrationBuilder.DropTable(
                name: "PluginInstallations");

            migrationBuilder.DropTable(
                name: "Plugins");
        }
    }
}
