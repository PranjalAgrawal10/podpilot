using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part5Lifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "GpuPods",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PodActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ActivityType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Metadata = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodActivities_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodIdlePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IdleTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false),
                    AutoShutdownEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoWakeEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MinimumRunningTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    IdleDetectedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodIdlePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodIdlePolicies_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodLifecycleEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    EventType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodLifecycleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodLifecycleEvents_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodLifecycleLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Operation = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    OwnerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodLifecycleLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodLifecycleLocks_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodWakeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodWakeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodWakeRequests_GpuPods_PodId",
                        column: x => x.PodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PodActivities_PodId_Timestamp",
                table: "PodActivities",
                columns: new[] { "PodId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PodIdlePolicies_PodId",
                table: "PodIdlePolicies",
                column: "PodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodLifecycleEvents_PodId_Timestamp",
                table: "PodLifecycleEvents",
                columns: new[] { "PodId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PodLifecycleLocks_PodId_Operation",
                table: "PodLifecycleLocks",
                columns: new[] { "PodId", "Operation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodWakeRequests_PodId_Status",
                table: "PodWakeRequests",
                columns: new[] { "PodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PodWakeRequests_RequestedAt",
                table: "PodWakeRequests",
                column: "RequestedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodActivities");

            migrationBuilder.DropTable(
                name: "PodIdlePolicies");

            migrationBuilder.DropTable(
                name: "PodLifecycleEvents");

            migrationBuilder.DropTable(
                name: "PodLifecycleLocks");

            migrationBuilder.DropTable(
                name: "PodWakeRequests");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "GpuPods");
        }
    }
}
