using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SWP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "AvatarUrl", "Bio", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PasswordResetTokenExpiry", "PasswordResetTokenHash", "Phone", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { "AD100001", null, null, null, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin@gmail.com", "System Admin", true, "admin123", null, null, null, "admin", new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "GV100001", null, null, null, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc), "lecturer@gmail.com", "Nguyễn Văn Giảng", true, "lecturer123", null, null, null, "lecturer", new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "SE100001", null, null, null, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc), "student@gmail.com", "Nguyễn Văn Học", true, "student123", null, null, null, "student", new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "AD100001");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "GV100001");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "SE100001");
        }
    }
}
