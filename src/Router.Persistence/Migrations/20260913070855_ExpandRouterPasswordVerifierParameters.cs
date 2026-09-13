using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Router.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandRouterPasswordVerifierParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_password_verifier_ParameterNumber",
                schema: "security",
                table: "password_verifier");

            migrationBuilder.AddCheckConstraint(
                name: "CK_password_verifier_ParameterNumber",
                schema: "security",
                table: "password_verifier",
                sql: "\"ParameterNumber\" BETWEEN 5 AND 8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_password_verifier_ParameterNumber",
                schema: "security",
                table: "password_verifier");

            migrationBuilder.AddCheckConstraint(
                name: "CK_password_verifier_ParameterNumber",
                schema: "security",
                table: "password_verifier",
                sql: "\"ParameterNumber\" = 5");
        }
    }
}
