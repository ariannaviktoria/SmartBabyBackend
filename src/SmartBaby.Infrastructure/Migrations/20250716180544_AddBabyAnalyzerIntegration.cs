using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartBaby.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBabyAnalyzerIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BabyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalItems = table.Column<int>(type: "integer", nullable: false),
                    ProcessedItems = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulItems = table.Column<int>(type: "integer", nullable: false),
                    FailedItems = table.Column<int>(type: "integer", nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchAnalyses_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RealtimeAnalysisSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BabyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
                    UpdateCount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Statistics = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealtimeAnalysisSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealtimeAnalysisSessions_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BabyAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BabyId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultData = table.Column<string>(type: "jsonb", nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: true),
                    PrimaryResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    ProcessingTimeMs = table.Column<float>(type: "real", nullable: true),
                    ModelVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OriginalFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StoredFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileChecksum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BatchAnalysisId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BabyAnalyses_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BabyAnalyses_BatchAnalyses_BatchAnalysisId",
                        column: x => x.BatchAnalysisId,
                        principalTable: "BatchAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RealtimeAnalysisUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdateData = table.Column<string>(type: "jsonb", nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: true),
                    PrimaryResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealtimeAnalysisUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealtimeAnalysisUpdates_RealtimeAnalysisSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "RealtimeAnalysisSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BabyId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisId = table.Column<int>(type: "integer", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    AdditionalData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisAlerts_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisAlerts_BabyAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "BabyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnalysisId = table.Column<int>(type: "integer", nullable: false),
                    TagName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TagValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisTags_BabyAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "BabyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisAlert_BabyId_CreatedAt",
                table: "AnalysisAlerts",
                columns: new[] { "BabyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisAlerts_AnalysisId",
                table: "AnalysisAlerts",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisTags_AnalysisId",
                table: "AnalysisTags",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyAnalyses_BatchAnalysisId",
                table: "BabyAnalyses",
                column: "BatchAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyAnalysis_AnalysisType",
                table: "BabyAnalyses",
                column: "AnalysisType");

            migrationBuilder.CreateIndex(
                name: "IX_BabyAnalysis_BabyId_CreatedAt",
                table: "BabyAnalyses",
                columns: new[] { "BabyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BatchAnalyses_BabyId",
                table: "BatchAnalyses",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeAnalysisSession_SessionId",
                table: "RealtimeAnalysisSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeAnalysisSessions_BabyId",
                table: "RealtimeAnalysisSessions",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeAnalysisUpdates_SessionId",
                table: "RealtimeAnalysisUpdates",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisAlerts");

            migrationBuilder.DropTable(
                name: "AnalysisTags");

            migrationBuilder.DropTable(
                name: "RealtimeAnalysisUpdates");

            migrationBuilder.DropTable(
                name: "BabyAnalyses");

            migrationBuilder.DropTable(
                name: "RealtimeAnalysisSessions");

            migrationBuilder.DropTable(
                name: "BatchAnalyses");
        }
    }
}
