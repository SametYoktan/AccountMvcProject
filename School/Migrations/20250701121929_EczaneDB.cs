using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace School.Migrations
{
    /// <inheritdoc />
    public partial class EczaneDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_NewRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_NewUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LoginErrorNumber = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_NewAccountConfirmationHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActiveId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewAccountConfirmationHistory", x => x.ID);
                    table.ForeignKey(
                        name: "FK__NewAccountConfirmationHistory__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewEmailHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MailType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewEmailHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__NewEmailHistory__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewLoginHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoginId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewLoginHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__NewLoginHistory__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewPasswordHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewPasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__NewPasswordHistory__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewUserActivityLog",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewUserActivityLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK__NewUserActivityLog__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewUserIsActiveHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActiveId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewUserIsActiveHistory", x => x.ID);
                    table.ForeignKey(
                        name: "FK__NewUserIsActiveHistory__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_NewUserRoles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NewUserRoles", x => x.ID);
                    table.ForeignKey(
                        name: "FK__NewUserRoles__NewRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "_NewRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__NewUserRoles__NewUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "_NewUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__NewAccountConfirmationHistory_UserID",
                table: "_NewAccountConfirmationHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewEmailHistory_UserID",
                table: "_NewEmailHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewLoginHistory_UserID",
                table: "_NewLoginHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewPasswordHistory_UserID",
                table: "_NewPasswordHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewUserActivityLog_UserID",
                table: "_NewUserActivityLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewUserIsActiveHistory_UserID",
                table: "_NewUserIsActiveHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX__NewUserRoles_RoleID",
                table: "_NewUserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX__NewUserRoles_UserID",
                table: "_NewUserRoles",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_NewAccountConfirmationHistory");

            migrationBuilder.DropTable(
                name: "_NewEmailHistory");

            migrationBuilder.DropTable(
                name: "_NewLoginHistory");

            migrationBuilder.DropTable(
                name: "_NewPasswordHistory");

            migrationBuilder.DropTable(
                name: "_NewUserActivityLog");

            migrationBuilder.DropTable(
                name: "_NewUserIsActiveHistory");

            migrationBuilder.DropTable(
                name: "_NewUserRoles");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "_NewRoles");

            migrationBuilder.DropTable(
                name: "_NewUsers");
        }
    }
}
