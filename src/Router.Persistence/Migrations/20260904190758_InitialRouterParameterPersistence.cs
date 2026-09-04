using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Router.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRouterParameterPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "node");

            migrationBuilder.CreateTable(
                name: "communications_node",
                schema: "node",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communications_node", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "managed_entity",
                schema: "node",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managed_entity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_managed_entity_communications_node_NodeId",
                        column: x => x.NodeId,
                        principalSchema: "node",
                        principalTable: "communications_node",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_set",
                schema: "node",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<byte>(type: "smallint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_set", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_set_managed_entity_ManagedEntityId",
                        column: x => x.ManagedEntityId,
                        principalSchema: "node",
                        principalTable: "managed_entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_value",
                schema: "node",
                columns: table => new
                {
                    ParameterSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterNumber = table.Column<byte>(type: "smallint", nullable: false),
                    EncodedValue = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_value", x => new { x.ParameterSetId, x.ParameterNumber });
                    table.ForeignKey(
                        name: "FK_parameter_value_parameter_set_ParameterSetId",
                        column: x => x.ParameterSetId,
                        principalSchema: "node",
                        principalTable: "parameter_set",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_managed_entity_NodeId_Kind",
                schema: "node",
                table: "managed_entity",
                columns: new[] { "NodeId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parameter_set_ManagedEntityId_Kind",
                schema: "node",
                table: "parameter_set",
                columns: new[] { "ManagedEntityId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_value",
                schema: "node");

            migrationBuilder.DropTable(
                name: "parameter_set",
                schema: "node");

            migrationBuilder.DropTable(
                name: "managed_entity",
                schema: "node");

            migrationBuilder.DropTable(
                name: "communications_node",
                schema: "node");
        }
    }
}
