using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part11Observability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RaisedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AlertType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ModelName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertHistory", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CostSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Period = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ModelName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    HourlyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DailyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WeeklyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MonthlyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProjectedMonthlyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AutoShutdownSavings = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostSnapshots", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MetricsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ModelName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    GpuUtilizationPercent = table.Column<double>(type: "double", nullable: false),
                    GpuMemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    GpuMemoryTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    CpuUtilizationPercent = table.Column<double>(type: "double", nullable: false),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    MemoryTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    DiskUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    DiskTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    NetworkInBytes = table.Column<long>(type: "bigint", nullable: false),
                    NetworkOutBytes = table.Column<long>(type: "bigint", nullable: false),
                    TemperatureCelsius = table.Column<double>(type: "double", nullable: true),
                    PowerWatts = table.Column<double>(type: "double", nullable: true),
                    ActiveStreams = table.Column<int>(type: "int", nullable: false),
                    QueueSize = table.Column<int>(type: "int", nullable: false),
                    InferenceCount = table.Column<int>(type: "int", nullable: false),
                    TokensGenerated = table.Column<long>(type: "bigint", nullable: false),
                    AverageLatencyMs = table.Column<double>(type: "double", nullable: false),
                    ErrorRate = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricsSnapshots", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemHealthHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Component = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Metadata = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemHealthHistory", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsageStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Period = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ModelName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    TokenCount = table.Column<long>(type: "bigint", nullable: false),
                    InferenceCount = table.Column<int>(type: "int", nullable: false),
                    TotalLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    UptimeSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageStatistics", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_OrganizationId_IsActive_RaisedAt",
                table: "AlertHistory",
                columns: new[] { "OrganizationId", "IsActive", "RaisedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_OrganizationId_RaisedAt",
                table: "AlertHistory",
                columns: new[] { "OrganizationId", "RaisedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CostSnapshots_OrganizationId_Period_RecordedAt",
                table: "CostSnapshots",
                columns: new[] { "OrganizationId", "Period", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CostSnapshots_OrganizationId_RecordedAt",
                table: "CostSnapshots",
                columns: new[] { "OrganizationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricsSnapshots_OrganizationId_GpuPodId_RecordedAt",
                table: "MetricsSnapshots",
                columns: new[] { "OrganizationId", "GpuPodId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricsSnapshots_OrganizationId_ProviderId_RecordedAt",
                table: "MetricsSnapshots",
                columns: new[] { "OrganizationId", "ProviderId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricsSnapshots_OrganizationId_RecordedAt",
                table: "MetricsSnapshots",
                columns: new[] { "OrganizationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemHealthHistory_OrganizationId_Component_RecordedAt",
                table: "SystemHealthHistory",
                columns: new[] { "OrganizationId", "Component", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemHealthHistory_OrganizationId_RecordedAt",
                table: "SystemHealthHistory",
                columns: new[] { "OrganizationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageStatistics_OrganizationId_Period_RecordedAt",
                table: "UsageStatistics",
                columns: new[] { "OrganizationId", "Period", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageStatistics_OrganizationId_RecordedAt",
                table: "UsageStatistics",
                columns: new[] { "OrganizationId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertHistory");

            migrationBuilder.DropTable(
                name: "CostSnapshots");

            migrationBuilder.DropTable(
                name: "MetricsSnapshots");

            migrationBuilder.DropTable(
                name: "SystemHealthHistory");

            migrationBuilder.DropTable(
                name: "UsageStatistics");
        }
    }
}
