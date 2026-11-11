
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("MovimientoInventario")]
    public class MovimientoInventario
    {
        [Key]
        [Column("IdMovimiento")]
        public int IdMovimiento { get; set; }

        [Required]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("IdBodega")]
        public int IdBodega { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [ForeignKey(nameof(IdBodega))]
        public Bodega? BodegaRelacion { get; set; }
    }
}
