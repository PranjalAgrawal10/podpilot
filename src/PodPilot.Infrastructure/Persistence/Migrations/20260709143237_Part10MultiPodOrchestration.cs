using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part10MultiPodOrchestration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapacitySnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodPoolId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalPods = table.Column<int>(type: "int", nullable: false),
                    HealthyPods = table.Column<int>(type: "int", nullable: false),
                    BusyPods = table.Column<int>(type: "int", nullable: false),
                    QueueLength = table.Column<int>(type: "int", nullable: false),
                    AverageWaitTimeMs = table.Column<double>(type: "double", nullable: false),
                    AverageLatencyMs = table.Column<double>(type: "double", nullable: false),
                    GpuUtilizationPercent = table.Column<double>(type: "double", nullable: false),
                    ConcurrentStreams = table.Column<int>(type: "int", nullable: false),
                    CurrentCapacity = table.Column<double>(type: "double", nullable: false),
                    ProjectedCapacity = table.Column<double>(type: "double", nullable: false),
                    RemainingCapacity = table.Column<double>(type: "double", nullable: false),
                    MaximumThroughput = table.Column<double>(type: "double", nullable: false),
                    SuggestedScale = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacitySnapshots", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoadBalancerConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Strategy = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    StickySessionsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StickySessionTtlMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBalancerConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadBalancerConfigs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodHealthMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GpuHealthy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OllamaHealthy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ModelsHealthy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    GpuUtilizationPercent = table.Column<double>(type: "double", nullable: true),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    DiskUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    NetworkHealthy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodHealthMetrics", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScalingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodPoolId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Direction = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    TriggerType = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PodCountBefore = table.Column<int>(type: "int", nullable: false),
                    PodCountAfter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScalingEvents", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScalingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    MinPods = table.Column<int>(type: "int", nullable: false),
                    MaxPods = table.Column<int>(type: "int", nullable: false),
                    MaxQueueLength = table.Column<int>(type: "int", nullable: false),
                    MaxLatencyMs = table.Column<int>(type: "int", nullable: false),
                    ScaleUpThreshold = table.Column<double>(type: "double", nullable: false),
                    ScaleDownThreshold = table.Column<double>(type: "double", nullable: false),
                    WarmStandbyCount = table.Column<int>(type: "int", nullable: false),
                    MinRuntimeMinutes = table.Column<int>(type: "int", nullable: false),
                    AutoScaleUpEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoScaleDownEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EvaluationIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScalingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScalingPolicies_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodPools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    PoolType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    GpuType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    Region = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TemplateId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ImageName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ScalingPolicyId = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodPools_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PodPools_ScalingPolicies_ScalingPolicyId",
                        column: x => x.ScalingPolicyId,
                        principalTable: "ScalingPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodPoolMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodPoolId = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    IsWarmStandby = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastHealthCheckAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DrainingStartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActiveStreams = table.Column<int>(type: "int", nullable: false),
                    AffinityTag = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodPoolMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodPoolMembers_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PodPoolMembers_PodPools_PodPoolId",
                        column: x => x.PodPoolId,
                        principalTable: "PodPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodPoolModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodPoolId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodPoolModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodPoolModels_PodPools_PodPoolId",
                        column: x => x.PodPoolId,
                        principalTable: "PodPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CapacitySnapshots_OrganizationId_PodPoolId_RecordedAt",
                table: "CapacitySnapshots",
                columns: new[] { "OrganizationId", "PodPoolId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CapacitySnapshots_OrganizationId_RecordedAt",
                table: "CapacitySnapshots",
                columns: new[] { "OrganizationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoadBalancerConfigs_OrganizationId",
                table: "LoadBalancerConfigs",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodHealthMetrics_GpuPodId_RecordedAt",
                table: "PodHealthMetrics",
                columns: new[] { "GpuPodId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PodHealthMetrics_OrganizationId_GpuPodId_RecordedAt",
                table: "PodHealthMetrics",
                columns: new[] { "OrganizationId", "GpuPodId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PodPoolMembers_GpuPodId",
                table: "PodPoolMembers",
                column: "GpuPodId");

            migrationBuilder.CreateIndex(
                name: "IX_PodPoolMembers_PodPoolId_GpuPodId",
                table: "PodPoolMembers",
                columns: new[] { "PodPoolId", "GpuPodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodPoolMembers_PodPoolId_State",
                table: "PodPoolMembers",
                columns: new[] { "PodPoolId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_PodPoolModels_PodPoolId_ModelName",
                table: "PodPoolModels",
                columns: new[] { "PodPoolId", "ModelName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodPools_OrganizationId_IsActive",
                table: "PodPools",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PodPools_OrganizationId_IsDefault",
                table: "PodPools",
                columns: new[] { "OrganizationId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_PodPools_OrganizationId_Name",
                table: "PodPools",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodPools_ScalingPolicyId",
                table: "PodPools",
                column: "ScalingPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScalingEvents_OrganizationId_OccurredAt",
                table: "ScalingEvents",
                columns: new[] { "OrganizationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScalingEvents_PodPoolId_OccurredAt",
                table: "ScalingEvents",
                columns: new[] { "PodPoolId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScalingPolicies_OrganizationId_Name",
                table: "ScalingPolicies",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapacitySnapshots");

            migrationBuilder.DropTable(
                name: "LoadBalancerConfigs");

            migrationBuilder.DropTable(
                name: "PodHealthMetrics");

            migrationBuilder.DropTable(
                name: "PodPoolMembers");

            migrationBuilder.DropTable(
                name: "PodPoolModels");

            migrationBuilder.DropTable(
                name: "ScalingEvents");

            migrationBuilder.DropTable(
                name: "PodPools");

            migrationBuilder.DropTable(
                name: "ScalingPolicies");
        }
    }
}
