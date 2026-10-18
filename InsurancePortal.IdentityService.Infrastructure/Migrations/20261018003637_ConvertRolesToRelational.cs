using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertRolesToRelational : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Service = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Code",
                table: "Permission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Role] ON;
INSERT INTO [Role] (Id, Name, Type, Description, IsActive) VALUES
(1, N'Guest', N'External', NULL, 1),
(2, N'Registered', N'External', NULL, 1),
(3, N'Admin', N'Internal', NULL, 1),
(4, N'Associates', N'Internal', NULL, 1),
(5, N'Agents', N'Internal', NULL, 1),
(6, N'Providers Admin', N'External', NULL, 1),
(7, N'Providers Staff', N'External', NULL, 1);
SET IDENTITY_INSERT [Role] OFF;");

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Permission] ON;
INSERT INTO [Permission] (Id, Code, Service, Resource, Action, Scope, Description) VALUES
(1, N'user.profile.read.own', N'Identity', N'user.profile', N'read', N'own', NULL),
(2, N'user.profile.update.own', N'Identity', N'user.profile', N'update', N'own', NULL),
(3, N'user.convert.own', N'Identity', N'user', N'convert', N'own', NULL),
(4, N'user.read.tenant', N'Identity', N'user', N'read', N'tenant', NULL),
(5, N'user.read.any', N'Identity', N'user', N'read', N'any', NULL),
(6, N'user.create.tenant', N'Identity', N'user', N'create', N'tenant', NULL),
(7, N'user.disable.tenant', N'Identity', N'user', N'disable', N'tenant', NULL),
(8, N'user.role.assign.tenant', N'Identity', N'user.role', N'assign', N'tenant', NULL),
(9, N'user.role.assign.any', N'Identity', N'user.role', N'assign', N'any', NULL),
(10, N'user.org.manage.organization', N'Identity', N'user.org', N'manage', N'organization', NULL),
(11, N'quote.create.guest', N'Quote', N'quote', N'create', N'guest', NULL),
(12, N'quote.read.own', N'Quote', N'quote', N'read', N'own', NULL),
(13, N'quote.create.own', N'Quote', N'quote', N'create', N'own', NULL),
(14, N'quote.update.own', N'Quote', N'quote', N'update', N'own', NULL),
(15, N'quote.create.assigned', N'Quote', N'quote', N'create', N'assigned', NULL),
(16, N'quote.update.assigned', N'Quote', N'quote', N'update', N'assigned', NULL),
(17, N'quote.read.assigned', N'Quote', N'quote', N'read', N'assigned', NULL),
(18, N'quote.read.tenant', N'Quote', N'quote', N'read', N'tenant', NULL),
(19, N'quote.approve.tenant', N'Quote', N'quote', N'approve', N'tenant', NULL),
(20, N'quote.delete.tenant', N'Quote', N'quote', N'delete', N'tenant', NULL),
(21, N'payment.read.own', N'Payment', N'payment', N'read', N'own', NULL),
(22, N'payment.create.own', N'Payment', N'payment', N'create', N'own', NULL),
(23, N'payment.read.assigned', N'Payment', N'payment', N'read', N'assigned', NULL),
(24, N'payment.read.tenant', N'Payment', N'payment', N'read', N'tenant', NULL),
(25, N'payment.refund.tenant', N'Payment', N'payment', N'refund', N'tenant', NULL),
(26, N'payment.reconcile.tenant', N'Payment', N'payment', N'reconcile', N'tenant', NULL),
(27, N'payment.read.organization', N'Payment', N'payment', N'read', N'organization', NULL),
(28, N'policy.read.own', N'Policy', N'policy', N'read', N'own', NULL),
(29, N'policy.issue.assigned', N'Policy', N'policy', N'issue', N'assigned', NULL),
(30, N'policy.read.assigned', N'Policy', N'policy', N'read', N'assigned', NULL),
(31, N'policy.update.assigned', N'Policy', N'policy', N'update', N'assigned', NULL),
(32, N'policy.renew.assigned', N'Policy', N'policy', N'renew', N'assigned', NULL),
(33, N'policy.read.tenant', N'Policy', N'policy', N'read', N'tenant', NULL),
(34, N'policy.issue.tenant', N'Policy', N'policy', N'issue', N'tenant', NULL),
(35, N'policy.cancel.tenant', N'Policy', N'policy', N'cancel', N'tenant', NULL),
(36, N'claim.submit.own', N'Claim', N'claim', N'submit', N'own', NULL),
(37, N'claim.read.own', N'Claim', N'claim', N'read', N'own', NULL),
(38, N'claim.document.upload.own', N'Claim', N'claim.document', N'upload', N'own', NULL),
(39, N'claim.read.assigned', N'Claim', N'claim', N'read', N'assigned', NULL),
(40, N'claim.read.tenant', N'Claim', N'claim', N'read', N'tenant', NULL),
(41, N'claim.review.tenant', N'Claim', N'claim', N'review', N'tenant', NULL),
(42, N'claim.approve.tenant', N'Claim', N'claim', N'approve', N'tenant', NULL),
(43, N'claim.settle.tenant', N'Claim', N'claim', N'settle', N'tenant', NULL),
(44, N'claim.assign.tenant', N'Claim', N'claim', N'assign', N'tenant', NULL),
(45, N'claim.read.organization', N'Claim', N'claim', N'read', N'organization', NULL),
(46, N'claim.submit.organization', N'Claim', N'claim', N'submit', N'organization', NULL),
(47, N'claim.status.update.organization', N'Claim', N'claim.status', N'update', N'organization', NULL),
(48, N'dashboard.personal.view.own', N'Dashboard', N'dashboard.personal', N'view', N'own', NULL),
(49, N'dashboard.agent.view.assigned', N'Dashboard', N'dashboard.agent', N'view', N'assigned', NULL),
(50, N'dashboard.operations.view.tenant', N'Dashboard', N'dashboard.operations', N'view', N'tenant', NULL),
(51, N'dashboard.provider.view.organization', N'Dashboard', N'dashboard.provider', N'view', N'organization', NULL),
(52, N'dashboard.admin.view.any', N'Dashboard', N'dashboard.admin', N'view', N'any', NULL);
SET IDENTITY_INSERT [Permission] OFF;");

            migrationBuilder.Sql(@"
INSERT INTO [RolePermission] (RoleId, PermissionId) VALUES
(1,1),(1,2),(1,3),(1,11),(1,12),(1,14),(1,48),
(2,1),(2,2),(2,12),(2,13),(2,14),(2,21),(2,22),(2,28),(2,36),(2,37),(2,38),(2,48),
(3,1),(3,2),(3,4),(3,5),(3,6),(3,7),(3,8),(3,9),(3,18),(3,20),(3,24),(3,26),(3,35),(3,40),(3,44),(3,48),(3,49),(3,50),(3,52),
(4,1),(4,2),(4,4),(4,18),(4,19),(4,20),(4,24),(4,25),(4,33),(4,34),(4,35),(4,39),(4,40),(4,41),(4,42),(4,43),(4,44),(4,48),(4,50),
(5,1),(5,2),(5,15),(5,16),(5,17),(5,23),(5,29),(5,30),(5,31),(5,32),(5,48),(5,49),
(6,1),(6,2),(6,10),(6,27),(6,45),(6,46),(6,47),(6,48),(6,51),
(7,1),(7,2),(7,27),(7,45),(7,46),(7,47),(7,48),(7,51);");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "UserRole",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ur SET ur.RoleId = r.Id
FROM [UserRole] ur
INNER JOIN [Role] r ON r.Name = ur.RoleName;

IF EXISTS (SELECT 1 FROM [UserRole] WHERE [RoleId] IS NULL)
    THROW 50000, 'UserRole contains RoleName values that have no matching Role row.', 1;");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "UserRole",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_RoleName",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "UserRole");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role_RoleId",
                table: "UserRole",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role_RoleId",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "UserRole",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ur SET ur.RoleName = r.Name
FROM [UserRole] ur
INNER JOIN [Role] r ON r.Id = ur.RoleId;");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "UserRole",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserRole");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                columns: new[] { "UserId", "RoleName" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleName",
                table: "UserRole",
                column: "RoleName");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
