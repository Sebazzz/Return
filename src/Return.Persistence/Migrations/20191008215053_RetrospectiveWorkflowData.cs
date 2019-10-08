using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Return.Persistence.Migrations
{
    public partial class RetrospectiveWorkflowData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WorkflowData_CurrentWorkflowInitiationTimestamp",
                table: "Retrospective",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowData_CurrentWorkflowTimeLimitInMinutes",
                table: "Retrospective",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowData_CurrentWorkflowInitiationTimestamp",
                table: "Retrospective");

            migrationBuilder.DropColumn(
                name: "WorkflowData_CurrentWorkflowTimeLimitInMinutes",
                table: "Retrospective");
        }
    }
}
