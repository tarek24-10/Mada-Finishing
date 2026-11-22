using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadaFinishing.Web.Data.Migrations;

/// <inheritdoc />
public partial class _20261121212829_AddInquiryFollowUps : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InquiryFollowUps",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                InquiryId = table.Column<int>(type: "int", nullable: false),
                ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                PreviousStatus = table.Column<int>(type: "int", nullable: true),
                NewStatus = table.Column<int>(type: "int", nullable: true),
                Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InquiryFollowUps", x => x.Id);
                table.ForeignKey(
                    name: "FK_InquiryFollowUps_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_InquiryFollowUps_Inquiries_InquiryId",
                    column: x => x.InquiryId,
                    principalTable: "Inquiries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InquiryFollowUps_ActorId",
            table: "InquiryFollowUps",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_InquiryFollowUps_InquiryId",
            table: "InquiryFollowUps",
            column: "InquiryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InquiryFollowUps");
    }
}
