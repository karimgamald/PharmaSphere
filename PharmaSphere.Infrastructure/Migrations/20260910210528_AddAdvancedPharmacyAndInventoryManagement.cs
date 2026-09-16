using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedPharmacyAndInventoryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteraction_Medicines_InteractingMedicineId",
                table: "DrugInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteraction_Medicines_MedicineId",
                table: "DrugInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_PurchaseOrder_PurchaseOrderId",
                table: "MedicineBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Medicines_MedicineId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrder_Supplier_SupplierId",
                table: "PurchaseOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrder",
                table: "PurchaseOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrugInteraction",
                table: "DrugInteraction");

            migrationBuilder.RenameTable(
                name: "Supplier",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "PurchaseOrder",
                newName: "PurchaseOrders");

            migrationBuilder.RenameTable(
                name: "DrugInteraction",
                newName: "DrugInteractions");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrder_SupplierId",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteraction_MedicineId",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteraction_InteractingMedicineId",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_InteractingMedicineId");

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCostPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ActiveIngredient",
                table: "Medicines",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActiveIngredientConcentration",
                table: "Medicines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Medicines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrders",
                table: "PurchaseOrders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrugInteractions",
                table: "DrugInteractions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SalesReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MedicineBatchId = table.Column<int>(type: "int", nullable: false),
                    ReturnedUnits = table.Column<int>(type: "int", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturns_MedicineBatches_MedicineBatchId",
                        column: x => x.MedicineBatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    SessionOrUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReservedUnits = table.Column<int>(type: "int", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReleasedOrCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReservations_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MedicineBatchId",
                table: "OrderItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_ActiveIngredient",
                table: "Medicines",
                column: "ActiveIngredient");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_BatchNumber",
                table: "MedicineBatches",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_MedicineBatchId",
                table: "SalesReturns",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_OrderId",
                table: "SalesReturns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ExpirationTime",
                table: "StockReservations",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_MedicineId",
                table: "StockReservations",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Medicines_InteractingMedicineId",
                table: "DrugInteractions",
                column: "InteractingMedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Medicines_MedicineId",
                table: "DrugInteractions",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_PurchaseOrders_PurchaseOrderId",
                table: "MedicineBatches",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_MedicineBatches_MedicineBatchId",
                table: "OrderItems",
                column: "MedicineBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Medicines_MedicineId",
                table: "OrderItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Medicines_InteractingMedicineId",
                table: "DrugInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Medicines_MedicineId",
                table: "DrugInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_PurchaseOrders_PurchaseOrderId",
                table: "MedicineBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_MedicineBatches_MedicineBatchId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Medicines_MedicineId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "SalesReturns");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_MedicineBatchId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_ActiveIngredient",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_BatchNumber",
                table: "MedicineBatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrders",
                table: "PurchaseOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrugInteractions",
                table: "DrugInteractions");

            migrationBuilder.DropColumn(
                name: "MedicineBatchId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitCostPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ActiveIngredient",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ActiveIngredientConcentration",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Medicines");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Supplier");

            migrationBuilder.RenameTable(
                name: "PurchaseOrders",
                newName: "PurchaseOrder");

            migrationBuilder.RenameTable(
                name: "DrugInteractions",
                newName: "DrugInteraction");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrder",
                newName: "IX_PurchaseOrder_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_MedicineId",
                table: "DrugInteraction",
                newName: "IX_DrugInteraction_MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_InteractingMedicineId",
                table: "DrugInteraction",
                newName: "IX_DrugInteraction_InteractingMedicineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supplier",
                table: "Supplier",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrder",
                table: "PurchaseOrder",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrugInteraction",
                table: "DrugInteraction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteraction_Medicines_InteractingMedicineId",
                table: "DrugInteraction",
                column: "InteractingMedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteraction_Medicines_MedicineId",
                table: "DrugInteraction",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_PurchaseOrder_PurchaseOrderId",
                table: "MedicineBatches",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrder",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Medicines_MedicineId",
                table: "OrderItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrder_Supplier_SupplierId",
                table: "PurchaseOrder",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
