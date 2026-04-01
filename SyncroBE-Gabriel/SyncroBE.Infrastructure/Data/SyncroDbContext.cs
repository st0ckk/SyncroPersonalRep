using Microsoft.EntityFrameworkCore;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Infrastructure.Data
{
    public class SyncroDbContext : DbContext
    {
        public SyncroDbContext(DbContextOptions<SyncroDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Distributor> Distributors => Set<Distributor>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<ClientLocation> ClientLocations => Set<ClientLocation>();
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<Canton> Cantons => Set<Canton>();
        public DbSet<District> Districts => Set<District>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
        public DbSet<Quote> Quotes => Set<Quote>();
        public DbSet<QuoteDetail> QuotesDetail => Set<QuoteDetail>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<Tax> Taxes => Set<Tax>();
        public DbSet<DeliveryRoute> DeliveryRoutes => Set<DeliveryRoute>();
        public DbSet<DeliveryRouteStop> DeliveryRouteStops => Set<DeliveryRouteStop>();
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<UserVacationBalance> UserVacationBalances { get; set; }
        public DbSet<VacationMovement> VacationMovements { get; set; }
        public DbSet<RouteTemplate> RouteTemplates => Set<RouteTemplate>();
        public DbSet<RouteTemplateStop> RouteTemplateStops => Set<RouteTemplateStop>();
        public DbSet<ClientAccount> ClientAccounts => Set<ClientAccount>();
        public DbSet<ClientAccountMovement> ClientAccountMovements => Set<ClientAccountMovement>();

        // ── Electronic Invoice (Hacienda) ──
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<CompanyConfig> CompanyConfigs => Set<CompanyConfig>();
        public DbSet<HaciendaConsecutive> HaciendaConsecutives => Set<HaciendaConsecutive>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* DISTRIBUTOR */
            modelBuilder.Entity<Distributor>(entity =>
            {
                entity.ToTable("distributor");

                entity.HasKey(e => e.DistributorId);

                entity.Property(e => e.DistributorId).HasColumnName("distributor_id");

                entity.Property(e => e.DistributorCode)
                      .HasColumnName("distributor_code")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => e.DistributorCode).IsUnique();

                entity.Property(e => e.Name)
                      .HasColumnName("name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.Email)
                      .HasColumnName("email")
                      .HasMaxLength(150);

                entity.Property(e => e.Phone)
                      .HasColumnName("phone")
                      .HasMaxLength(50);

                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            /* PRODUCT */
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");

                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.DistributorId)
                      .HasColumnName("distributor_id")
                      .IsRequired();

                entity.HasOne(e => e.Distributor)
                      .WithMany(d => d.Products)
                      .HasForeignKey(e => e.DistributorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.ProductName)
                      .HasColumnName("product_name")
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.ProductType)
                      .HasColumnName("product_type")
                      .HasMaxLength(100);

                entity.Property(e => e.ProductPrice)
                      .HasColumnName("product_price")
                      .HasColumnType("decimal(10,2)");

                entity.Property(e => e.PulperoPrice)
                      .HasColumnName("pulpero_price")
                      .HasColumnType("decimal(10,2)");

                entity.Property(e => e.ExtranjeroPrice)
                      .HasColumnName("extranjero_price")
                      .HasColumnType("decimal(10,2)");

                entity.Property(e => e.RuteroPrice)
                      .HasColumnName("rutero_price")
                      .HasColumnType("decimal(10,2)");

                entity.Property(e => e.ProductQuantity).HasColumnName("product_quantity");
                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.CabysCode)
                      .HasColumnName("cabys_code")
                      .HasMaxLength(13);

                entity.Property(e => e.IsService)
                      .HasColumnName("is_service")
                      .HasDefaultValue(false);
            });

            /* CLIENT */
            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("clients");

                entity.HasKey(e => e.ClientId);

                entity.Property(e => e.ClientId).HasColumnName("client_id");

                entity.Property(e => e.ClientName).HasColumnName("client_name");
                entity.Property(e => e.ClientEmail).HasColumnName("client_email");
                entity.Property(e => e.ClientPhone).HasColumnName("client_phone");
                entity.Property(e => e.ClientElectronicInvoice).HasColumnName("client_electronic_invoice");
                entity.Property(e => e.ClientType).HasColumnName("client_type");
                entity.Property(e => e.HaciendaIdType)
                      .HasColumnName("hacienda_id_type")
                      .HasMaxLength(2);
                entity.Property(e => e.ClientPurchases).HasColumnName("client_purchases");
                entity.Property(e => e.ClientLastPurchase).HasColumnName("client_last_purchase");

                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.ProvinceCode).HasColumnName("province_code");
                entity.Property(e => e.CantonCode).HasColumnName("canton_code");
                entity.Property(e => e.DistrictCode).HasColumnName("district_code");
                entity.Property(e => e.ExactAddress).HasColumnName("exact_address");

                // Exoneration fields
                entity.Property(e => e.ExonerationDocType)
                      .HasColumnName("exoneration_doc_type")
                      .HasMaxLength(2);
                entity.Property(e => e.ExonerationDocNumber)
                      .HasColumnName("exoneration_doc_number")
                      .HasMaxLength(40);
                entity.Property(e => e.ExonerationInstitutionCode)
                      .HasColumnName("exoneration_institution_code")
                      .HasMaxLength(2);
                entity.Property(e => e.ExonerationInstitutionName)
                      .HasColumnName("exoneration_institution_name")
                      .HasMaxLength(160);
                entity.Property(e => e.ExonerationDate)
                      .HasColumnName("exoneration_date");
                entity.Property(e => e.ExonerationPercentage)
                      .HasColumnName("exoneration_percentage");
                entity.Property(e => e.ActivityCode)
                      .HasColumnName("activity_code")
                      .HasMaxLength(6);

                entity.HasOne(e => e.Location)
                      .WithOne(l => l.Client)
                      .HasForeignKey<ClientLocation>(l => l.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.District)
                      .WithMany(c => c.Clients)
                      .HasForeignKey(e => e.DistrictCode)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Canton)
                      .WithMany(c => c.Clients)
                      .HasForeignKey(e => e.CantonCode)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Province)
                      .WithMany(c => c.Clients)
                      .HasForeignKey(e => e.ProvinceCode)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* CLIENT LOCATION */
            modelBuilder.Entity<ClientLocation>(entity =>
            {
                entity.ToTable("client_location");

                entity.HasKey(e => e.LocationId);

                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.ClientId).HasColumnName("client_id");

                entity.Property(e => e.Latitude)
                      .HasColumnName("latitude")
                      .HasPrecision(9, 6);   // ✅ evita truncamiento

                entity.Property(e => e.Longitude)
                      .HasColumnName("longitude")
                      .HasPrecision(9, 6);   // ✅ evita truncamiento

                entity.Property(e => e.Address).HasColumnName("address");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            /* PROVINCE */
            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("province");

                entity.HasKey(e => e.ProvinceCode);

                entity.Property(e => e.ProvinceCode).HasColumnName("province_code");
                entity.Property(e => e.ProvinceName).HasColumnName("province_name");
            });

            /* CANTON */
            modelBuilder.Entity<Canton>(entity =>
            {
                entity.ToTable("canton");

                entity.HasKey(e => e.CantonCode);

                entity.Property(e => e.CantonCode).HasColumnName("canton_code");
                entity.Property(e => e.CantonName).HasColumnName("canton_name");
                entity.Property(e => e.ProvinceCode).HasColumnName("province_code");

                entity.HasOne(e => e.Province)
                      .WithMany(d => d.Cantons)
                      .HasForeignKey(e => e.ProvinceCode)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* DISTRICT */
            modelBuilder.Entity<District>(entity =>
            {
                entity.ToTable("district");

                entity.HasKey(e => e.DistrictCode);

                entity.Property(e => e.DistrictCode).HasColumnName("district_code");
                entity.Property(e => e.DistrictName).HasColumnName("district_name");
                entity.Property(e => e.CantonCode).HasColumnName("canton_code");

                entity.HasOne(e => e.Canton)
                      .WithMany(d => d.Districts)
                      .HasForeignKey(e => e.CantonCode)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* USERS */
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.UserRole)
                      .HasColumnName("user_role")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.UserName)
                      .HasColumnName("user_name")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.UserLastname)
                      .HasColumnName("user_lastname")
                      .HasMaxLength(100);

                entity.Property(e => e.UserEmail)
                      .HasColumnName("user_email")
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.PasswordHash)
                      .HasColumnName("password_hash")
                      .IsRequired();

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
                entity.Property(e => e.MustChangePassword).HasColumnName("MustChangePassword");
                entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
                entity.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
                entity.Property(e => e.Telefono).HasColumnName("telefono");
                entity.Property(e => e.TelefonoPersonal).HasColumnName("telefono_personal");
            });

            /* QUOTES */
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.ToTable("quotes");

                entity.HasKey(e => e.QuoteId);

                entity.HasIndex(e => e.QuoteNumber)
                      .HasDatabaseName("quote_number");

                entity.Property(e => e.QuoteId).HasColumnName("quote_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ClientId)
                      .HasColumnName("client_id")
                      .HasColumnType("varchar(20)")
                      .IsRequired();

                entity.Property(e => e.DiscountId)
                      .HasColumnName("discount_id")
                      .IsRequired(false);

                entity.Property(e => e.QuoteNumber)
                      .HasColumnName("quote_number")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.QuoteCustomer)
                      .HasColumnName("quote_customer")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.QuoteDate)
                .HasColumnType("datetime2")
                .HasColumnName("quote_date");

                entity.Property(e => e.QuoteValidDate)
                .HasColumnType("datetime2")
                .HasColumnName("quote_validdate");

                entity.Property(e => e.QuoteRemarks)
                      .HasColumnName("quote_remarks")
                      .HasColumnType("varchar(max)");

                entity.Property(e => e.QuoteConditions)
                      .HasColumnName("quote_conditions")
                      .HasColumnType("varchar(max)");

                entity.Property(e => e.QuoteStatus)
                    .HasColumnName("quote_status")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.QuoteDiscountApplied).HasColumnName("quote_discountapplied");

                entity.Property(e => e.QuoteDiscountPercentage).HasColumnName("quote_discountpercentage");

                entity.Property(e => e.QuoteDiscountReason)
                    .HasColumnName("quote_discountreason")
                    .HasColumnType("varchar(max)");


                entity.HasOne(e => e.User)
                      .WithMany(q => q.Quotes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                      .WithMany(q => q.Quotes)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Discount)
                      .WithMany(q => q.Quotes)
                      .HasForeignKey(e => e.DiscountId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            /* QUOTE DETAIL */
            modelBuilder.Entity<QuoteDetail>(entity =>
            {
                entity.ToTable("quote_detail");

                entity.HasKey(e => e.QuoteDetailId);

                entity.Property(e => e.QuoteId).HasColumnName("quote_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.ProductName)
                      .HasColumnName("product_name")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.UnitPrice)
                      .HasColumnName("unit_price")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.LineTotal)
                      .HasColumnName("line_total")
                      .HasColumnType("decimal(18, 2)")
                      .HasComputedColumnSql("[quantity] * [unit_price]", stored: true);

                entity.HasOne(e => e.Quote)
                      .WithMany(q => q.QuoteDetails)
                      .HasForeignKey(e => e.QuoteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(q => q.QuoteDetails)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* ASSET */
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("asset");

                entity.HasKey(e => e.AssetId);

                entity.Property(e => e.AssetId).HasColumnName("asset_id");

                entity.Property(e => e.AssetName)
                      .HasColumnName("asset_name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasColumnName("description")
                      .HasMaxLength(500);

                entity.Property(e => e.SerialNumber)
                      .HasColumnName("serial_number")
                      .HasMaxLength(100);

                entity.Property(e => e.Observations)
                      .HasColumnName("observations")
                      .HasMaxLength(1000);

                entity.Property(e => e.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(e => e.AssignmentDate).HasColumnName("assignment_date");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* TAX */
            modelBuilder.Entity<Tax>(entity =>
            {
                entity.ToTable("tax");

                entity.HasKey(e => e.TaxId);

                entity.Property(e => e.TaxId).HasColumnName("tax_id");

                entity.Property(e => e.TaxName)
                      .HasColumnName("tax_name")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Percentage)
                      .HasColumnName("percentage")
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.HaciendaTaxCode)
                      .HasColumnName("hacienda_tax_code")
                      .HasMaxLength(2);

                entity.Property(e => e.HaciendaIvaRateCode)
                      .HasColumnName("hacienda_iva_rate_code")
                      .HasMaxLength(2);
            });

            /* PURCHASE */
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.ToTable("purchase");

                entity.HasKey(e => e.PurchaseId);

                entity.HasIndex(e => e.PurchaseOrderNumber)
                      .HasDatabaseName("purchase_ordernumber");

                entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ClientId)
                      .HasColumnName("client_id")
                      .HasColumnType("varchar(20)")
                      .IsRequired();

                entity.Property(e => e.DiscountId)
                      .HasColumnName("discount_id")
                      .IsRequired(false);

                entity.Property(e => e.RouteId).HasColumnName("route_id");

                entity.Property(e => e.ClientAccountId)
                      .HasColumnName("clientaccount_id")
                      .IsRequired(false);

                entity.Property(e => e.PurchaseOrderNumber)
                        .HasColumnName("purchase_ordernumber")
                        .HasMaxLength(50)
                        .IsRequired();

                entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
                entity.Property(e => e.PurchasePaid).HasColumnName("purchase_paid");

                entity.Property(e => e.TaxId).HasColumnName("tax_id");
                entity.Property(e => e.TaxPercentage)
                      .HasColumnName("tax_percentage")
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.Subtotal)
                      .HasColumnName("subtotal")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TaxAmount)
                      .HasColumnName("tax_amount")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Total)
                      .HasColumnName("total")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.PurchaseDiscountApplied).HasColumnName("purchase_discountapplied");

                entity.Property(e => e.PurchaseDiscountPercentage).HasColumnName("purchase_discountpercentage");

                entity.Property(e => e.PurchaseDiscountReason)
                    .HasColumnName("purchase_discountreason")
                    .HasColumnType("varchar(max)");

                entity.Property(e => e.PurchasePaymentMethod)
                       .HasColumnName("purchase_paymentmethod")
                       .HasColumnType("varchar(20)");

            entity.HasOne(e => e.User)
                      .WithMany(p => p.Purchases)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                      .WithMany(p => p.Purchases)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tax)
                      .WithMany(p => p.Purchases)
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Discount)
                      .WithMany(P => P.Purchases)
                      .HasForeignKey(e => e.DiscountId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Route)
                       .WithMany(P => P.Purchases)
                       .HasForeignKey(e => e.RouteId)
                       .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ClientAccount)
                      .WithMany(P => P.Purchases)
                      .HasForeignKey(e => e.ClientAccountId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            /* SALE DETAIL */
            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.ToTable("sale_detail");

                entity.HasKey(e => e.SaleDetailId);

                entity.Property(e => e.SaleDetailId).HasColumnName("sale_detail_id");
                entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.ProductName)
                      .HasColumnName("product_name")
                      .HasMaxLength(255);

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.UnitPrice)
                      .HasColumnName("unit_price")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LineTotal)
                      .HasColumnName("line_total")
                      .HasColumnType("decimal(18,2)")
                      .HasComputedColumnSql("[quantity] * [unit_price]", stored: true);

                entity.HasOne(e => e.Purchase)
                      .WithMany(p => p.SaleDetails)
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.SaleDetails)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* EMPLOYEE SCHEDULE */
            modelBuilder.Entity<EmployeeSchedule>(entity =>
            {
                entity.ToTable("employee_schedule");

                entity.HasKey(e => e.ScheduleId);

                entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");

                entity.Property(e => e.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(e => e.StartAt)
                      .HasColumnName("start_at")
                      .IsRequired();

                entity.Property(e => e.EndAt)
                      .HasColumnName("end_at")
                      .IsRequired();

                entity.Property(e => e.Notes)
                      .HasColumnName("notes")
                      .HasMaxLength(500);

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            /* DESCUENTOS */
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("discount");

                entity.HasKey(e => e.DiscountId);

                entity.Property(e => e.DiscountId).HasColumnName("discount_id");

                entity.Property(e => e.DiscountName)
                      .HasColumnName("discount_name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage");

                entity.Property(e => e.IsActive).HasColumnName("is_active");
            });

            /* VACATIONS */
            modelBuilder.Entity<Vacation>(entity =>
            {
                entity.ToTable("vacations");

                entity.HasKey(e => e.VacationId);
                entity.Property(e => e.VacationId).HasColumnName("vacation_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("date");
                entity.Property(e => e.EndDate).HasColumnName("end_date").HasColumnType("date");
                entity.Property(e => e.DaysRequested).HasColumnName("days_requested").HasColumnType("decimal(5,2)");
                entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.User)
                      .WithMany() // si luego agregás colecciones en User, aquí lo cambiás
                      .HasForeignKey(e => e.UserId)
                      .HasConstraintName("FK_Vacations_Users");
            });

            modelBuilder.Entity<UserVacationBalance>(entity =>
            {
                entity.ToTable("user_vacation_balance");

                entity.HasKey(e => e.VBalanceId);
                entity.Property(e => e.VBalanceId).HasColumnName("v_balance_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AvailableDays).HasColumnName("available_days").HasColumnType("decimal(5,2)");
                entity.Property(e => e.LastAccrualDate).HasColumnName("last_accrual_date").HasColumnType("date");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<UserVacationBalance>(e => e.UserId)
                      .HasConstraintName("FK_user_vacation_balance_users");
            });

            modelBuilder.Entity<VacationMovement>(entity =>
            {
                entity.ToTable("vacation_movements");

                entity.HasKey(e => e.MovementsId);
                entity.Property(e => e.MovementsId).HasColumnName("movements_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.MovementType).HasColumnName("movement_type").HasMaxLength(20);
                entity.Property(e => e.Days).HasColumnName("days").HasColumnType("decimal(5,2)");
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .HasConstraintName("FK_vacation_movements_Users");
            });
            /* DELIVERY ROUTE */
            modelBuilder.Entity<DeliveryRoute>(entity =>
            {
                entity.ToTable("delivery_route");

                entity.HasKey(e => e.RouteId);

                entity.Property(e => e.RouteId).HasColumnName("route_id");
                entity.Property(e => e.RouteName).HasColumnName("route_name").HasMaxLength(150).IsRequired();
                entity.Property(e => e.RouteDate).HasColumnName("route_date").HasColumnType("date");
                entity.Property(e => e.DriverUserId).HasColumnName("driver_user_id").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.StartAtPlanned).HasColumnName("start_at_planned");
                entity.Property(e => e.EndAtEstimated).HasColumnName("end_at_estimated");
                entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
                entity.Property(e => e.EstimatedDistanceKm).HasColumnName("estimated_distance_km").HasColumnType("decimal(10,2)");
                entity.Property(e => e.Polyline).HasColumnName("polyline");
                entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.DriverUser)
                      .WithMany()
                      .HasForeignKey(e => e.DriverUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* DELIVERY ROUTE STOP */
            modelBuilder.Entity<DeliveryRouteStop>(entity =>
            {
                entity.ToTable("delivery_route_stop");

                entity.HasKey(e => e.RouteStopId);

                entity.Property(e => e.RouteStopId).HasColumnName("route_stop_id");
                entity.Property(e => e.RouteId).HasColumnName("route_id").IsRequired();

                entity.Property(e => e.ClientId)
                      .HasColumnName("client_id")
                      .HasColumnType("varchar(20)")
                      .IsRequired();

                entity.Property(e => e.ClientNameSnapshot)
                      .HasColumnName("client_name_snapshot")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.AddressSnapshot)
                      .HasColumnName("address_snapshot")
                      .HasMaxLength(500);

                entity.Property(e => e.StopOrder).HasColumnName("stop_order").IsRequired();
                entity.Property(e => e.PlannedArrival).HasColumnName("planned_arrival");
                entity.Property(e => e.EstimatedTravelMinutesFromPrevious).HasColumnName("estimated_travel_minutes_from_previous");

                entity.Property(e => e.Latitude)
                      .HasColumnName("latitude")
                      .HasPrecision(9, 6);

                entity.Property(e => e.Longitude)
                      .HasColumnName("longitude")
                      .HasPrecision(9, 6);

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.Notes)
                      .HasColumnName("notes")
                      .HasMaxLength(500);

                entity.Property(e => e.DeliveryPhotoPath)
      .HasColumnName("delivery_photo_path")
      .HasMaxLength(500);

                entity.Property(e => e.DeliveryPhotoUploadedAt)
                      .HasColumnName("delivery_photo_uploaded_at");

                entity.Property(e => e.DeliveredAt)
                      .HasColumnName("delivered_at");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => new { e.RouteId, e.StopOrder }).IsUnique();

                entity.HasOne(e => e.Route)
                      .WithMany(r => r.Stops)
                      .HasForeignKey(e => e.RouteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            /* ROUTE TEMPLATE */
            modelBuilder.Entity<RouteTemplate>(entity =>
            {
                entity.ToTable("route_template");

                entity.HasKey(e => e.TemplateId);

                entity.Property(e => e.TemplateId).HasColumnName("template_id");
                entity.Property(e => e.TemplateName).HasColumnName("template_name").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.DefaultDriverUserId).HasColumnName("default_driver_user_id");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.DefaultDriverUser)
                      .WithMany()
                      .HasForeignKey(e => e.DefaultDriverUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* ROUTE TEMPLATE STOP */
            modelBuilder.Entity<RouteTemplateStop>(entity =>
            {
                entity.ToTable("route_template_stop");

                entity.HasKey(e => e.TemplateStopId);

                entity.Property(e => e.TemplateStopId).HasColumnName("template_stop_id");
                entity.Property(e => e.TemplateId).HasColumnName("template_id").IsRequired();

                entity.Property(e => e.ClientId)
                      .HasColumnName("client_id")
                      .HasColumnType("varchar(20)")
                      .IsRequired();

                entity.Property(e => e.ClientNameSnapshot)
                      .HasColumnName("client_name_snapshot")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.AddressSnapshot)
                      .HasColumnName("address_snapshot")
                      .HasMaxLength(500);

                entity.Property(e => e.StopOrder).HasColumnName("stop_order").IsRequired();

                entity.Property(e => e.Latitude)
                      .HasColumnName("latitude")
                      .HasPrecision(9, 6);

                entity.Property(e => e.Longitude)
                      .HasColumnName("longitude")
                      .HasPrecision(9, 6);

                entity.Property(e => e.Notes)
                      .HasColumnName("notes")
                      .HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => new { e.TemplateId, e.StopOrder }).IsUnique();

                entity.HasOne(e => e.Template)
                      .WithMany(t => t.Stops)
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* CREDIT ACCOUNT */
            modelBuilder.Entity<ClientAccount>(entity =>
            {
                entity.ToTable("client_accounts");

                entity.HasKey(e => e.ClientAccountId);

                entity.Property(e => e.ClientAccountId).HasColumnName("clientaccount_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ClientId)
                      .HasColumnName("client_id")
                      .HasColumnType("varchar(20)")
                      .IsRequired();

                entity.HasIndex(e => e.ClientAccountNumber)
                      .HasDatabaseName("clientaccount_number");

                entity.Property(e => e.ClientAccountNumber)
                      .HasColumnName("clientaccount_number")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.ClientAccountOpeningDate)
                      .HasColumnType("datetime2")
                      .HasColumnName("clientaccount_openingdate");

                entity.Property(e => e.ClientAccountCreditLimit)
                      .HasColumnName("clientaccount_creditlimit")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountInterestRate)
                      .HasColumnName("clientaccount_interestrate")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountCurrentBalance)
                      .HasColumnName("clientaccount_currentbalance")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountStatus)
                      .HasColumnName("clientaccount_accountstatus")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.ClientAccountConditions)
                      .HasColumnName("clientaccount_conditions")
                      .HasColumnType("varchar(max)");

                entity.HasOne(e => e.User)
                      .WithMany(ca => ca.ClientAccounts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                      .WithMany(ca => ca.ClientAccounts)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* CREDIT ACCOUNT MOVEMENTS */
            modelBuilder.Entity<ClientAccountMovement>(entity =>
            {
                entity.ToTable("client_accountmovements");

                entity.HasKey(e => e.ClientAccountMovementId);

                entity.Property(e => e.ClientAccountMovementId).HasColumnName("clientaccountmovement_id");

                entity.Property(e => e.ClientAccountId).HasColumnName("clientaccount_id");

                entity.Property(e => e.ClientAccountMovementDate)
                      .HasColumnType("datetime2")
                      .HasColumnName("clientaccountmovement_movementdate");

                entity.Property(e => e.ClientAccountMovementDescription)
                      .HasColumnName("clientaccountmovement_description")
                      .HasColumnType("varchar(max)");

                entity.Property(e => e.ClientAccountMovementAmount)
                      .HasColumnName("clientaccountmovement_amount")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountMovementOldBalance)
                      .HasColumnName("clientaccountmovement_oldbalance")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountMovementNewBalance)
                      .HasColumnName("clientaccountmovement_newbalance")
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ClientAccountMovementType)
                      .HasColumnName("clientaccountmovement_type")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasOne(e => e.ClientAccount)
                      .WithMany(cam => cam.Movements)
                      .HasForeignKey(e => e.ClientAccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* INVOICE (Electronic Invoice) */
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoice");

                entity.HasKey(e => e.InvoiceId);

                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
                entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");

                entity.Property(e => e.ElectronicInvoice)
                      .HasColumnName("electronic_invoice")
                      .HasMaxLength(500);

                entity.Property(e => e.InvoiceTotal)
                      .HasColumnName("invoice_total")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");

                // Hacienda fields
                entity.Property(e => e.Clave)
                      .HasColumnName("clave")
                      .HasMaxLength(50);

                entity.Property(e => e.ConsecutiveNumber)
                      .HasColumnName("consecutive_number")
                      .HasMaxLength(20);

                entity.Property(e => e.DocumentType)
                      .HasColumnName("document_type")
                      .HasMaxLength(2)
                      .HasDefaultValue("01");

                entity.Property(e => e.HaciendaStatus)
                      .HasColumnName("hacienda_status")
                      .HasMaxLength(20)
                      .HasDefaultValue("pending");

                entity.Property(e => e.XmlSigned)
                      .HasColumnName("xml_signed");

                entity.Property(e => e.XmlResponse)
                      .HasColumnName("xml_response");

                entity.Property(e => e.HaciendaMessage)
                      .HasColumnName("hacienda_message")
                      .HasMaxLength(500);

                entity.Property(e => e.EmissionDate).HasColumnName("emission_date");
                entity.Property(e => e.SentAt).HasColumnName("sent_at");
                entity.Property(e => e.ResponseAt).HasColumnName("response_at");

                entity.Property(e => e.CurrencyCode)
                      .HasColumnName("currency_code")
                      .HasMaxLength(3)
                      .HasDefaultValue("CRC");

                entity.Property(e => e.ExchangeRate)
                      .HasColumnName("exchange_rate")
                      .HasColumnType("decimal(18,5)")
                      .HasDefaultValue(1m);

                entity.Property(e => e.SaleCondition)
                      .HasColumnName("sale_condition")
                      .HasMaxLength(2)
                      .HasDefaultValue("01");

                entity.Property(e => e.PaymentMethodCode)
                      .HasColumnName("payment_method_code")
                      .HasMaxLength(2)
                      .HasDefaultValue("01");

                entity.Property(e => e.ReferenceDocumentClave)
                      .HasColumnName("reference_document_clave")
                      .HasMaxLength(50);

                entity.Property(e => e.ReferenceCode)
                      .HasColumnName("reference_code")
                      .HasMaxLength(2);

                entity.Property(e => e.ReferenceReason)
                      .HasColumnName("reference_reason")
                      .HasMaxLength(180);

                entity.Property(e => e.ActivityCode)
                      .HasColumnName("activity_code")
                      .HasMaxLength(6);

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.Clave)
                      .IsUnique()
                      .HasDatabaseName("uq_invoice_clave")
                      .HasFilter("[clave] IS NOT NULL");

                entity.HasIndex(e => e.HaciendaStatus)
                      .HasDatabaseName("ix_invoice_hacienda_status");

                entity.HasOne(e => e.Purchase)
                      .WithOne(p => p.Invoice)
                      .HasForeignKey<Invoice>(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* COMPANY CONFIG (Emisor) */
            modelBuilder.Entity<CompanyConfig>(entity =>
            {
                entity.ToTable("company_config");

                entity.HasKey(e => e.ConfigId);

                entity.Property(e => e.ConfigId).HasColumnName("config_id");

                entity.Property(e => e.CompanyName)
                      .HasColumnName("company_name")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.CommercialName)
                      .HasColumnName("commercial_name")
                      .HasMaxLength(100);

                entity.Property(e => e.IdType)
                      .HasColumnName("id_type")
                      .HasMaxLength(2)
                      .IsRequired();

                entity.Property(e => e.IdNumber)
                      .HasColumnName("id_number")
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.ActivityCode)
                      .HasColumnName("activity_code")
                      .HasMaxLength(6)
                      .IsRequired();

                entity.Property(e => e.ProvinceCode).HasColumnName("province_code");
                entity.Property(e => e.CantonCode).HasColumnName("canton_code");
                entity.Property(e => e.DistrictCode).HasColumnName("district_code");
                entity.Property(e => e.NeighborhoodCode).HasColumnName("neighborhood_code");

                entity.Property(e => e.OtherAddress)
                      .HasColumnName("other_address")
                      .HasMaxLength(250)
                      .IsRequired();

                entity.Property(e => e.PhoneCountryCode)
                      .HasColumnName("phone_country_code")
                      .HasMaxLength(3)
                      .HasDefaultValue("506");

                entity.Property(e => e.PhoneNumber)
                      .HasColumnName("phone_number")
                      .HasMaxLength(20);

                entity.Property(e => e.FaxCountryCode)
                      .HasColumnName("fax_country_code")
                      .HasMaxLength(3);

                entity.Property(e => e.FaxNumber)
                      .HasColumnName("fax_number")
                      .HasMaxLength(20);

                entity.Property(e => e.Email)
                      .HasColumnName("email")
                      .HasMaxLength(160)
                      .IsRequired();

                entity.Property(e => e.BranchNumber)
                      .HasColumnName("branch_number")
                      .HasMaxLength(3)
                      .HasDefaultValue("001");

                entity.Property(e => e.TerminalNumber)
                      .HasColumnName("terminal_number")
                      .HasMaxLength(5)
                      .HasDefaultValue("00001");

                entity.Property(e => e.Environment)
                      .HasColumnName("environment")
                      .HasMaxLength(10)
                      .HasDefaultValue("sandbox");

                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.Province)
                      .WithMany()
                      .HasForeignKey(e => e.ProvinceCode)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Canton)
                      .WithMany()
                      .HasForeignKey(e => e.CantonCode)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.District)
                      .WithMany()
                      .HasForeignKey(e => e.DistrictCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* HACIENDA CONSECUTIVE */
            modelBuilder.Entity<HaciendaConsecutive>(entity =>
            {
                entity.ToTable("hacienda_consecutive");

                entity.HasKey(e => e.ConsecutiveId);

                entity.Property(e => e.ConsecutiveId).HasColumnName("consecutive_id");

                entity.Property(e => e.DocumentType)
                      .HasColumnName("document_type")
                      .HasMaxLength(2)
                      .IsRequired();

                entity.Property(e => e.BranchNumber)
                      .HasColumnName("branch_number")
                      .HasMaxLength(3)
                      .HasDefaultValue("001");

                entity.Property(e => e.TerminalNumber)
                      .HasColumnName("terminal_number")
                      .HasMaxLength(5)
                      .HasDefaultValue("00001");

                entity.Property(e => e.LastNumber).HasColumnName("last_number");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => new { e.DocumentType, e.BranchNumber, e.TerminalNumber })
                      .IsUnique()
                      .HasDatabaseName("uq_hacienda_consecutive");
            });
        }
    }
}
