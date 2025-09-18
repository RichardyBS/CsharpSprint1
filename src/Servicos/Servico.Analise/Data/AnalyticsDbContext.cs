using Analytics.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Service.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<OcupacaoVaga> OcupacoesVagas => Set<OcupacaoVaga>();
    public DbSet<MetricaDiaria> MetricasDiarias => Set<MetricaDiaria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OcupacaoVaga>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoVaga).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ClienteNome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ClienteCpf).HasMaxLength(14).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ValorCobrado).HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.VagaId);
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.DataEntrada);
            entity.HasIndex(e => new { e.DataEntrada, e.Status });
        });

        modelBuilder.Entity<MetricaDiaria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceitaTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TicketMedio).HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.Data).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}