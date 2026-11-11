
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("Existencia")]
    public class Existencia
    {
        [Key]
        [Column("IdExistencia")]
        public int IdExistencia { get; set; }

        [Required]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("IdBodega")]
        public int IdBodega { get; set; }

        [Range(0, int.MaxValue)]
        [Column("Stock")]
        public int Stock { get; set; }

        [ForeignKey(nameof(IdBodega))]
        public Bodega? BodegaRelacion { get; set; }
    }
}
