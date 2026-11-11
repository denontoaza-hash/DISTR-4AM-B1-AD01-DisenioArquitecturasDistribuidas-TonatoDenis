
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BodegasController : ControllerBase
    {
        private readonly InventarioDBContext _context;

        public BodegasController(InventarioDBContext context)
        {
            _context = context;
        }

        // GET: api/Bodegas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bodega>>> ObtenerBodegas()
        {
            return await _context.Bodegas
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Bodegas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bodega>> ObtenerBodega(int id)
        {
            var bodega = await _context.Bodegas
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.IdBodega == id);

            if (bodega == null)
            {
                return NotFound("Bodega no encontrada.");
            }

            return Ok(bodega);
        }

        // POST: api/Bodegas
        [HttpPost]
        public async Task<ActionResult<Bodega>> AgregarBodega(Bodega bodega)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Bodegas.Add(bodega);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerBodega),
                new { id = bodega.IdBodega },
                bodega);
        }

        // PUT: api/Bodegas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarBodega(
            int id, Bodega bodega)
        {
            if (id != bodega.IdBodega)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.Bodegas.FindAsync(id);

            if (existente == null)
            {
                return NotFound("Bodega no encontrada.");
            }

            _context.Entry(existente).CurrentValues.SetValues(bodega);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Bodegas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarBodega(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);

            if (bodega == null)
            {
                return NotFound("Bodega no encontrada.");
            }

            _context.Bodegas.Remove(bodega);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
