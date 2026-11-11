
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("ReservaStock")]
    public class ReservaStock
    {
        [Key]
        [Column("IdReserva")]
        public int IdReserva { get; set; }

        [Required]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("IdBodega")]
        public int IdBodega { get; set; }

        [Range(1, int.MaxValue)]
        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";

        [ForeignKey(nameof(IdBodega))]
        public Bodega? BodegaRelacion { get; set; }
    }
}
