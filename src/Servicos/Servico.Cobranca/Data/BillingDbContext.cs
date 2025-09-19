using Billing.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing.Service.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Fatura> Faturas => Set<Fatura>();
    public DbSet<ItemFatura> ItensFatura => Set<ItemFatura>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroFatura).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ClienteNome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ClienteEmail).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MetodoPagamento).HasMaxLength(100);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);
            
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DataEmissao);
            entity.HasIndex(e => e.NumeroFatura).IsUnique();
            
            // Configuração do relacionamento com ItemFatura
            entity.HasMany(e => e.Itens)
                  .WithOne(i => i.Fatura)
                  .HasForeignKey(i => i.FaturaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemFatura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoVaga).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ValorHora).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Descricao).HasMaxLength(500).IsRequired();
            
            entity.HasIndex(e => e.FaturaId);
            entity.HasIndex(e => e.OcupacaoId);
            entity.HasIndex(e => e.VagaId);
            entity.HasIndex(e => e.DataEntrada);
        });

        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetodoPagamento).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TransacaoId).HasMaxLength(100);
            
            entity.HasIndex(e => e.FaturaId);
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DataProcessamento);
        });

        base.OnModelCreating(modelBuilder);
    }
}