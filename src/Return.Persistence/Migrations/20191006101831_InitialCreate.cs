using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Return.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteLanes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteLanes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color_R = table.Column<byte>(nullable: true),
                    Color_G = table.Column<byte>(nullable: true),
                    Color_B = table.Column<byte>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Retrospectives",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlId_StringId = table.Column<string>(unicode: false, maxLength: 32, nullable: true),
                    HashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    ManagerHashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    CreationTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retrospectives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetrospectiveId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(maxLength: 2048, nullable: false),
                    LaneId = table.Column<int>(nullable: false),
                    ParticipantId = table.Column<int>(nullable: true),
                    CreationTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_NoteLanes_LaneId",
                        column: x => x.LaneId,
                        principalTable: "NoteLanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Note_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Note_Retrospectives_RetrospectiveId",
                        column: x => x.RetrospectiveId,
                        principalTable: "Retrospectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Note_LaneId",
                table: "Note",
                column: "LaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_ParticipantId",
                table: "Note",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_RetrospectiveId",
                table: "Note",
                column: "RetrospectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Retrospectives_UrlId_StringId",
                table: "Retrospectives",
                column: "UrlId_StringId",
                unique: true,
                filter: "[UrlId_StringId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "NoteLanes");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Retrospectives");
        }
    }
}
