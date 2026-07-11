using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PharmacySystem.Categories;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
using PharmacySystem.Purchases;
using PharmacySystem.Sales;
using PharmacySystem.Stocks;
using PharmacySystem.Suppliers;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace PharmacySystem.Data;

/// <summary>
/// Seeds a realistic, fully-linked demo data set (categories, suppliers,
/// customers, medicines, stock lots, purchase history and sales history) so a
/// fresh installation is immediately usable for testing every screen: catalogue
/// lists, stock, low-stock/expiring alerts, and the dashboard aggregates
/// (6-month sales/purchase trend, top-selling, stock-by-category, expiry
/// timeline).
/// </summary>
/// <remarks>
/// <para>
/// Stock is built the same way the running application builds it: purchases feed
/// <see cref="StockManager.IncreaseAsync"/> and sales feed
/// <see cref="StockManager.DecreaseAsync"/>, so current stock is always exactly
/// (purchased − sold) and never inconsistent. Sales only ever draw from healthy,
/// long-dated lots; deliberately short-dated and intentionally-low lots are left
/// untouched so the expiry/low-stock screens have stable, populated rows.
/// </para>
/// <para>
/// This is demo/test data and is therefore gated behind the
/// <c>PharmacySystem:SeedDemoData</c> configuration flag (see the DbMigrator's
/// appsettings.json). The flag is intentionally absent in the automated test
/// configuration, so this contributor never runs during unit/integration tests.
/// Seeding is also idempotent: if any medicine already exists it does nothing.
/// </para>
/// </remarks>
public class PharmacyDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    // Deterministic RNG so repeated seeds (on different empty databases) produce
    // the same shape of data. Not security-sensitive.
    private const int RandomSeed = 20260711;

    // How many sale documents to generate across the last 6 months.
    private const int SaleCount = 160;

    // How many lots go on one purchase document.
    private const int LotsPerPurchase = 4;

    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly StockManager _stockManager;

    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Purchase, Guid> _purchaseRepository;
    private readonly IRepository<Sale, Guid> _saleRepository;

    public ILogger<PharmacyDemoDataSeedContributor> Logger { get; set; }

    public PharmacyDemoDataSeedContributor(
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        StockManager stockManager,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Medicine, Guid> medicineRepository,
        IRepository<Purchase, Guid> purchaseRepository,
        IRepository<Sale, Guid> saleRepository)
    {
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _stockManager = stockManager;
        _categoryRepository = categoryRepository;
        _supplierRepository = supplierRepository;
        _customerRepository = customerRepository;
        _medicineRepository = medicineRepository;
        _purchaseRepository = purchaseRepository;
        _saleRepository = saleRepository;

        Logger = NullLogger<PharmacyDemoDataSeedContributor>.Instance;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Opt-in only. Absent/false (e.g. in automated tests) => do nothing.
        var flag = _configuration["PharmacySystem:SeedDemoData"];
        if (!string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using (_currentTenant.Change(context?.TenantId))
        {
            var rng = new Random(RandomSeed);
            var today = DateTime.Today;

            // 1) Master data (categories, suppliers, customers, medicines) in one
            //    transaction. Also the idempotency gate: skip everything if the
            //    catalogue is already populated.
            var master = await SeedMasterDataAsync(rng, today);
            if (master == null)
            {
                Logger.LogInformation("Pharmacy demo data already present; skipping seed.");
                return;
            }

            // 2) Build stock through purchase documents (spread over 6 months).
            var lots = BuildLots(master.Medicines, rng, today);
            await SeedPurchasesAndStockAsync(lots, master.SupplierIds, rng, today);

            // 3) Draw sales from healthy stock (spread over 6 months).
            await SeedSalesAsync(lots, master.ActiveCustomerIds, rng, today);

            Logger.LogInformation(
                "Pharmacy demo data seeded: {Categories} categories, {Suppliers} suppliers, " +
                "{Customers} customers, {Medicines} medicines, {Lots} stock lots, {Sales} sales.",
                master.CategoryCount, master.SupplierIds.Count, master.CustomerCount,
                master.Medicines.Count, lots.Count, SaleCount);
        }
    }

    // ----- Master data ------------------------------------------------------

    private async Task<MasterData?> SeedMasterDataAsync(Random rng, DateTime today)
    {
        MasterData? result = null;

        using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
        {
            // Idempotency: if any medicine exists we assume the database is
            // already seeded (or in use) and leave it untouched.
            if (await _medicineRepository.GetCountAsync() > 0)
            {
                return null;
            }

            var categoryIds = new List<Guid>();
            foreach (var (name, description) in Categories)
            {
                var id = _guidGenerator.Create();
                await _categoryRepository.InsertAsync(
                    new Category(id, name, description, isActive: true), autoSave: true);
                categoryIds.Add(id);
            }

            var supplierIds = new List<Guid>();
            foreach (var s in Suppliers)
            {
                var id = _guidGenerator.Create();
                await _supplierRepository.InsertAsync(
                    new Supplier(id, s.Name, s.Contact, s.Phone, s.Email, s.Address, isActive: true),
                    autoSave: true);
                supplierIds.Add(id);
            }

            var customerCount = 40;
            var activeCustomerIds = new List<Guid>();
            for (var i = 0; i < customerCount; i++)
            {
                var id = _guidGenerator.Create();
                var name = $"{FirstNames[i % FirstNames.Length]} {LastNames[(i * 7 + 3) % LastNames.Length]}";
                var gender = i % 2 == 0 ? "Male" : "Female";
                var dob = today.AddYears(-(18 + (i * 3) % 55)).AddDays(-((i * 29) % 300));
                var phone = "98" + (10000000 + (i * 1234567) % 89999999).ToString();
                var patientCode = $"PT-{i + 1:D4}";
                var address = Cities[i % Cities.Length];
                var isActive = i % 14 != 13; // a couple of inactive customers

                await _customerRepository.InsertAsync(
                    new Customer(id, name, phone, address, gender, dob, patientCode, isActive),
                    autoSave: true);

                if (isActive)
                {
                    activeCustomerIds.Add(id);
                }
            }

            var medicines = new List<MedRow>();
            for (var i = 0; i < Medicines.Length; i++)
            {
                var spec = Medicines[i];
                var id = _guidGenerator.Create();
                var isActive = i % 19 != 18; // a few discontinued medicines
                var barcode = "890" + (1000000000L + i).ToString(); // unique 13-digit EAN-like

                await _medicineRepository.InsertAsync(
                    new Medicine(
                        id,
                        spec.Name,
                        categoryIds[spec.Category],
                        spec.Purchase,
                        spec.Sale,
                        spec.Generic,
                        spec.Unit,
                        barcode,
                        spec.Reorder,
                        isActive),
                    autoSave: true);

                medicines.Add(new MedRow(id, i, spec.Purchase, spec.Sale, spec.Reorder, isActive));
            }

            await uow.CompleteAsync();

            result = new MasterData
            {
                CategoryCount = categoryIds.Count,
                SupplierIds = supplierIds,
                CustomerCount = customerCount,
                ActiveCustomerIds = activeCustomerIds,
                Medicines = medicines
            };
        }

        return result;
    }

    // ----- Stock lots -------------------------------------------------------

    // Expiry offsets (days from today) used for each medicine's showcase lot.
    // The negative and small positive values populate the "expired" and
    // "expiring soon" screens and dashboard buckets; the larger ones are normal
    // long-dated stock that sales can draw from.
    private static readonly int[] ShowcaseExpiryOffsets = { -20, 12, 40, 75, 150, 260, 400, 610 };

    private static List<LotState> BuildLots(List<MedRow> medicines, Random rng, DateTime today)
    {
        var lots = new List<LotState>();

        foreach (var med in medicines)
        {
            var i = med.Index;

            // Lot A: the "showcase" lot. Its expiry rotates through the buckets;
            // some are intentionally low to populate the low-stock report.
            var lowShowcase = i % 11 == 5;
            var offsetA = ShowcaseExpiryOffsets[i % ShowcaseExpiryOffsets.Length];
            var expiryA = today.AddDays(offsetA);
            var qtyA = lowShowcase
                ? rng.Next(3, Math.Max(4, med.Reorder))       // below reorder level
                : rng.Next(60, 200);
            // Only sell from lot A if it is active, not a low-stock showcase, and
            // comfortably long-dated (so short-dated lots keep their quantity).
            var sellableA = med.IsActive && !lowShowcase && offsetA >= 100;

            lots.Add(new LotState(
                med.Id, $"BN{i + 1:D3}-A", expiryA, qtyA, med.Purchase, med.Sale, sellableA));

            // Lot B: deep, long-dated, sellable stock for active medicines. This
            // guarantees plenty of supply for the sales history.
            if (med.IsActive)
            {
                var expiryB = today.AddDays(rng.Next(365, 700));
                var qtyB = rng.Next(220, 500);
                lots.Add(new LotState(
                    med.Id, $"BN{i + 1:D3}-B", expiryB, qtyB, med.Purchase, med.Sale, sellable: true));
            }
        }

        return lots;
    }

    private async Task SeedPurchasesAndStockAsync(
        List<LotState> lots, List<Guid> supplierIds, Random rng, DateTime today)
    {
        var purchaseCount = (int)Math.Ceiling(lots.Count / (double)LotsPerPurchase);

        for (var p = 0; p < purchaseCount; p++)
        {
            var slice = lots.Skip(p * LotsPerPurchase).Take(LotsPerPurchase).ToList();
            if (slice.Count == 0)
            {
                continue;
            }

            var supplierId = supplierIds[p % supplierIds.Count];
            var date = today.AddMonths(-(p % 6)).AddDays(-rng.Next(0, 12));

            var purchase = new Purchase(
                _guidGenerator.Create(),
                $"PUR-{p + 1:D5}",
                supplierId,
                date,
                invoiceNumber: $"INV-{1000 + p}",
                notes: null,
                discountAmount: 0m);

            foreach (var lot in slice)
            {
                purchase.AddItem(lot.MedicineId, lot.Quantity, lot.UnitCost, lot.Batch, lot.Expiry);
            }

            purchase.EnsureValid();

            await InUnitOfWorkAsync(async () =>
            {
                await _purchaseRepository.InsertAsync(purchase, autoSave: true);

                foreach (var lot in slice)
                {
                    await _stockManager.IncreaseAsync(
                        lot.MedicineId, lot.Batch, lot.Expiry, lot.Quantity, lot.UnitCost);
                }
            });
        }
    }

    private async Task SeedSalesAsync(
        List<LotState> lots, List<Guid> activeCustomerIds, Random rng, DateTime today)
    {
        var sellableLots = lots.Where(l => l.Sellable).ToList();
        if (sellableLots.Count == 0)
        {
            return;
        }

        for (var s = 0; s < SaleCount; s++)
        {
            var pool = sellableLots.Where(l => l.Available >= 5).ToList();
            if (pool.Count == 0)
            {
                break; // ran out of sellable stock (should not happen with the seeded volumes)
            }

            var date = today.AddMonths(-(s % 6)).AddDays(-rng.Next(0, 18));

            Guid? customerId = rng.Next(100) < 70 && activeCustomerIds.Count > 0
                ? activeCustomerIds[rng.Next(activeCustomerIds.Count)]
                : null; // ~30% walk-in sales

            var itemCount = Math.Min(rng.Next(1, 5), pool.Count);
            var chosen = new List<LotState>(itemCount);
            while (chosen.Count < itemCount)
            {
                var candidate = pool[rng.Next(pool.Count)];
                if (!chosen.Contains(candidate))
                {
                    chosen.Add(candidate);
                }
            }

            var sale = new Sale(
                _guidGenerator.Create(),
                $"SAL-{s + 1:D5}",
                date,
                customerId,
                notes: null,
                discountAmount: 0m);

            var lines = new List<(LotState Lot, int Qty)>();
            foreach (var lot in chosen)
            {
                var qty = rng.Next(1, Math.Min(lot.Available, 20) + 1);
                lot.Available -= qty; // reserve immediately so the pool stays consistent
                sale.AddItem(lot.MedicineId, qty, lot.SalePrice, lot.Batch, lot.Expiry);
                lines.Add((lot, qty));
            }

            // Apply a small discount to roughly a third of sales.
            if (rng.Next(100) < 35)
            {
                var total = sale.TotalAmount;
                var discount = Math.Round(total * (decimal)(rng.Next(2, 9) / 100.0), 2);
                if (discount > 0 && discount < total)
                {
                    sale.SetDiscountAmount(discount);
                }
            }

            sale.EnsureValid();

            await InUnitOfWorkAsync(async () =>
            {
                // Mirror the application service: deduct stock, then persist the sale.
                foreach (var (lot, qty) in lines)
                {
                    await _stockManager.DecreaseAsync(lot.MedicineId, lot.Batch, lot.Expiry, qty);
                }

                await _saleRepository.InsertAsync(sale, autoSave: true);
            });
        }
    }

    // Runs a body in its own committed unit of work. Keeping each purchase/sale
    // in a small transaction avoids unbounded change-tracker growth over the
    // hundreds of writes this seeder performs.
    private async Task InUnitOfWorkAsync(Func<Task> body)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        await body();
        await uow.CompleteAsync();
    }

    // ----- Local models -----------------------------------------------------

    private sealed class MasterData
    {
        public int CategoryCount { get; init; }
        public required List<Guid> SupplierIds { get; init; }
        public int CustomerCount { get; init; }
        public required List<Guid> ActiveCustomerIds { get; init; }
        public required List<MedRow> Medicines { get; init; }
    }

    private sealed record MedRow(Guid Id, int Index, decimal Purchase, decimal Sale, int Reorder, bool IsActive);

    private sealed class LotState
    {
        public LotState(Guid medicineId, string batch, DateTime? expiry, int quantity,
            decimal unitCost, decimal salePrice, bool sellable)
        {
            MedicineId = medicineId;
            Batch = batch;
            Expiry = expiry;
            Quantity = quantity;
            Available = quantity;
            UnitCost = unitCost;
            SalePrice = salePrice;
            Sellable = sellable;
        }

        public Guid MedicineId { get; }
        public string Batch { get; }
        public DateTime? Expiry { get; }
        public int Quantity { get; }
        public int Available { get; set; }
        public decimal UnitCost { get; }
        public decimal SalePrice { get; }
        public bool Sellable { get; }
    }

    // ----- Static catalogue data -------------------------------------------

    private static readonly (string Name, string? Description)[] Categories =
    {
        ("Analgesics", "Pain relievers and anti-inflammatory medicines"),
        ("Antibiotics", "Medicines that treat bacterial infections"),
        ("Antacids & Antiulcer", "Acidity, reflux and ulcer treatments"),
        ("Antihistamines", "Allergy and hypersensitivity relief"),
        ("Antihypertensives", "Blood pressure control medicines"),
        ("Antidiabetics", "Blood sugar control medicines"),
        ("Vitamins & Supplements", "Nutritional and dietary supplements"),
        ("Dermatology", "Skin creams, ointments and topical care"),
        ("Respiratory", "Asthma, cough and breathing medicines"),
        ("Cardiovascular", "Heart and cholesterol medicines"),
        ("Gastrointestinal", "Digestive and gut medicines"),
        ("Ophthalmic", "Eye drops and eye-care preparations"),
    };

    private static readonly (string Name, string Contact, string Phone, string Email, string Address)[] Suppliers =
    {
        ("MediCore Distributors", "Ramesh Shrestha", "9801000011", "sales@medicore.example", "Kathmandu"),
        ("Himalayan Pharma Supplies", "Sita Gurung", "9801000022", "info@himpharma.example", "Lalitpur"),
        ("Everest Medical Traders", "Bikash Thapa", "9801000033", "orders@everestmed.example", "Bhaktapur"),
        ("Nova Health Wholesale", "Anita Rai", "9801000044", "contact@novahealth.example", "Pokhara"),
        ("PrimeCare Pharmaceuticals", "Deepak Karki", "9801000055", "sales@primecare.example", "Biratnagar"),
        ("Sunrise Drug House", "Kabita Magar", "9801000066", "hello@sunrisedrug.example", "Birgunj"),
        ("Global Meds Link", "Suresh Adhikari", "9801000077", "support@globalmeds.example", "Butwal"),
        ("Zenith Life Sciences", "Puja Sharma", "9801000088", "orders@zenithls.example", "Dharan"),
        ("CarePlus Distribution", "Nabin Basnet", "9801000099", "sales@careplus.example", "Hetauda"),
        ("Wellness Pharma Hub", "Manisha Koirala", "9801000110", "info@wellnesshub.example", "Nepalgunj"),
        ("Alpha Medico Supply", "Rajan Bhandari", "9801000121", "orders@alphamedico.example", "Itahari"),
        ("United Remedies", "Sabina Tamang", "9801000132", "contact@unitedremedies.example", "Janakpur"),
    };

    private static readonly string[] FirstNames =
    {
        "Aarav", "Priya", "Rohan", "Sneha", "Kiran", "Maya", "Nabin", "Anita",
        "Bikash", "Sita", "Deepak", "Puja", "Suresh", "Kabita", "Rajan", "Manisha",
        "Sabin", "Rekha", "Prakash", "Gita",
    };

    private static readonly string[] LastNames =
    {
        "Sharma", "Shrestha", "Gurung", "Thapa", "Rai", "Karki", "Adhikari", "Magar",
        "Basnet", "Koirala", "Bhandari", "Tamang", "Pandey", "Khadka", "Poudel", "Dahal",
        "Bhattarai", "Acharya", "Lama", "Joshi",
    };

    private static readonly string[] Cities =
    {
        "Kathmandu", "Lalitpur", "Bhaktapur", "Pokhara", "Biratnagar",
        "Birgunj", "Butwal", "Dharan", "Hetauda", "Nepalgunj",
    };

    // Name, Generic, CategoryIndex, Unit, PurchasePrice, SalePrice, ReorderLevel
    private static readonly (string Name, string Generic, int Category, string Unit, decimal Purchase, decimal Sale, int Reorder)[] Medicines =
    {
        // Analgesics (0)
        ("Paracetamol 500mg", "Acetaminophen", 0, "Tablet", 1.20m, 2.00m, 100),
        ("Ibuprofen 400mg", "Ibuprofen", 0, "Tablet", 1.80m, 3.00m, 80),
        ("Aspirin 300mg", "Acetylsalicylic Acid", 0, "Tablet", 1.00m, 1.80m, 60),
        ("Diclofenac 50mg", "Diclofenac Sodium", 0, "Tablet", 2.20m, 3.80m, 50),
        ("Naproxen 250mg", "Naproxen", 0, "Tablet", 3.00m, 5.00m, 40),
        ("Tramadol 50mg", "Tramadol HCl", 0, "Capsule", 4.50m, 7.50m, 30),

        // Antibiotics (1)
        ("Amoxicillin 500mg", "Amoxicillin", 1, "Capsule", 3.50m, 6.00m, 60),
        ("Azithromycin 500mg", "Azithromycin", 1, "Tablet", 8.00m, 13.00m, 40),
        ("Ciprofloxacin 500mg", "Ciprofloxacin", 1, "Tablet", 5.00m, 8.50m, 40),
        ("Co-amoxiclav 625mg", "Amoxicillin/Clavulanate", 1, "Tablet", 9.00m, 15.00m, 35),
        ("Doxycycline 100mg", "Doxycycline", 1, "Capsule", 4.00m, 7.00m, 30),
        ("Metronidazole 400mg", "Metronidazole", 1, "Tablet", 2.50m, 4.50m, 50),

        // Antacids & Antiulcer (2)
        ("Omeprazole 20mg", "Omeprazole", 2, "Capsule", 2.80m, 5.00m, 70),
        ("Pantoprazole 40mg", "Pantoprazole", 2, "Tablet", 3.20m, 5.50m, 60),
        ("Ranitidine 150mg", "Ranitidine", 2, "Tablet", 1.50m, 2.80m, 50),
        ("Antacid Suspension", "Aluminium/Magnesium Hydroxide", 2, "Bottle", 3.50m, 6.00m, 40),
        ("Esomeprazole 40mg", "Esomeprazole", 2, "Tablet", 5.50m, 9.00m, 30),

        // Antihistamines (3)
        ("Cetirizine 10mg", "Cetirizine", 3, "Tablet", 1.20m, 2.20m, 80),
        ("Loratadine 10mg", "Loratadine", 3, "Tablet", 1.60m, 3.00m, 60),
        ("Chlorpheniramine 4mg", "Chlorphenamine", 3, "Tablet", 0.80m, 1.50m, 50),
        ("Fexofenadine 120mg", "Fexofenadine", 3, "Tablet", 3.00m, 5.20m, 40),
        ("Levocetirizine 5mg", "Levocetirizine", 3, "Tablet", 2.00m, 3.60m, 40),

        // Antihypertensives (4)
        ("Amlodipine 5mg", "Amlodipine", 4, "Tablet", 1.80m, 3.20m, 70),
        ("Losartan 50mg", "Losartan Potassium", 4, "Tablet", 2.60m, 4.50m, 60),
        ("Enalapril 10mg", "Enalapril", 4, "Tablet", 2.00m, 3.60m, 50),
        ("Metoprolol 50mg", "Metoprolol", 4, "Tablet", 2.40m, 4.20m, 45),
        ("Telmisartan 40mg", "Telmisartan", 4, "Tablet", 3.20m, 5.60m, 40),

        // Antidiabetics (5)
        ("Metformin 500mg", "Metformin HCl", 5, "Tablet", 1.40m, 2.60m, 90),
        ("Glimepiride 2mg", "Glimepiride", 5, "Tablet", 2.20m, 4.00m, 50),
        ("Gliclazide 80mg", "Gliclazide", 5, "Tablet", 2.60m, 4.60m, 40),
        ("Sitagliptin 100mg", "Sitagliptin", 5, "Tablet", 9.00m, 15.00m, 30),
        ("Insulin Glargine", "Insulin Glargine", 5, "Injection", 20.00m, 32.00m, 20),

        // Vitamins & Supplements (6)
        ("Vitamin C 500mg", "Ascorbic Acid", 6, "Tablet", 1.00m, 2.00m, 100),
        ("Vitamin D3 60000 IU", "Cholecalciferol", 6, "Capsule", 3.50m, 6.00m, 60),
        ("Multivitamin", "Multivitamin", 6, "Tablet", 2.50m, 4.50m, 70),
        ("Calcium + D3", "Calcium Carbonate/D3", 6, "Tablet", 2.20m, 4.00m, 60),
        ("Ferrous Sulphate", "Iron", 6, "Tablet", 1.60m, 3.00m, 50),
        ("Zinc 50mg", "Zinc Sulphate", 6, "Tablet", 1.40m, 2.60m, 50),
        ("Folic Acid 5mg", "Folic Acid", 6, "Tablet", 0.90m, 1.80m, 60),

        // Dermatology (7)
        ("Betamethasone Cream", "Betamethasone", 7, "Tube", 2.80m, 5.00m, 40),
        ("Clotrimazole Cream", "Clotrimazole", 7, "Tube", 2.40m, 4.40m, 40),
        ("Mupirocin Ointment", "Mupirocin", 7, "Tube", 4.00m, 7.00m, 30),
        ("Calamine Lotion", "Calamine", 7, "Bottle", 2.00m, 3.80m, 35),
        ("Ketoconazole Shampoo", "Ketoconazole", 7, "Bottle", 5.00m, 8.50m, 25),

        // Respiratory (8)
        ("Salbutamol Inhaler", "Salbutamol", 8, "Inhaler", 6.00m, 10.00m, 40),
        ("Montelukast 10mg", "Montelukast", 8, "Tablet", 3.60m, 6.20m, 40),
        ("Cough Syrup", "Dextromethorphan", 8, "Bottle", 3.00m, 5.50m, 50),
        ("Budesonide Inhaler", "Budesonide", 8, "Inhaler", 8.00m, 13.00m, 25),
        ("Ambroxol Syrup", "Ambroxol", 8, "Bottle", 2.80m, 5.00m, 40),

        // Cardiovascular (9)
        ("Atorvastatin 10mg", "Atorvastatin", 9, "Tablet", 2.40m, 4.40m, 60),
        ("Rosuvastatin 10mg", "Rosuvastatin", 9, "Tablet", 3.20m, 5.60m, 50),
        ("Clopidogrel 75mg", "Clopidogrel", 9, "Tablet", 3.60m, 6.20m, 45),
        ("Isosorbide Mononitrate", "ISMN", 9, "Tablet", 2.20m, 4.00m, 35),
        ("Digoxin 0.25mg", "Digoxin", 9, "Tablet", 1.80m, 3.40m, 30),

        // Gastrointestinal (10)
        ("Ondansetron 4mg", "Ondansetron", 10, "Tablet", 2.60m, 4.60m, 40),
        ("Domperidone 10mg", "Domperidone", 10, "Tablet", 1.80m, 3.20m, 50),
        ("Loperamide 2mg", "Loperamide", 10, "Capsule", 1.40m, 2.60m, 45),
        ("ORS Sachet", "Oral Rehydration Salts", 10, "Sachet", 0.60m, 1.20m, 100),
        ("Lactulose Solution", "Lactulose", 10, "Bottle", 3.40m, 5.80m, 35),

        // Ophthalmic (11)
        ("Moxifloxacin Eye Drops", "Moxifloxacin", 11, "Drops", 4.00m, 7.00m, 30),
        ("Lubricant Eye Drops", "Carboxymethylcellulose", 11, "Drops", 3.00m, 5.40m, 35),
        ("Timolol Eye Drops", "Timolol", 11, "Drops", 3.60m, 6.20m, 25),
        ("Tobramycin Eye Drops", "Tobramycin", 11, "Drops", 4.20m, 7.20m, 25),
    };
}
