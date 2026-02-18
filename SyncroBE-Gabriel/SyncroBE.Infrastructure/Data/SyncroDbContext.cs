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

                entity.Property(e => e.ProductQuantity).HasColumnName("product_quantity");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
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
                entity.Property(e => e.ClientPurchases).HasColumnName("client_purchases");
                entity.Property(e => e.ClientLastPurchase).HasColumnName("client_last_purchase");

                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.ProvinceCode).HasColumnName("province_code");
                entity.Property(e => e.CantonCode).HasColumnName("canton_code");
                entity.Property(e => e.DistrictCode).HasColumnName("district_code");
                entity.Property(e => e.ExactAddress).HasColumnName("exact_address");

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
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");          // ✅ tu tabla lo tiene
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
                entity.Property(e => e.MustChangePassword).HasColumnName("MustChangePassword"); // ✅ tu tabla lo tiene
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

                entity.HasOne(e => e.User)
                      .WithMany(q => q.Quotes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                      .WithMany(q => q.Quotes)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
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
        }
    }
}
