
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Models;

namespace Pedidos.Api.Data
{
    public class PedidosDBContext : DbContext
    {
        public PedidosDBContext(DbContextOptions<PedidosDBContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<DetallePedido> DetallesPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación entre Cliente y Pedido
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.ClienteRelacion)
                .WithMany()
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación entre Pedido y DetallePedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.PedidoRelacion)
                .WithMany()
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
