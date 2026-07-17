using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part17OneClickAiPodDeployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeploymentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Runtime = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ContainerImage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    StartupCommand = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    EnvironmentVariablesJson = table.Column<string>(type: "longtext", nullable: true),
                    HealthCheckPath = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    HealthCheckPort = table.Column<int>(type: "int", nullable: false),
                    RecommendedGpuCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    DefaultModelCodesJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentTemplates", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GpuCatalogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    GpuType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    VramGb = table.Column<int>(type: "int", nullable: false),
                    CudaCapability = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    EstimatedHourlyCostUsd = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProviderAvailabilityJson = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsCustom = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpuCatalogEntries", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ModelCatalogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    ModelReference = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Family = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Parameters = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Quantization = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ContextLength = table.Column<int>(type: "int", nullable: false),
                    RequiredVramGb = table.Column<int>(type: "int", nullable: false),
                    RecommendedGpuCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    MinimumGpuCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    SupportsVision = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsTools = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsEmbeddings = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    License = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    DownloadSizeGb = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreferredRuntime = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelCatalogEntries", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RuntimeVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Runtime = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Version = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    CudaVersion = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    ContainerImage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    HealthPath = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsRecommended = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeVersions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AiDeployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CloudProvider = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    GpuCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    ProviderGpuId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Runtime = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    CudaVersion = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GatewayRouteId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ImageName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    StatusMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CancellationRequested = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstimatedHourlyCostUsd = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EnvironmentVariablesJson = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDeployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiDeployments_ComputeProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AiDeployments_DeploymentTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "DeploymentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDeployments_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDeployments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeploymentHealthSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DeploymentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    GpuAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CudaAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RuntimeRunning = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ModelAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GatewayReachable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StreamingWorks = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DetailsJson = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentHealthSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentHealthSnapshots_AiDeployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "AiDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeploymentHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DeploymentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FromStatus = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    ToStatus = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentHistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentHistoryEntries_AiDeployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "AiDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeploymentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DeploymentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Level = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    Stage = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentLogs_AiDeployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "AiDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeploymentModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DeploymentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelCatalogId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ModelReference = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    DownloadStatus = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentModels_AiDeployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "AiDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeploymentModels_ModelCatalogEntries_ModelCatalogId",
                        column: x => x.ModelCatalogId,
                        principalTable: "ModelCatalogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AiDeployments_GpuPodId",
                table: "AiDeployments",
                column: "GpuPodId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDeployments_OrganizationId",
                table: "AiDeployments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDeployments_OrganizationId_Status",
                table: "AiDeployments",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AiDeployments_ProviderId",
                table: "AiDeployments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDeployments_TemplateId",
                table: "AiDeployments",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentHealthSnapshots_DeploymentId",
                table: "DeploymentHealthSnapshots",
                column: "DeploymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentHistoryEntries_DeploymentId_TimestampUtc",
                table: "DeploymentHistoryEntries",
                columns: new[] { "DeploymentId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentLogs_DeploymentId_TimestampUtc",
                table: "DeploymentLogs",
                columns: new[] { "DeploymentId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentModels_DeploymentId",
                table: "DeploymentModels",
                column: "DeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentModels_ModelCatalogId",
                table: "DeploymentModels",
                column: "ModelCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentTemplates_Code",
                table: "DeploymentTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentTemplates_Kind",
                table: "DeploymentTemplates",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_GpuCatalogEntries_Code",
                table: "GpuCatalogEntries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GpuCatalogEntries_IsActive",
                table: "GpuCatalogEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCatalogEntries_Code",
                table: "ModelCatalogEntries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelCatalogEntries_IsActive",
                table: "ModelCatalogEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCatalogEntries_ModelReference",
                table: "ModelCatalogEntries",
                column: "ModelReference");

            migrationBuilder.CreateIndex(
                name: "IX_RuntimeVersions_Runtime_Version",
                table: "RuntimeVersions",
                columns: new[] { "Runtime", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeploymentHealthSnapshots");

            migrationBuilder.DropTable(
                name: "DeploymentHistoryEntries");

            migrationBuilder.DropTable(
                name: "DeploymentLogs");

            migrationBuilder.DropTable(
                name: "DeploymentModels");

            migrationBuilder.DropTable(
                name: "GpuCatalogEntries");

            migrationBuilder.DropTable(
                name: "RuntimeVersions");

            migrationBuilder.DropTable(
                name: "AiDeployments");

            migrationBuilder.DropTable(
                name: "ModelCatalogEntries");

            migrationBuilder.DropTable(
                name: "DeploymentTemplates");
        }
    }
}
