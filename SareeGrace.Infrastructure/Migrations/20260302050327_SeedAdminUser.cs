using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SareeGrace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "EmailVerified", "FirstName", "IsActive", "LastName", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@sareegrace.com", true, "Super", true, "Admin", "$2a$11$TqJnJiIjAJdVaD3IXkQ4veqoIxOJHqQXgJDxGjIgZdl.JjFtfKMKy", "9999999999", "Admin", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
        }
    }
}
