
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models
{
    [Table("Pedido")]
    public class Pedido
    {
        [Key]
        [Column("IdPedido")]
        public int IdPedido { get; set; }

        [Required]
        [Column("IdCliente")]
        public int IdCliente { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue)]
        [Column("Total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";

        [ForeignKey(nameof(IdCliente))]
        public Cliente? ClienteRelacion { get; set; }
    }
}
