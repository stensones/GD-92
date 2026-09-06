using Microsoft.EntityFrameworkCore.Migrations;

namespace NodeManager.Persistence.Migrations;

public partial class InitialNodeManagerParameterPersistence : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.EnsureSchema(name: "node");

		migrationBuilder.CreateTable(
			name: "parameter_set",
			schema: "node",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				Kind = table.Column<byte>(type: "smallint", nullable: false),
				Revision = table.Column<long>(type: "bigint", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_parameter_set", parameterSet => parameterSet.Id);
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
				table.PrimaryKey(
					"PK_parameter_value",
					parameterValue => new
					{
						parameterValue.ParameterSetId,
						parameterValue.ParameterNumber
					});
				table.ForeignKey(
					name: "FK_parameter_value_parameter_set_ParameterSetId",
					column: parameterValue => parameterValue.ParameterSetId,
					principalSchema: "node",
					principalTable: "parameter_set",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_parameter_set_Kind",
			schema: "node",
			table: "parameter_set",
			column: "Kind",
			unique: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(name: "parameter_value", schema: "node");
		migrationBuilder.DropTable(name: "parameter_set", schema: "node");
	}
}
