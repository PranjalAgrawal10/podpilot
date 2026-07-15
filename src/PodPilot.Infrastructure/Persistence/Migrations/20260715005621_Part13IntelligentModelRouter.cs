using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part13IntelligentModelRouter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryProviderId",
                table: "RoutingPolicies",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<double>(
                name: "AvailabilityWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ContextWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CostWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CustomRulesJson",
                table: "RoutingPolicies",
                type: "varchar(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FeaturesWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LatencyWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTaskTypesJson",
                table: "RoutingPolicies",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<double>(
                name: "ReliabilityWeight",
                table: "RoutingPolicies",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Strategy",
                table: "RoutingPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "QualityScore",
                table: "ProviderModels",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ReliabilityScore",
                table: "ProviderModels",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SpeedScore",
                table: "ProviderModels",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsReasoning",
                table: "ProviderModels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CostHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    GpuRuntimeMs = table.Column<int>(type: "int", nullable: true),
                    CostUsd = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    IsPredicted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostHistory_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LatencyHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    QueueDepth = table.Column<int>(type: "int", nullable: false),
                    PodLoadPercent = table.Column<double>(type: "double", nullable: false),
                    WasColdStart = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ColdStartMs = table.Column<int>(type: "int", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatencyHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LatencyHistory_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ModelScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AiProviderModelId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<double>(type: "double", nullable: false),
                    CostScore = table.Column<double>(type: "double", nullable: false),
                    LatencyScore = table.Column<double>(type: "double", nullable: false),
                    ReliabilityScore = table.Column<double>(type: "double", nullable: false),
                    ContextScore = table.Column<double>(type: "double", nullable: false),
                    FeaturesScore = table.Column<double>(type: "double", nullable: false),
                    AvailabilityScore = table.Column<double>(type: "double", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelScores_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelScores_ProviderModels_AiProviderModelId",
                        column: x => x.AiProviderModelId,
                        principalTable: "ProviderModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoutingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoutingPolicyId = table.Column<Guid>(type: "char(36)", nullable: true),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    Complexity = table.Column<int>(type: "int", nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    SelectedProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    SelectedModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OverallScore = table.Column<double>(type: "double", nullable: true),
                    EstimatedInputTokens = table.Column<int>(type: "int", nullable: false),
                    EstimatedOutputTokens = table.Column<int>(type: "int", nullable: false),
                    EstimatedCostUsd = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    EstimatedLatencyMs = table.Column<int>(type: "int", nullable: false),
                    FallbackCount = table.Column<int>(type: "int", nullable: false),
                    IsSimulation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: true),
                    DecisionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingEvents_AiProviders_SelectedProviderId",
                        column: x => x.SelectedProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RoutingEvents_RoutingPolicies_RoutingPolicyId",
                        column: x => x.RoutingPolicyId,
                        principalTable: "RoutingPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CostHistory_AiProviderId",
                table: "CostHistory",
                column: "AiProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_CostHistory_OrganizationId_RecordedAt",
                table: "CostHistory",
                columns: new[] { "OrganizationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LatencyHistory_AiProviderId",
                table: "LatencyHistory",
                column: "AiProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_LatencyHistory_OrganizationId_AiProviderId_RecordedAt",
                table: "LatencyHistory",
                columns: new[] { "OrganizationId", "AiProviderId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModelScores_AiProviderId",
                table: "ModelScores",
                column: "AiProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelScores_AiProviderModelId",
                table: "ModelScores",
                column: "AiProviderModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelScores_OrganizationId_AiProviderModelId_Strategy",
                table: "ModelScores",
                columns: new[] { "OrganizationId", "AiProviderModelId", "Strategy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingEvents_OrganizationId_DecidedAt",
                table: "RoutingEvents",
                columns: new[] { "OrganizationId", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RoutingEvents_RoutingPolicyId",
                table: "RoutingEvents",
                column: "RoutingPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingEvents_SelectedProviderId",
                table: "RoutingEvents",
                column: "SelectedProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostHistory");

            migrationBuilder.DropTable(
                name: "LatencyHistory");

            migrationBuilder.DropTable(
                name: "ModelScores");

            migrationBuilder.DropTable(
                name: "RoutingEvents");

            migrationBuilder.DropColumn(
                name: "AvailabilityWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "ContextWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "CostWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "CustomRulesJson",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "FeaturesWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "LatencyWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "PreferredTaskTypesJson",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "ReliabilityWeight",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "Strategy",
                table: "RoutingPolicies");

            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "ProviderModels");

            migrationBuilder.DropColumn(
                name: "ReliabilityScore",
                table: "ProviderModels");

            migrationBuilder.DropColumn(
                name: "SpeedScore",
                table: "ProviderModels");

            migrationBuilder.DropColumn(
                name: "SupportsReasoning",
                table: "ProviderModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryProviderId",
                table: "RoutingPolicies",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }
    }
}
