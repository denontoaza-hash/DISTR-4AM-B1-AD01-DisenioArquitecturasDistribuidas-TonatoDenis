
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {
        private readonly Data.CatalogoDBContext _context;

        public MarcasController(Data.CatalogoDBContext context)
        {
            _context = context;
        }

        // GET: api/Marcas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            var marcas = await _context.Marcas
                .AsNoTracking()
                .ToListAsync();

            return Ok(marcas);
        }

        // GET: api/Marcas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Marca>> GetMarca(int id)
        {
            var marca = await _context.Marcas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null)
                return NotFound();

            return Ok(marca);
        }

        // POST: api/Marcas
        [HttpPost]
        public async Task<ActionResult<Marca>> CrearMarca(Marca marca)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMarca),
                new { id = marca.IdMarca },
                marca);
        }

        // PUT: api/Marcas/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> ActualizarMarca(int id, Marca marca)
        {
            if (id != marca.IdMarca)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var marcaExiste = await _context.Marcas.FindAsync(id);

            if (marcaExiste == null)
                return NotFound();

            _context.Entry(marcaExiste).CurrentValues.SetValues(marca);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Marcas/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
                return NotFound();

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
