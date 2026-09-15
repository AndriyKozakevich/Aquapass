using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AquaPass.Migrations
{
    /// <inheritdoc />
    public partial class newMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerFirstName = table.Column<string>(type: "text", nullable: false),
                    CustomerLastName = table.Column<string>(type: "text", nullable: false),
                    CustomerEmail = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sunbeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Row = table.Column<string>(type: "text", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sunbeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sunbeds_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DayType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tariffs_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntranceTariffId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntrancePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TicketCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SunbedId = table.Column<Guid>(type: "uuid", nullable: true),
                    SunbedPrice = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Sunbeds_SunbedId",
                        column: x => x.SunbedId,
                        principalTable: "Sunbeds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Tariffs_EntranceTariffId",
                        column: x => x.EntranceTariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Zones",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "VIP-зона" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Стандартна зона" }
                });

            migrationBuilder.InsertData(
                table: "Tariffs",
                columns: new[] { "Id", "DayType", "Name", "Price", "ServiceType", "ZoneId" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "Weekend", "Дорослий вхідний квиток(Вихідний)", 750.00m, "EntranceTicketAdult", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "Weekend", "Дитячий вхідний квиток(Вихідний)", 400.00m, "EntranceTicketChild", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "Weekday", "Дорослий вхідний квиток(Будний)", 550.00m, "EntranceTicketAdult", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), "Weekday", "Дитячий вхідний квиток(Будний)", 350.00m, "EntranceTicketChild", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), "Weekday", "Бунгало(Будний)", 600.00m, "Bungalow", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a0000000-0000-0000-0000-000000000006"), "Weekend", "Бунгало(Вихідний)", 800.00m, "Bungalow", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a0000000-0000-0000-0000-000000000007"), "Weekend", "Лежак(Вихідний)", 200.00m, "Sunbed", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a0000000-0000-0000-0000-000000000008"), "Weekday", "Лежак(Будний)", 150.00m, "Sunbed", new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sunbeds_ZoneId",
                table: "Sunbeds",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_ZoneId",
                table: "Tariffs",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EntranceTariffId",
                table: "Tickets",
                column: "EntranceTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrderId",
                table: "Tickets",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SunbedId",
                table: "Tickets",
                column: "SunbedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Sunbeds");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DropTable(
                name: "Zones");
        }
    }
}
