using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Router.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRouterPasswordVerifierPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "password_verifier",
                schema: "security",
                columns: table => new
                {
                    ParameterSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterNumber = table.Column<byte>(type: "smallint", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    WorkFactor = table.Column<int>(type: "integer", nullable: false),
                    Salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    Hash = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_verifier", x => new { x.ParameterSetId, x.ParameterNumber });
                    table.CheckConstraint("CK_password_verifier_ParameterNumber", "\"ParameterNumber\" = 5");
                    table.ForeignKey(
                        name: "FK_password_verifier_parameter_set_ParameterSetId",
                        column: x => x.ParameterSetId,
                        principalSchema: "node",
                        principalTable: "parameter_set",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_verifier",
                schema: "security");
        }
    }
}
