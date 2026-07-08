using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part9SmartRequestScheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GatewayRequests_OrganizationId_StartedAt",
                table: "GatewayRequests");

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "GatewayRequests",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GatewayRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ExecutionTimeMs",
                table: "GatewayRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "GatewayRequests",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "GatewayRequests",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "QueueTimeMs",
                table: "GatewayRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestBodyHash",
                table: "GatewayRequests",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "GatewayRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "GatewayRequests",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestExecutions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    QueueName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    EnqueuedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ClientRequestId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestQueue", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SchedulerEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    EventType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Metadata = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerEvents", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_OrganizationId_CreatedAt",
                table: "GatewayRequests",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_OrganizationId_Status",
                table: "GatewayRequests",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestExecutions_GatewayRequestId_AttemptNumber",
                table: "RequestExecutions",
                columns: new[] { "GatewayRequestId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestExecutions_GpuPodId",
                table: "RequestExecutions",
                column: "GpuPodId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestQueue_GatewayRequestId",
                table: "RequestQueue",
                column: "GatewayRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestQueue_OrganizationId_IsActive_EnqueuedAt",
                table: "RequestQueue",
                columns: new[] { "OrganizationId", "IsActive", "EnqueuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerEvents_GatewayRequestId",
                table: "SchedulerEvents",
                column: "GatewayRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerEvents_OrganizationId_Timestamp",
                table: "SchedulerEvents",
                columns: new[] { "OrganizationId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestExecutions");

            migrationBuilder.DropTable(
                name: "RequestQueue");

            migrationBuilder.DropTable(
                name: "SchedulerEvents");

            migrationBuilder.DropIndex(
                name: "IX_GatewayRequests_OrganizationId_CreatedAt",
                table: "GatewayRequests");

            migrationBuilder.DropIndex(
                name: "IX_GatewayRequests_OrganizationId_Status",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "ExecutionTimeMs",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "QueueTimeMs",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "RequestBodyHash",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "GatewayRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GatewayRequests");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_OrganizationId_StartedAt",
                table: "GatewayRequests",
                columns: new[] { "OrganizationId", "StartedAt" });
        }
    }
}
