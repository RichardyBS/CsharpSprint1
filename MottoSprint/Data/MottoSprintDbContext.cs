using Microsoft.EntityFrameworkCore;
using MottoSprint.Models;

namespace MottoSprint.Data;

public class MottoSprintDbContext : DbContext
{
    public MottoSprintDbContext(DbContextOptions<MottoSprintDbContext> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
    
    // Novos DbSets para sistema de notificação de motos
    public DbSet<MotoNotification> MotoNotifications => Set<MotoNotification>();
    public DbSet<FilaEntrada> FilaEntradas => Set<FilaEntrada>();
    public DbSet<FilaSaida> FilaSaidas => Set<FilaSaida>();
    public DbSet<LogMovimentacao> LogsMovimentacao => Set<LogMovimentacao>();
    public DbSet<ConfiguracaoNotificacao> ConfiguracoesNotificacao => Set<ConfiguracaoNotificacao>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Moto> Motos => Set<Moto>();
    public DbSet<Vaga> Vagas => Set<Vaga>();
    public DbSet<EstatisticasEstacionamento> EstatisticasEstacionamento => Set<EstatisticasEstacionamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Todo configuration
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ParkingSpot configuration
        modelBuilder.Entity<ParkingSpot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SpotNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.SpotNumber).IsUnique();
            entity.HasIndex(e => e.IsOccupied);
        });

        base.OnModelCreating(modelBuilder);
    }
}