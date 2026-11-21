using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadaFinishing.Web.Data.Migrations;

/// <inheritdoc />
public partial class _20261121145048_AddInquiryAssignments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InquiryAssignments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                InquiryId = table.Column<int>(type: "int", nullable: false),
                ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PreviousCoordinatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                NewCoordinatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InquiryAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_InquiryAssignments_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_InquiryAssignments_AspNetUsers_NewCoordinatorId",
                    column: x => x.NewCoordinatorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_InquiryAssignments_AspNetUsers_PreviousCoordinatorId",
                    column: x => x.PreviousCoordinatorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_InquiryAssignments_Inquiries_InquiryId",
                    column: x => x.InquiryId,
                    principalTable: "Inquiries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InquiryAssignments_ActorId",
            table: "InquiryAssignments",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_InquiryAssignments_InquiryId",
            table: "InquiryAssignments",
            column: "InquiryId");

        migrationBuilder.CreateIndex(
            name: "IX_InquiryAssignments_NewCoordinatorId",
            table: "InquiryAssignments",
            column: "NewCoordinatorId");

        migrationBuilder.CreateIndex(
            name: "IX_InquiryAssignments_PreviousCoordinatorId",
            table: "InquiryAssignments",
            column: "PreviousCoordinatorId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InquiryAssignments");
    }
}
