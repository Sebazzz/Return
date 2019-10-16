using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Return.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteLane",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteLane", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PredefinedParticipantColor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Color_R = table.Column<byte>(nullable: true),
                    Color_G = table.Column<byte>(nullable: true),
                    Color_B = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedParticipantColor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Retrospective",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlId_StringId = table.Column<string>(unicode: false, maxLength: 32, nullable: true),
                    CurrentStage = table.Column<int>(nullable: false),
                    HashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    FacilitatorHashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    CreationTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Options_MaximumNumberOfVotes = table.Column<int>(nullable: true),
                    WorkflowData_CurrentWorkflowInitiationTimestamp = table.Column<DateTimeOffset>(nullable: true),
                    WorkflowData_CurrentWorkflowTimeLimitInMinutes = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retrospective", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteGroup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetrospectiveId = table.Column<int>(nullable: false),
                    LaneId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteGroup_NoteLane_LaneId",
                        column: x => x.LaneId,
                        principalTable: "NoteLane",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoteGroup_Retrospective_RetrospectiveId",
                        column: x => x.RetrospectiveId,
                        principalTable: "Retrospective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    RetrospectiveId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    IsFacilitator = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participant_Retrospective_RetrospectiveId",
                        column: x => x.RetrospectiveId,
                        principalTable: "Retrospective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ParticipantId = table.Column<int>(nullable: false),
                    CreationTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    GroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_NoteGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "NoteGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Note_NoteLane_LaneId",
                        column: x => x.LaneId,
                        principalTable: "NoteLane",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Note_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Note_Retrospective_RetrospectiveId",
                        column: x => x.RetrospectiveId,
                        principalTable: "Retrospective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NoteVote",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteId = table.Column<int>(nullable: true),
                    NoteGroupId = table.Column<int>(nullable: true),
                    RetrospectiveId = table.Column<int>(nullable: false),
                    ParticipantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteVote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteVote_NoteGroup_NoteGroupId",
                        column: x => x.NoteGroupId,
                        principalTable: "NoteGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoteVote_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoteVote_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoteVote_Retrospective_RetrospectiveId",
                        column: x => x.RetrospectiveId,
                        principalTable: "Retrospective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Note_GroupId",
                table: "Note",
                column: "GroupId");

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
                name: "IX_NoteGroup_LaneId",
                table: "NoteGroup",
                column: "LaneId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteGroup_RetrospectiveId",
                table: "NoteGroup",
                column: "RetrospectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteVote_NoteGroupId",
                table: "NoteVote",
                column: "NoteGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteVote_NoteId",
                table: "NoteVote",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteVote_ParticipantId",
                table: "NoteVote",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteVote_RetrospectiveId",
                table: "NoteVote",
                column: "RetrospectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_RetrospectiveId",
                table: "Participant",
                column: "RetrospectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Retrospective_UrlId_StringId",
                table: "Retrospective",
                column: "UrlId_StringId",
                unique: true,
                filter: "[UrlId_StringId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoteVote");

            migrationBuilder.DropTable(
                name: "PredefinedParticipantColor");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "NoteGroup");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "NoteLane");

            migrationBuilder.DropTable(
                name: "Retrospective");
        }
    }
}
