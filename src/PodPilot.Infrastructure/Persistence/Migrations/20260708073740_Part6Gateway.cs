using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part6Gateway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GatewayApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    KeyType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    KeyPrefix = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    KeyHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: false),
                    RateLimitPerDay = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewayApiKeys_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GatewayRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewayRoutes_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GatewayRoutes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GatewayRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApiKeyId = table.Column<Guid>(type: "char(36)", nullable: true),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    HttpMethod = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    Path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Model = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    WakeTriggered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsStreaming = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    UpstreamBaseUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewayRequests_GatewayApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "GatewayApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GatewayRequests_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GatewayErrors",
                columns: table => new
                {
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ErrorFormat = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    InternalDetails = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayErrors", x => x.GatewayRequestId);
                    table.ForeignKey(
                        name: "FK_GatewayErrors_GatewayRequests_GatewayRequestId",
                        column: x => x.GatewayRequestId,
                        principalTable: "GatewayRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GatewayLatencies",
                columns: table => new
                {
                    GatewayRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    WakeLatencyMs = table.Column<int>(type: "int", nullable: true),
                    HealthCheckLatencyMs = table.Column<int>(type: "int", nullable: true),
                    ForwardLatencyMs = table.Column<int>(type: "int", nullable: true),
                    TotalLatencyMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayLatencies", x => x.GatewayRequestId);
                    table.ForeignKey(
                        name: "FK_GatewayLatencies_GatewayRequests_GatewayRequestId",
                        column: x => x.GatewayRequestId,
                        principalTable: "GatewayRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayApiKeys_KeyHash",
                table: "GatewayApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GatewayApiKeys_OrganizationId_KeyPrefix",
                table: "GatewayApiKeys",
                columns: new[] { "OrganizationId", "KeyPrefix" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_ApiKeyId",
                table: "GatewayRequests",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_GpuPodId",
                table: "GatewayRequests",
                column: "GpuPodId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRequests_OrganizationId_StartedAt",
                table: "GatewayRequests",
                columns: new[] { "OrganizationId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRoutes_GpuPodId",
                table: "GatewayRoutes",
                column: "GpuPodId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRoutes_OrganizationId_IsDefault",
                table: "GatewayRoutes",
                columns: new[] { "OrganizationId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayRoutes_OrganizationId_ModelName",
                table: "GatewayRoutes",
                columns: new[] { "OrganizationId", "ModelName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GatewayErrors");

            migrationBuilder.DropTable(
                name: "GatewayLatencies");

            migrationBuilder.DropTable(
                name: "GatewayRoutes");

            migrationBuilder.DropTable(
                name: "GatewayRequests");

            migrationBuilder.DropTable(
                name: "GatewayApiKeys");
        }
    }
}
