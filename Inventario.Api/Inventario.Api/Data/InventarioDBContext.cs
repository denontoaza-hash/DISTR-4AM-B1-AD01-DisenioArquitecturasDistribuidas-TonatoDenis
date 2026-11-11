
using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data
{
    public class InventarioDBContext : DbContext
    {
        public InventarioDBContext(DbContextOptions<InventarioDBContext> options)
            : base(options)
        {
        }

        public DbSet<Bodega> Bodegas { get; set; }

        public DbSet<Existencia> Existencias { get; set; }

        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        public DbSet<ReservaStock> ReservasStock { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Existencia>()
                .HasOne(e => e.BodegaRelacion)
                .WithMany()
                .HasForeignKey(e => e.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.BodegaRelacion)
                .WithMany()
                .HasForeignKey(m => m.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservaStock>()
                .HasOne(r => r.BodegaRelacion)
                .WithMany()
                .HasForeignKey(r => r.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Existencia>()
                .HasIndex(e => new { e.IdProducto, e.IdBodega })
                .IsUnique();
        }
    }
}
