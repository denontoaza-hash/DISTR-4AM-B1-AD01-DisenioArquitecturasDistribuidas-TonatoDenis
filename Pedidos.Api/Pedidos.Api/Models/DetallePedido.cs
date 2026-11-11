using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models
{
    [Table("DetallePedido")]
    public class DetallePedido
    {
        [Key]
        [Column("IdDetalle")]
        public int IdDetalle { get; set; }

        [Required]
        [Column("IdPedido")]
        public int IdPedido { get; set; }

        [Required]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue)]
        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue)]
        [Column("PrecioUnitario", TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [ForeignKey(nameof(IdPedido))]
        public Pedido? PedidoRelacion { get; set; }
    }
}