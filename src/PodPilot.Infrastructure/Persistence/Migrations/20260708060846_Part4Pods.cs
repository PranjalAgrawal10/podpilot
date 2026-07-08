using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PodPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Part4Pods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GpuPods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProviderPodId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    GpuType = table.Column<int>(type: "int", nullable: false),
                    GpuId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Region = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    TemplateId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ImageName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ContainerDisk = table.Column<int>(type: "int", nullable: false),
                    VolumeDisk = table.Column<int>(type: "int", nullable: false),
                    PublicIp = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Endpoint = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HourlyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LastStartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastStoppedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpuPods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpuPods_ComputeProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "ComputeProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GpuPods_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TemplateId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TemplateName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ImageName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ContainerDiskGb = table.Column<int>(type: "int", nullable: false),
                    VolumeDiskGb = table.Column<int>(type: "int", nullable: false),
                    VolumeMountPath = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    GpuCount = table.Column<int>(type: "int", nullable: false),
                    EnvironmentVariablesJson = table.Column<string>(type: "varchar(8000)", maxLength: 8000, nullable: true),
                    PortsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    EnablePublicIp = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodConfigurations_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodEndpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Protocol = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    PublicPort = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodEndpoints_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PodStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    GpuPodId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodStatusHistory_GpuPods_GpuPodId",
                        column: x => x.GpuPodId,
                        principalTable: "GpuPods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPods_OrganizationId_Name",
                table: "GpuPods",
                columns: new[] { "OrganizationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_GpuPods_ProviderId",
                table: "GpuPods",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPods_Status",
                table: "GpuPods",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PodConfigurations_GpuPodId",
                table: "PodConfigurations",
                column: "GpuPodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodEndpoints_GpuPodId_Port_Protocol",
                table: "PodEndpoints",
                columns: new[] { "GpuPodId", "Port", "Protocol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodStatusHistory_GpuPodId_RecordedAt",
                table: "PodStatusHistory",
                columns: new[] { "GpuPodId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodConfigurations");

            migrationBuilder.DropTable(
                name: "PodEndpoints");

            migrationBuilder.DropTable(
                name: "PodStatusHistory");

            migrationBuilder.DropTable(
                name: "GpuPods");
        }
    }
}
