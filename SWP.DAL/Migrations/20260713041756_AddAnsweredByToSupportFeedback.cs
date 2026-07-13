using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAnsweredByToSupportFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnsweredByUserId",
                table: "SupportFeedbacks",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportFeedbacks_AnsweredByUserId",
                table: "SupportFeedbacks",
                column: "AnsweredByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportFeedbacks_AnsweredBy",
                table: "SupportFeedbacks",
                column: "AnsweredByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportFeedbacks_AnsweredBy",
                table: "SupportFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_SupportFeedbacks_AnsweredByUserId",
                table: "SupportFeedbacks");

            migrationBuilder.DropColumn(
                name: "AnsweredByUserId",
                table: "SupportFeedbacks");
        }
    }
}
