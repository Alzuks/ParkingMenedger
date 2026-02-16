using Microsoft.EntityFrameworkCore;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PassageRow> Passages => Set<PassageRow>();
    public DbSet<ParkingSessionRow> ParkingSessions => Set<ParkingSessionRow>();
    public DbSet<OwnerRow> Owners => Set<OwnerRow>();
    public DbSet<VehicleRow> Vehicles => Set<VehicleRow>();
    public DbSet<VehicleOwnerRow> VehicleOwners => Set<VehicleOwnerRow>();
    public DbSet<EmployeeRow> Employees => Set<EmployeeRow>();
    public DbSet<PlaceRow> Places => Set<PlaceRow>();
    public DbSet<TariffRow> Tariffs => Set<TariffRow>();
    public DbSet<TariffRateRow> TariffRates => Set<TariffRateRow>();
    public DbSet<ContractRow> Contracts => Set<ContractRow>();
    public DbSet<ContractPlaceRow> ContractPlaces => Set<ContractPlaceRow>();
    public DbSet<ContractVehicleRow> ContractVehicles => Set<ContractVehicleRow>();
    public DbSet<PaymentRow> Payments => Set<PaymentRow>();
    public DbSet<WatchlistItemRow> Watchlist => Set<WatchlistItemRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PassageRow>(e =>
        {
            e.ToTable("passages");
            e.HasKey(x => x.Id);
            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.PlateRaw).HasMaxLength(32).IsRequired();
            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.Direction).IsRequired();
            e.Property(x => x.JpegPath).HasMaxLength(512);
            e.Property(x => x.Confidence).HasColumnName("confidence");

            e.HasIndex(x => x.OccurredAt);
            e.HasIndex(x => x.PlateNorm);
        });

        modelBuilder.Entity<ParkingSessionRow>(e =>
        {
            e.ToTable("parking_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.OpenedAt).IsRequired();
            e.Property(x => x.ClosedAt);

            // быстро найти активную сессию по номеру
            e.HasIndex(x => new { x.PlateNorm, x.ClosedAt });
        });
        // owners

        modelBuilder.Entity<OwnerRow>(e =>
        {
            e.ToTable("owners");
            e.Property(x => x.Id).HasColumnName("id");
            e.HasKey(x => x.Id);
            e.Property(x => x.Surname).HasMaxLength(128).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(128);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.Phone).HasDatabaseName("ix_owners_phone");
        });

        // vehicles

        modelBuilder.Entity<VehicleRow>(e =>
        {
            e.ToTable("vehicles");
            e.Property(x => x.Id).HasColumnName("id");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.PlateRaw).HasMaxLength(32);
            e.Property(x => x.Brand).HasMaxLength(64);
            e.Property(x => x.Model).HasMaxLength(64);
            e.Property(x => x.Color).HasMaxLength(32);
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.PlateNorm)
                .IsUnique()
                .HasDatabaseName("ux_vehicles_platenorm");
        });

        // vehicle_owners  (M:N owners <-> vehicles)

        modelBuilder.Entity<VehicleOwnerRow>(e =>
        {
            e.ToTable("vehicle_owners");
            e.HasKey(x => new { x.VehicleId, x.OwnerId });

            e.Property(x => x.IsPayer).IsRequired();

            e.HasIndex(x => x.OwnerId).HasDatabaseName("ix_vehicle_owners_owner");

            e.HasOne(x => x.Vehicle)
                .WithMany(v => v.VehicleOwners)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Owner)
                .WithMany(o => o.VehicleOwners)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // employees

        modelBuilder.Entity<EmployeeRow>(e =>
        {
            e.ToTable("employees");
            e.HasKey(x => x.Id);
            e.Property(x => x.Surname).HasMaxLength(128).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(128);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.Role).HasMaxLength(64);
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.Phone).HasDatabaseName("ix_employees_phone");
        });

        // places

        modelBuilder.Entity<PlaceRow>(e =>
        {
            e.ToTable("places");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlaceNo).HasMaxLength(32).IsRequired();
            e.Property(x => x.Block).HasMaxLength(32);
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.PlaceNo)
                .IsUnique()
                .HasDatabaseName("ux_places_placeno");
        });

        // tariffs

        modelBuilder.Entity<TariffRow>(e =>
        {
            e.ToTable("tariffs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.BillingModel).HasMaxLength(32).IsRequired();
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.BillingModel).HasDatabaseName("ix_tariffs_billingmodel");
        });

        // tariff_rates

        modelBuilder.Entity<TariffRateRow>(e =>
        {
            e.ToTable("tariff_rates");
            e.HasKey(x => x.Id);
            e.Property(x => x.ValidFrom).IsRequired();
            e.Property(x => x.Cost).HasPrecision(12, 2).IsRequired();

            e.HasOne(x => x.Tariff)
                .WithMany(t => t.Rates)
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.TariffId, x.ValidFrom, x.ValidTo })
                .HasDatabaseName("ix_tariff_rates_period");
        });

        // contracts

        modelBuilder.Entity<ContractRow>(e =>
        {
            e.ToTable("contracts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(16).IsRequired();
            e.Property(x => x.StartAt).IsRequired();
            e.Property(x => x.PauseBalanceDays).IsRequired();

            e.HasOne(x => x.CustomerOwner)
                .WithMany(o => o.Contracts)
                .HasForeignKey(x => x.CustomerOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Tariff)
                .WithMany(t => t.Contracts)
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.CustomerOwnerId).HasDatabaseName("ix_contracts_owner");
            e.HasIndex(x => x.PaidUntil).HasDatabaseName("ix_contracts_paiduntil");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_contracts_status");
        });

        // contract_places

        modelBuilder.Entity<ContractPlaceRow>(e =>
        {
            e.ToTable("contract_places");
            e.HasKey(x => new { x.ContractId, x.PlaceId });

            e.Property(x => x.Status).HasMaxLength(16).IsRequired();

            e.HasOne(x => x.Contract)
                .WithMany(c => c.ContractPlaces)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Place)
                .WithMany(p => p.ContractPlaces)
                .HasForeignKey(x => x.PlaceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LinkedContract)
                .WithMany(c => c.LinkedPlaces)
                .HasForeignKey(x => x.LinkedContractId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.PlaceId).HasDatabaseName("ix_contract_places_place");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_contract_places_status");
            e.HasIndex(x => x.LinkedContractId).HasDatabaseName("ix_contract_places_linked");
        });

        // contract_vehicles

        modelBuilder.Entity<ContractVehicleRow>(e =>
        {
            e.ToTable("contract_vehicles");
            e.HasKey(x => new { x.ContractId, x.VehicleId });

            e.HasOne(x => x.Contract)
                .WithMany(c => c.ContractVehicles)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Vehicle)
                .WithMany(v => v.ContractVehicles)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.VehicleId).HasDatabaseName("ix_contract_vehicles_vehicle");
        });

        // payments

        modelBuilder.Entity<PaymentRow>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2).IsRequired();
            e.Property(x => x.PaidAt).IsRequired();
            e.Property(x => x.Method).HasMaxLength(16).IsRequired();
            e.Property(x => x.ReceiptNo).HasMaxLength(64);
            e.Property(x => x.EvidenceRef).HasMaxLength(512);

            e.HasOne(x => x.Contract)
                .WithMany(c => c.Payments)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.Payments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ContractId).HasDatabaseName("ix_payments_contract");
            e.HasIndex(x => x.PaidAt).HasDatabaseName("ix_payments_paidat");
        });

        // watchlist

        modelBuilder.Entity<WatchlistItemRow>(e =>
        {
            e.ToTable("watchlist");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.Type).HasMaxLength(16).IsRequired();
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.PlateNorm).HasDatabaseName("ix_watchlist_plate");
            e.HasIndex(x => x.Type).HasDatabaseName("ix_watchlist_type");
        });

    }
}
