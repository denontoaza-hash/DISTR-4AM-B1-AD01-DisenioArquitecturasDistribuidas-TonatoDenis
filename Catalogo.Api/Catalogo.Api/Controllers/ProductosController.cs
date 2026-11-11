
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly Data.CatalogoDBContext _context;

        public ProductosController(Data.CatalogoDBContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/Productos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound();

            return Ok(producto);
        }

        // GET: api/Productos/por-categoria/1
        [HttpGet("por-categoria/{idCategoria:int}")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosPorCategoria(int idCategoria)
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.IdCategoria == idCategoria)
                .ToListAsync();

            return Ok(productos);
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> CrearProducto(Producto producto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (producto.IdCategoria <= 0 || producto.IdMarca <= 0)
                return BadRequest("La categoría y la marca deben ser válidas.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            if (string.IsNullOrWhiteSpace(producto.SKU))
                return BadRequest("El SKU es obligatorio.");

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            if (!categoriaExiste)
                return BadRequest("La categoría indicada no existe.");

            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == producto.IdMarca);

            if (!marcaExiste)
                return BadRequest("La marca indicada no existe.");

            var skuExiste = await _context.Productos
                .AnyAsync(p => p.SKU == producto.SKU);

            if (skuExiste)
                return Conflict("El SKU ya está registrado.");

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.IdProducto },
                producto);
        }

        // PUT: api/Productos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> ActualizarProducto(int id, Producto producto)
        {
            if (id != producto.IdProducto)
                return BadRequest("El ID de la URL no coincide con el producto.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (producto.IdCategoria <= 0 || producto.IdMarca <= 0)
                return BadRequest("La categoría y la marca deben ser válidas.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            if (string.IsNullOrWhiteSpace(producto.SKU))
                return BadRequest("El SKU es obligatorio.");

            var productoExiste = await _context.Productos.FindAsync(id);

            if (productoExiste == null)
                return NotFound();

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            if (!categoriaExiste)
                return BadRequest("La categoría indicada no existe.");

            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == producto.IdMarca);

            if (!marcaExiste)
                return BadRequest("La marca indicada no existe.");

            var skuExiste = await _context.Productos
                .AnyAsync(p => p.SKU == producto.SKU && p.IdProducto != id);

            if (skuExiste)
                return Conflict("El SKU ya está registrado en otro producto.");

            _context.Entry(productoExiste).CurrentValues.SetValues(producto);

            await _context.SaveChangesAsync();

            return Ok(producto);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
