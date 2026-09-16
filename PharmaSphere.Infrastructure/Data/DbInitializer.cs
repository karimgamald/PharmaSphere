using Microsoft.EntityFrameworkCore;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;

namespace PharmaSphere.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. Apply pending migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            // Prevent duplicate seed data
            if (await context.Medicines.AnyAsync())
            {
                return;
            }

            // =========================================================
            // 2. Suppliers
            // =========================================================

            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "المتحدة للصيدليات (United Pharma)",
                    Phone = "01012345678",
                    Email = "info@unitedpharma.com",
                    Address = "Cairo, Egypt",
                    IsActive = true
                },

                new Supplier
                {
                    Name = "ابن سينا فارما (Ibnsina Pharma)",
                    Phone = "01123456789",
                    Email = "info@ibnsina-pharma.com",
                    Address = "Giza, Egypt",
                    IsActive = true
                }
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();


            // =========================================================
            // 3. Categories
            // =========================================================

            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Analgesics & Cold",
                    Description = "مسكنات الآلام وأدوية البرد والإنفلونزا",
                    IsActive = true
                },

                new Category
                {
                    Name = "Antibiotics",
                    Description = "المضادات الحيوية ومضادات الالتهابات",
                    IsActive = true
                },

                new Category
                {
                    Name = "Vitamins & Supplements",
                    Description = "الفيتامينات والمكملات الغذائية",
                    IsActive = true
                },

                new Category
                {
                    Name = "Medical Devices",
                    Description = "الأجهزة والمستلزمات الطبية",
                    IsActive = true
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();


            // =========================================================
            // 4. Medicines
            // =========================================================

            var panadol = new Medicine
            {
                Name = "Panadol Extra - بنادول إكسترا",
                ScientificName = "Paracetamol + Caffeine",
                ActiveIngredient = "Paracetamol",
                ActiveIngredientConcentration = "500mg",
                UnitsPerBox = 2,
                Barcode = "6221234567891",
                RequiresPrescription = false,
                CategoryId = categories[0].Id,
                IsActive = true
            };

            var cataflam = new Medicine
            {
                Name = "Cataflam 50mg - كتافلام 50 ملجم",
                ScientificName = "Diclofenac Potassium",
                ActiveIngredient = "Diclofenac Potassium",
                ActiveIngredientConcentration = "50mg",
                UnitsPerBox = 2,
                Barcode = "6221234567892",
                RequiresPrescription = false,
                CategoryId = categories[0].Id,
                IsActive = true
            };

            var adol = new Medicine
            {
                Name = "Adol 500mg - أدول 500 ملجم",
                ScientificName = "Paracetamol",
                ActiveIngredient = "Paracetamol",
                ActiveIngredientConcentration = "500mg",
                UnitsPerBox = 2,
                Barcode = "6221234567893",
                RequiresPrescription = false,
                CategoryId = categories[0].Id,
                IsActive = true
            };

            var augmentin = new Medicine
            {
                Name = "Augmentin 1g - أوجمنتين 1 جرام",
                ScientificName = "Amoxicillin + Clavulanic Acid",
                ActiveIngredient = "Amoxicillin",
                ActiveIngredientConcentration = "875mg",
                UnitsPerBox = 2,
                Barcode = "6221234567894",
                RequiresPrescription = true,
                CategoryId = categories[1].Id,
                IsActive = true
            };

            var zithromax = new Medicine
            {
                Name = "Zithromax 500mg - زيثرومكس 500 ملجم",
                ScientificName = "Azithromycin",
                ActiveIngredient = "Azithromycin",
                ActiveIngredientConcentration = "500mg",
                UnitsPerBox = 1,
                Barcode = "6221234567895",
                RequiresPrescription = true,
                CategoryId = categories[1].Id,
                IsActive = true
            };

            var centrum = new Medicine
            {
                Name = "Centrum with Lutein - سينتروم مع لوتين",
                ScientificName = "Multivitamins + Minerals",
                ActiveIngredient = "Multivitamins",
                ActiveIngredientConcentration = "Standard",
                UnitsPerBox = 1,
                Barcode = "6221234567896",
                RequiresPrescription = false,
                CategoryId = categories[2].Id,
                IsActive = true
            };

            var omron = new Medicine
            {
                Name = "Omron M2 Blood Pressure Monitor - جهاز ضغط أومرون",
                ScientificName = "Digital Upper Arm Blood Pressure Monitor",
                ActiveIngredient = "N/A",
                ActiveIngredientConcentration = "N/A",
                UnitsPerBox = 1,
                Barcode = "6221234567897",
                RequiresPrescription = false,
                CategoryId = categories[3].Id,
                IsActive = true
            };

            var medicines = new List<Medicine>
            {
                panadol,
                cataflam,
                adol,
                augmentin,
                zithromax,
                centrum,
                omron
            };

            await context.Medicines.AddRangeAsync(medicines);
            await context.SaveChangesAsync();


            // =========================================================
            // 5. Purchase Order
            // =========================================================

            var purchaseOrder = new PurchaseOrder
            {
                InvoiceNumber = "PO-2026-001",
                OrderDate = DateTime.UtcNow.AddDays(-10),
                SupplierId = suppliers[0].Id,
                TotalAmount = 0
            };

            await context.PurchaseOrders.AddAsync(purchaseOrder);
            await context.SaveChangesAsync();


            // =========================================================
            // 6. Purchase Order Items
            // =========================================================

            var purchaseItems = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = panadol.Id,
                    BatchNumber = "B2026-001",
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    Quantity = 120,
                    PurchasePrice = 35.00m,
                    SellingPrice = 45.00m,
                    TotalPrice = 120 * 35.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = cataflam.Id,
                    BatchNumber = "B2026-002",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    Quantity = 50,
                    PurchasePrice = 45.00m,
                    SellingPrice = 58.50m,
                    TotalPrice = 50 * 45.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = adol.Id,
                    BatchNumber = "B2026-003",
                    ExpiryDate = DateTime.UtcNow.AddMonths(18),
                    Quantity = 80,
                    PurchasePrice = 22.00m,
                    SellingPrice = 30.00m,
                    TotalPrice = 80 * 22.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = augmentin.Id,
                    BatchNumber = "B2026-004",
                    ExpiryDate = DateTime.UtcNow.AddMonths(10),
                    Quantity = 30,
                    PurchasePrice = 85.00m,
                    SellingPrice = 110.00m,
                    TotalPrice = 30 * 85.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = zithromax.Id,
                    BatchNumber = "B2026-005",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    Quantity = 20,
                    PurchasePrice = 75.00m,
                    SellingPrice = 95.00m,
                    TotalPrice = 20 * 75.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = centrum.Id,
                    BatchNumber = "B2026-006",
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    Quantity = 15,
                    PurchasePrice = 280.00m,
                    SellingPrice = 350.00m,
                    TotalPrice = 15 * 280.00m
                },

                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    MedicineId = omron.Id,
                    BatchNumber = "B2026-007",
                    ExpiryDate = DateTime.UtcNow.AddYears(3),
                    Quantity = 8,
                    PurchasePrice = 1500.00m,
                    SellingPrice = 1850.00m,
                    TotalPrice = 8 * 1500.00m
                }
            };

            await context.PurchaseOrderItems.AddRangeAsync(purchaseItems);
            await context.SaveChangesAsync();


            // =========================================================
            // 7. Update Purchase Order Total
            // =========================================================

            purchaseOrder.TotalAmount = purchaseItems.Sum(x => x.TotalPrice);

            context.PurchaseOrders.Update(purchaseOrder);
            await context.SaveChangesAsync();


            // =========================================================
            // 8. Medicine Batches
            // =========================================================

            var batches = new List<MedicineBatch>
            {
                new MedicineBatch
                {
                    MedicineId = panadol.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-001",
                    OriginalUnits = 120,
                    RemainingUnits = 120,
                    PurchasePrice = 35.00m,
                    SellingPrice = 45.00m,
                    ExpiryDate = DateTime.UtcNow.AddYears(2)
                },

                new MedicineBatch
                {
                    MedicineId = cataflam.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-002",
                    OriginalUnits = 50,
                    RemainingUnits = 50,
                    PurchasePrice = 45.00m,
                    SellingPrice = 58.50m,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                },

                new MedicineBatch
                {
                    MedicineId = adol.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-003",
                    OriginalUnits = 80,
                    RemainingUnits = 80,
                    PurchasePrice = 22.00m,
                    SellingPrice = 30.00m,
                    ExpiryDate = DateTime.UtcNow.AddMonths(18)
                },

                new MedicineBatch
                {
                    MedicineId = augmentin.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-004",
                    OriginalUnits = 30,
                    RemainingUnits = 30,
                    PurchasePrice = 85.00m,
                    SellingPrice = 110.00m,
                    ExpiryDate = DateTime.UtcNow.AddMonths(10)
                },

                new MedicineBatch
                {
                    MedicineId = zithromax.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-005",
                    OriginalUnits = 20,
                    RemainingUnits = 20,
                    PurchasePrice = 75.00m,
                    SellingPrice = 95.00m,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                },

                new MedicineBatch
                {
                    MedicineId = centrum.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-006",
                    OriginalUnits = 15,
                    RemainingUnits = 15,
                    PurchasePrice = 280.00m,
                    SellingPrice = 350.00m,
                    ExpiryDate = DateTime.UtcNow.AddYears(2)
                },

                new MedicineBatch
                {
                    MedicineId = omron.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    BatchNumber = "B2026-007",
                    OriginalUnits = 8,
                    RemainingUnits = 8,
                    PurchasePrice = 1500.00m,
                    SellingPrice = 1850.00m,
                    ExpiryDate = DateTime.UtcNow.AddYears(3)
                }
            };

            await context.MedicineBatches.AddRangeAsync(batches);
            await context.SaveChangesAsync();


            // =========================================================
            // 9. Drug Interactions
            // =========================================================

            var interactions = new List<DrugInteraction>
            {
                new DrugInteraction
                {
                    MedicineId = cataflam.Id,
                    InteractingMedicineId = augmentin.Id,

                    Description =
                        "قد يسبب تناول الأدوية معاً زيادة خطر بعض الآثار الجانبية، " +
                        "ويُنصح باستشارة الطبيب أو الصيدلي."
                },

                new DrugInteraction
                {
                    MedicineId = panadol.Id,
                    InteractingMedicineId = adol.Id,
                    Description =
                        "يحتوي كلا الدواءين على مادة Paracetamol، " +
                        "وقد يؤدي استخدامهما معاً إلى تجاوز الجرعة الآمنة."
                }
            };

            await context.DrugInteractions.AddRangeAsync(interactions);
            await context.SaveChangesAsync();


            // =========================================================
            // 10. Stock Transactions
            // =========================================================

            var stockTransactions = new List<StockTransaction>();

            foreach (var batch in batches)
            {
                stockTransactions.Add(new StockTransaction
                {
                    MedicineBatchId = batch.Id,
                    Type = StockTransactionType.Purchase,
                    Quantity = batch.OriginalUnits,
                    BalanceAfterTransaction = batch.RemainingUnits,
                    Reference = purchaseOrder.InvoiceNumber,
                    Notes = "Initial stock from purchase order",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.StockTransactions.AddRangeAsync(stockTransactions);
            await context.SaveChangesAsync();
        }
    }
}