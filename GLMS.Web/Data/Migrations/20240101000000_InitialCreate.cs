using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.Web.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Clients",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 150, nullable: false),
                ContactDetails = table.Column<string>(maxLength: 200, nullable: false),
                Region = table.Column<string>(maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Clients", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Contracts",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ClientId = table.Column<int>(nullable: false),
                StartDate = table.Column<DateTime>(nullable: false),
                EndDate = table.Column<DateTime>(nullable: false),
                Status = table.Column<string>(nullable: false, defaultValue: "Draft"),
                ServiceLevel = table.Column<string>(maxLength: 100, nullable: false),
                SignedAgreementPath = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Contracts", x => x.Id);
                table.ForeignKey("FK_Contracts_Clients_ClientId", x => x.ClientId,
                    principalTable: "Clients", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ServiceRequests",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ContractId = table.Column<int>(nullable: false),
                Description = table.Column<string>(maxLength: 500, nullable: false),
                CostUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CostZar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Status = table.Column<string>(nullable: false, defaultValue: "Pending"),
                CreatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                table.ForeignKey("FK_ServiceRequests_Contracts_ContractId", x => x.ContractId,
                    principalTable: "Contracts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Contracts_ClientId", "Contracts", "ClientId");
        migrationBuilder.CreateIndex("IX_ServiceRequests_ContractId", "ServiceRequests", "ContractId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ServiceRequests");
        migrationBuilder.DropTable("Contracts");
        migrationBuilder.DropTable("Clients");
    }
}
