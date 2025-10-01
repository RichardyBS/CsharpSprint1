using Microsoft.EntityFrameworkCore;
using MottoSprint.Models;

namespace MottoSprint.Data;

public class MottoSprintDbContext : DbContext
{
    public MottoSprintDbContext(DbContextOptions<MottoSprintDbContext> options) : base(options) { }


    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
    
    // Novos DbSets para sistema de notificação de motos
    public DbSet<MotoNotification> MotoNotifications => Set<MotoNotification>();
    public DbSet<FilaEntrada> FilaEntradas => Set<FilaEntrada>();
    public DbSet<FilaSaida> FilaSaidas => Set<FilaSaida>();
    public DbSet<LogMovimentacao> LogsMovimentacao => Set<LogMovimentacao>();
    public DbSet<ConfiguracaoNotificacao> ConfiguracoesNotificacao => Set<ConfiguracaoNotificacao>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<MotoDb> Motos => Set<MotoDb>();
    public DbSet<VagaDb> Vagas => Set<VagaDb>();
    public DbSet<EstatisticasEstacionamento> EstatisticasEstacionamento => Set<EstatisticasEstacionamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.PlacaMoto).HasMaxLength(20);
            entity.Property(e => e.NotificationType).HasMaxLength(50).HasDefaultValue("GENERAL");
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("NORMAL");
            
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.PlacaMoto);
            entity.HasIndex(e => e.VagaId);
            entity.HasIndex(e => e.NotificationType);
            entity.HasIndex(e => e.Priority);
        });

        // ParkingSpot configuration
        modelBuilder.Entity<ParkingSpot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SpotNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasIndex(e => e.SpotNumber).IsUnique();
            entity.HasIndex(e => e.IsOccupied);
        });

        base.OnModelCreating(modelBuilder);
    }
}