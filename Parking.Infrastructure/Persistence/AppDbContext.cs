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

    public DbSet<RoleRow> Roles => Set<RoleRow>();
    public DbSet<EmployeeRow> Employees => Set<EmployeeRow>();
    public DbSet<ShiftRow> Shifts => Set<ShiftRow>();

    public DbSet<PlaceTypeRow> PlaceTypes => Set<PlaceTypeRow>();
    public DbSet<PlaceRow> Places => Set<PlaceRow>();
    public DbSet<PlaceTypeTariffRow> PlaceTypeTariffs => Set<PlaceTypeTariffRow>();

    public DbSet<TariffRow> Tariffs => Set<TariffRow>();
    public DbSet<TariffRateRow> TariffRates => Set<TariffRateRow>();

    public DbSet<ContractRow> Contracts => Set<ContractRow>();
    public DbSet<ContractPlaceRow> ContractPlaces => Set<ContractPlaceRow>();
    public DbSet<ContractVehicleRow> ContractVehicles => Set<ContractVehicleRow>();

    public DbSet<ServiceTypeRow> ServiceTypes => Set<ServiceTypeRow>();
    public DbSet<ContractServiceRow> ContractServices => Set<ContractServiceRow>();

    public DbSet<PaymentRow> Payments => Set<PaymentRow>();

    public DbSet<WatchlistTypeRow> WatchlistTypes => Set<WatchlistTypeRow>();
    public DbSet<WatchlistItemRow> Watchlist => Set<WatchlistItemRow>();

    public DbSet<NotificationRow> Notifications => Set<NotificationRow>();

    public DbSet<CameraTypeRow> CameraTypes => Set<CameraTypeRow>();
    public DbSet<CameraRow> Cameras => Set<CameraRow>();
    public DbSet<CameraZoneRow> CameraZones => Set<CameraZoneRow>();
    public DbSet<CameraZonePlaceRow> CameraZonePlaces => Set<CameraZonePlaceRow>();
    public DbSet<CameraEventRow> CameraEvents => Set<CameraEventRow>();

    public DbSet<BarrierRow> Barriers => Set<BarrierRow>();
    public DbSet<BarrierCommandRow> BarrierCommands => Set<BarrierCommandRow>();
    public DbSet<BarrierEventRow> BarrierEvents => Set<BarrierEventRow>();

    public DbSet<AuditLogRow> AuditLog => Set<AuditLogRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // passages
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

            e.HasOne(x => x.Camera)
                .WithMany(c => c.Passages)
                .HasForeignKey(x => x.CameraId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.OccurredAt);
            e.HasIndex(x => x.PlateNorm);
            e.HasIndex(x => x.CameraId).HasDatabaseName("ix_passages_camera");
        });

        // parking_sessions
        modelBuilder.Entity<ParkingSessionRow>(e =>
        {
            e.ToTable("parking_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.OpenedAt).IsRequired();

            e.HasIndex(x => new { x.PlateNorm, x.ClosedAt });
        });

        // owners
        modelBuilder.Entity<OwnerRow>(e =>
        {
            e.ToTable("owners");
            e.HasKey(x => x.Id);

            e.Property(x => x.Surname).HasMaxLength(128).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(128);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.ResidentialAddress).HasMaxLength(512);
            e.Property(x => x.JpegPath).HasMaxLength(512);

            e.Property(x => x.TelegramUsername).HasMaxLength(64);
            e.Property(x => x.TelegramEnabled).HasDefaultValue(false).IsRequired();
            e.Property(x => x.NotifyEntry).HasDefaultValue(false).IsRequired();
            e.Property(x => x.NotifyExit).HasDefaultValue(false).IsRequired();
            e.Property(x => x.NotifyPaymentDue).HasDefaultValue(false).IsRequired();
            e.Property(x => x.NotifyDebtWarning).HasDefaultValue(false).IsRequired();

            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.Phone).HasDatabaseName("ix_owners_phone");
        });

        // vehicles
        modelBuilder.Entity<VehicleRow>(e =>
        {
            e.ToTable("vehicles");
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

        // vehicle_owners
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

        // roles
        modelBuilder.Entity<RoleRow>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_roles_code");
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
            e.Property(x => x.ResidentialAddress).HasMaxLength(512);
            e.Property(x => x.JpegPath).HasMaxLength(512).HasDefaultValue("").IsRequired();

            e.Property(x => x.Login).HasMaxLength(64);
            e.Property(x => x.PasswordHash).HasMaxLength(255);
            e.Property(x => x.IsActive).IsRequired();

            e.HasOne(x => x.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Phone).HasDatabaseName("ix_employees_phone");
            e.HasIndex(x => x.RoleId).HasDatabaseName("ix_employees_role");

            e.HasIndex(x => x.Login)
                .IsUnique()
                .HasFilter("login IS NOT NULL")
                .HasDatabaseName("ux_employees_login");
        });

        // shifts
        modelBuilder.Entity<ShiftRow>(e =>
        {
            e.ToTable("shifts");
            e.HasKey(x => x.Id);

            e.Property(x => x.StartedAt).IsRequired();
            e.Property(x => x.OpeningCash).HasPrecision(12, 2).HasDefaultValue(0m).IsRequired();
            e.Property(x => x.ClosingCash).HasPrecision(12, 2);

            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.Shifts)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EmployeeId, x.StartedAt })
                .HasDatabaseName("ix_shifts_employee_started");
        });

        // place_types
        modelBuilder.Entity<PlaceTypeRow>(e =>
        {
            e.ToTable("place_types");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_place_types_code");
        });

        // places
        modelBuilder.Entity<PlaceRow>(e =>
        {
            e.ToTable("places");
            e.HasKey(x => x.Id);

            e.Property(x => x.PlaceNo).HasMaxLength(32).IsRequired();
            e.Property(x => x.Block).HasMaxLength(32);
            e.Property(x => x.IsActive).IsRequired();

            e.HasOne(x => x.PlaceType)
                .WithMany(pt => pt.Places)
                .HasForeignKey(x => x.PlaceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.PlaceNo)
                .IsUnique()
                .HasDatabaseName("ux_places_placeno");

            e.HasIndex(x => x.PlaceTypeId).HasDatabaseName("ix_places_place_type");
        });

        // tariffs
        modelBuilder.Entity<TariffRow>(e =>
        {
            e.ToTable("tariffs");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.BillingModel).HasMaxLength(32).IsRequired();
            e.Property(x => x.PaymentMode).HasMaxLength(16).HasDefaultValue("PREPAID").IsRequired();
            e.Property(x => x.GracePeriodDays).HasDefaultValue(20).IsRequired();
            e.Property(x => x.StampText).HasMaxLength(64);
            e.Property(x => x.StampColor).HasMaxLength(16);
            e.Property(x => x.CanPause).HasDefaultValue(false).IsRequired();
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

        // place_type_tariffs
        modelBuilder.Entity<PlaceTypeTariffRow>(e =>
        {
            e.ToTable("place_type_tariffs");
            e.HasKey(x => new { x.PlaceTypeId, x.TariffId });

            e.Property(x => x.IsDefault).HasDefaultValue(false).IsRequired();

            e.HasOne(x => x.PlaceType)
                .WithMany(pt => pt.PlaceTypeTariffs)
                .HasForeignKey(x => x.PlaceTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Tariff)
                .WithMany(t => t.PlaceTypeTariffs)
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // contracts
        modelBuilder.Entity<ContractRow>(e =>
        {
            e.ToTable("contracts");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasMaxLength(16).IsRequired();

            e.HasOne(x => x.CustomerOwner)
                .WithMany(o => o.Contracts)
                .HasForeignKey(x => x.CustomerOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.CustomerOwnerId).HasDatabaseName("ix_contracts_owner");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_contracts_status");
        });

        // contract_places
        modelBuilder.Entity<ContractPlaceRow>(e =>
        {
            e.ToTable("contract_places");
            e.HasKey(x => new { x.ContractId, x.PlaceId });

            e.Property(x => x.Status).HasMaxLength(16).IsRequired();
            e.Property(x => x.StartAt).IsRequired();
            e.Property(x => x.PauseBalanceDays).HasDefaultValue(0).IsRequired();

            e.HasOne(x => x.Contract)
                .WithMany(c => c.ContractPlaces)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Place)
                .WithMany(p => p.ContractPlaces)
                .HasForeignKey(x => x.PlaceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Tariff)
                .WithMany(t => t.ContractPlaces)
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LinkedContract)
                .WithMany(c => c.LinkedPlaces)
                .HasForeignKey(x => x.LinkedContractId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.PlaceId).HasDatabaseName("ix_contract_places_place");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_contract_places_status");
            e.HasIndex(x => x.LinkedContractId).HasDatabaseName("ix_contract_places_linked");
            e.HasIndex(x => x.TariffId).HasDatabaseName("ix_contract_places_tariff");
            e.HasIndex(x => x.PaidUntil).HasDatabaseName("ix_contract_places_paid_until");
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

        // service_types
        modelBuilder.Entity<ServiceTypeRow>(e =>
        {
            e.ToTable("service_types");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_service_types_code");
        });

        // contract_services
        modelBuilder.Entity<ContractServiceRow>(e =>
        {
            e.ToTable("contract_services");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasMaxLength(16).IsRequired();
            e.Property(x => x.StartAt).IsRequired();

            e.HasOne(x => x.Contract)
                .WithMany(c => c.ContractServices)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ServiceType)
                .WithMany(st => st.ContractServices)
                .HasForeignKey(x => x.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Tariff)
                .WithMany(t => t.ContractServices)
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ContractId).HasDatabaseName("ix_contract_services_contract");
            e.HasIndex(x => x.ServiceTypeId).HasDatabaseName("ix_contract_services_service_type");
            e.HasIndex(x => x.PaidUntil).HasDatabaseName("ix_contract_services_paid_until");
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

            e.HasOne(x => x.ContractPlace)
                .WithMany(cp => cp.Payments)
                .HasForeignKey(x => new { x.ContractId, x.PlaceId })
                .HasPrincipalKey(cp => new { cp.ContractId, cp.PlaceId })
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ContractService)
                .WithMany(cs => cs.Payments)
                .HasForeignKey(x => x.ContractServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.Payments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Shift)
                .WithMany(s => s.Payments)
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ContractId).HasDatabaseName("ix_payments_contract");
            e.HasIndex(x => x.PaidAt).HasDatabaseName("ix_payments_paidat");
            e.HasIndex(x => new { x.ContractId, x.PlaceId }).HasDatabaseName("ix_payments_contract_place");
            e.HasIndex(x => x.ContractServiceId).HasDatabaseName("ix_payments_contract_service");
            e.HasIndex(x => x.ShiftId).HasDatabaseName("ix_payments_shift");
        });

        // watchlist_types
        modelBuilder.Entity<WatchlistTypeRow>(e =>
        {
            e.ToTable("watchlist_types");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();

            e.Property(x => x.AllowUnrestrictedAccess).HasDefaultValue(false).IsRequired();
            e.Property(x => x.UnlimitedStay).HasDefaultValue(false).IsRequired();

            e.Property(x => x.StampText).HasMaxLength(64);
            e.Property(x => x.StampColor).HasMaxLength(16);
            e.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_watchlist_types_code");
        });

        // watchlist
        modelBuilder.Entity<WatchlistItemRow>(e =>
        {
            e.ToTable("watchlist");
            e.HasKey(x => x.Id);

            e.Property(x => x.PlateNorm).HasMaxLength(32).IsRequired();
            e.Property(x => x.IsActive).IsRequired();

            e.HasOne(x => x.WatchlistType)
                .WithMany(wt => wt.WatchlistItems)
                .HasForeignKey(x => x.WatchlistTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.PlateNorm).HasDatabaseName("ix_watchlist_plate");
            e.HasIndex(x => x.WatchlistTypeId).HasDatabaseName("ix_watchlist_type");
        });

        // notifications
        modelBuilder.Entity<NotificationRow>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);

            e.Property(x => x.NotificationType).HasMaxLength(32).IsRequired();
            e.Property(x => x.MessageText).IsRequired();
            e.Property(x => x.Status).HasMaxLength(16).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();

            e.HasOne(x => x.Owner)
                .WithMany(o => o.Notifications)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Vehicle)
                .WithMany()
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Passage)
                .WithMany()
                .HasForeignKey(x => x.PassageId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.OwnerId).HasDatabaseName("ix_notifications_owner");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_notifications_status");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_notifications_created_at");
            e.HasIndex(x => x.PassageId).HasDatabaseName("ix_notifications_passage");
        });

        // camera_types
        modelBuilder.Entity<CameraTypeRow>(e =>
        {
            e.ToTable("camera_types");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_camera_types_code");
        });

        // cameras
        modelBuilder.Entity<CameraRow>(e =>
        {
            e.ToTable("cameras");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Address).HasMaxLength(255);
            e.Property(x => x.Location).HasMaxLength(255);
            e.Property(x => x.IsActive).IsRequired();

            e.HasOne(x => x.CameraType)
                .WithMany(ct => ct.Cameras)
                .HasForeignKey(x => x.CameraTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.CameraTypeId, x.IsActive })
                .HasDatabaseName("ix_cameras_type_active");
        });

        // camera_zones
        modelBuilder.Entity<CameraZoneRow>(e =>
        {
            e.ToTable("camera_zones");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();

            e.HasOne(x => x.Camera)
                .WithMany(c => c.CameraZones)
                .HasForeignKey(x => x.CameraId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CameraId, x.Code })
                .IsUnique()
                .HasDatabaseName("ux_camera_zones_camera_code");
        });

        // camera_zone_places
        modelBuilder.Entity<CameraZonePlaceRow>(e =>
        {
            e.ToTable("camera_zone_places");
            e.HasKey(x => new { x.CameraZoneId, x.PlaceId });

            e.HasOne(x => x.CameraZone)
                .WithMany(z => z.CameraZonePlaces)
                .HasForeignKey(x => x.CameraZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Place)
                .WithMany(p => p.CameraZonePlaces)
                .HasForeignKey(x => x.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // camera_events
        modelBuilder.Entity<CameraEventRow>(e =>
        {
            e.ToTable("camera_events");
            e.HasKey(x => x.Id);

            e.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.JpegPath).HasMaxLength(512);
            e.Property(x => x.RawData).HasColumnType("jsonb");

            e.HasOne(x => x.Camera)
                .WithMany(c => c.CameraEvents)
                .HasForeignKey(x => x.CameraId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CameraZone)
                .WithMany(z => z.CameraEvents)
                .HasForeignKey(x => x.CameraZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.CameraId, x.OccurredAt })
                .HasDatabaseName("ix_camera_events_camera_occurred");
            e.HasIndex(x => x.CameraZoneId).HasDatabaseName("ix_camera_events_zone");
            e.HasIndex(x => x.EventType).HasDatabaseName("ix_camera_events_type");
        });

        // barriers
        modelBuilder.Entity<BarrierRow>(e =>
        {
            e.ToTable("barriers");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Address).HasMaxLength(255);
            e.Property(x => x.ControllerType).HasMaxLength(32);
            e.Property(x => x.IsActive).IsRequired();
        });

        // barrier_commands
        modelBuilder.Entity<BarrierCommandRow>(e =>
        {
            e.ToTable("barrier_commands");
            e.HasKey(x => x.Id);

            e.Property(x => x.CommandType).HasMaxLength(16).IsRequired();
            e.Property(x => x.Source).HasMaxLength(16).IsRequired();
            e.Property(x => x.RequestedAt).IsRequired();
            e.Property(x => x.Status).HasMaxLength(16).IsRequired();

            e.HasOne(x => x.Barrier)
                .WithMany(b => b.Commands)
                .HasForeignKey(x => x.BarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.BarrierCommands)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Passage)
                .WithMany()
                .HasForeignKey(x => x.PassageId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.BarrierId).HasDatabaseName("ix_barrier_commands_barrier");
            e.HasIndex(x => x.RequestedAt).HasDatabaseName("ix_barrier_commands_requested");
            e.HasIndex(x => x.PassageId).HasDatabaseName("ix_barrier_commands_passage");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_barrier_commands_status");
        });

        // barrier_events
        modelBuilder.Entity<BarrierEventRow>(e =>
        {
            e.ToTable("barrier_events");
            e.HasKey(x => x.Id);

            e.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.RawData).HasColumnType("jsonb");

            e.HasOne(x => x.Barrier)
                .WithMany(b => b.Events)
                .HasForeignKey(x => x.BarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BarrierCommand)
                .WithMany(c => c.Events)
                .HasForeignKey(x => x.BarrierCommandId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.BarrierId, x.OccurredAt })
                .HasDatabaseName("ix_barrier_events_barrier_occurred");
            e.HasIndex(x => x.BarrierCommandId).HasDatabaseName("ix_barrier_events_command");
            e.HasIndex(x => x.EventType).HasDatabaseName("ix_barrier_events_type");
        });

        // audit_log
        modelBuilder.Entity<AuditLogRow>(e =>
        {
            e.ToTable("audit_log");
            e.HasKey(x => x.Id);

            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.ActionCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            e.Property(x => x.OldData).HasColumnType("jsonb");
            e.Property(x => x.NewData).HasColumnType("jsonb");

            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.AuditLogEntries)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.OccurredAt).HasDatabaseName("ix_audit_log_occurred");
            e.HasIndex(x => x.EmployeeId).HasDatabaseName("ix_audit_log_employee");
            e.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("ix_audit_log_entity");
            e.HasIndex(x => x.ActionCode).HasDatabaseName("ix_audit_log_action");
        });
    }
}
