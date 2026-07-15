using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part12UniversalAiProviderEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiFailoverEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    FromProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ToProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Reason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Succeeded = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiFailoverEvents", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ProviderKind = table.Column<int>(type: "int", nullable: false),
                    BaseUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DeploymentName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ApiVersion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsValidated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastValidatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProviders_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AiProviderCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProviderCredentials_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AiProviderHealth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: true),
                    ErrorRate = table.Column<double>(type: "double", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderHealth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProviderHealth_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ContextLength = table.Column<int>(type: "int", nullable: true),
                    Parameters = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    SupportsStreaming = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsVision = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsTools = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsEmbeddings = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InputCostPerMillionTokens = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    OutputCostPerMillionTokens = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderModels_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoutingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    PrimaryProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FallbackProviderIdsJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    FailoverStrategy = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingPolicies_AiProviders_PrimaryProviderId",
                        column: x => x.PrimaryProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AiFailoverEvents_OrganizationId_OccurredAt",
                table: "AiFailoverEvents",
                columns: new[] { "OrganizationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderCredentials_AiProviderId",
                table: "AiProviderCredentials",
                column: "AiProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderHealth_AiProviderId",
                table: "AiProviderHealth",
                column: "AiProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_OrganizationId_Name",
                table: "AiProviders",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderModels_AiProviderId_ModelName",
                table: "ProviderModels",
                columns: new[] { "AiProviderId", "ModelName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderModels_OrganizationId_ModelName",
                table: "ProviderModels",
                columns: new[] { "OrganizationId", "ModelName" });

            migrationBuilder.CreateIndex(
                name: "IX_RoutingPolicies_OrganizationId_Name",
                table: "RoutingPolicies",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingPolicies_PrimaryProviderId",
                table: "RoutingPolicies",
                column: "PrimaryProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiFailoverEvents");

            migrationBuilder.DropTable(
                name: "AiProviderCredentials");

            migrationBuilder.DropTable(
                name: "AiProviderHealth");

            migrationBuilder.DropTable(
                name: "ProviderModels");

            migrationBuilder.DropTable(
                name: "RoutingPolicies");

            migrationBuilder.DropTable(
                name: "AiProviders");
        }
    }
}
