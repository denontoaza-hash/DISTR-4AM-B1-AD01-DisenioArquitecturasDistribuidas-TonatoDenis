
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly Data.CatalogoDBContext _context;

        public CategoriasController(Data.CatalogoDBContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/Categorias/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> CrearCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.IdCategoria },
                categoria);
        }

        // PUT: api/Categorias/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> ActualizarCategoria(int id, Categoria categoria)
        {
            if (id != categoria.IdCategoria)
                return BadRequest();

            var categoriaExiste = await _context.Categorias
                .FindAsync(id);

            if (categoriaExiste == null)
                return NotFound();

            _context.Entry(categoriaExiste).CurrentValues.SetValues(categoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
