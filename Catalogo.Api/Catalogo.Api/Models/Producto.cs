
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api.Models
{
    [Table("Producto")]
    public class Producto
    {
        [Key]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("IdCategoria")]
        public int IdCategoria { get; set; }

        [Required]
        [Column("IdMarca")]
        public int IdMarca { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("SKU")]
        public string SKU { get; set; } = string.Empty;

        [Column("Precio")]
        public decimal Precio { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; } = true;
    }
}
