using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesGuestAndProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserIdentity",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuest",
                table: "UserIdentity",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserIdentity",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "UserIdentity",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserIdentity",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleName });
                    table.ForeignKey(
                        name: "FK_UserRole_UserIdentity_UserId",
                        column: x => x.UserId,
                        principalTable: "UserIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleName",
                table: "UserRole",
                column: "RoleName");

            migrationBuilder.Sql(
                """
                INSERT INTO UserRole (UserId, RoleName)
                SELECT u.Id, 'Registered'
                FROM UserIdentity u
                WHERE u.IsGuest = 0
                  AND NOT EXISTS (SELECT 1 FROM UserRole r WHERE r.UserId = u.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserIdentity");

            migrationBuilder.DropColumn(
                name: "IsGuest",
                table: "UserIdentity");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserIdentity");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserIdentity");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserIdentity");
        }
    }
}
