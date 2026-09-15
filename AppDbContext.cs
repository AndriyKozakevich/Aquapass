using Microsoft.EntityFrameworkCore;
using AquaPass.Models;
using AquaPass.Enums;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AquaPass
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Sunbed> Sunbeds { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Вказуємо PostgreSQL генерувати UUID за замовчуванням
            modelBuilder.Entity<Staff>()
                .Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Sunbed>()
                .Property(s => s.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Налаштування таблиці Zone
            modelBuilder.Entity<Zone>(entity =>
            {
                entity.HasKey(z => z.Id);
                entity.Property(z => z.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // Налаштування таблиці Tariff
            modelBuilder.Entity<Tariff>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(t => t.Price)
                      .HasPrecision(18, 2);

                entity.Property(t => t.ServiceType)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                entity.Property(t => t.DayType)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.HasOne(t => t.Zone)
                      .WithMany(z => z.Tariffs)
                      .HasForeignKey(t => t.ZoneId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Фіксовані статичні ідентифікатори для Seed Data
            var vipZoneId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var standardZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<Zone>().HasData(
                new Zone
                {
                    Id = vipZoneId,
                    Name = "VIP-зона"
                },
                new Zone
                {
                    Id = standardZoneId,
                    Name = "Стандартна зона"
                }
            );

            modelBuilder.Entity<Tariff>().HasData(
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                    ZoneId = standardZoneId,
                    Name = "Дорослий вхідний квиток(Вихідний)",
                    Price = 750.00m,
                    ServiceType = ServiceType.EntranceTicketAdult,
                    DayType = DayType.Weekend
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                    ZoneId = standardZoneId,
                    Name = "Дитячий вхідний квиток(Вихідний)",
                    Price = 400.00m,
                    ServiceType = ServiceType.EntranceTicketChild,
                    DayType = DayType.Weekend
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                    ZoneId = standardZoneId,
                    Name = "Дорослий вхідний квиток(Будний)",
                    Price = 550.00m,
                    ServiceType = ServiceType.EntranceTicketAdult,
                    DayType = DayType.Weekday
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                    ZoneId = standardZoneId,
                    Name = "Дитячий вхідний квиток(Будний)",
                    Price = 350.00m,
                    ServiceType = ServiceType.EntranceTicketChild,
                    DayType = DayType.Weekday
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"),
                    ZoneId = vipZoneId,
                    Name = "Бунгало(Будний)",
                    Price = 600.00m,
                    ServiceType = ServiceType.Bungalow,
                    DayType = DayType.Weekday
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000006"),
                    ZoneId = vipZoneId,
                    Name = "Бунгало(Вихідний)",
                    Price = 800.00m,
                    ServiceType = ServiceType.Bungalow,
                    DayType = DayType.Weekend
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000007"),
                    ZoneId = standardZoneId,
                    Name = "Лежак(Вихідний)",
                    Price = 200.00m,
                    ServiceType = ServiceType.Sunbed,
                    DayType = DayType.Weekend
                },
                new Tariff
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000008"),
                    ZoneId = standardZoneId,
                    Name = "Лежак(Будний)",
                    Price = 150.00m,
                    ServiceType = ServiceType.Sunbed,
                    DayType = DayType.Weekday
                }
            );
        }
    }
}