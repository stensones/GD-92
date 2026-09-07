using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Router.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnsureSingleRouterManagedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_managed_entity_Kind",
                schema: "node",
                table: "managed_entity",
                column: "Kind",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_managed_entity_Kind",
                schema: "node",
                table: "managed_entity");
        }
    }
}
