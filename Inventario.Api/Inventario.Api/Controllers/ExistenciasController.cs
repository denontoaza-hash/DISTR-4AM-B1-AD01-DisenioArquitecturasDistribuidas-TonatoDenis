
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExistenciasController : ControllerBase
    {
        private readonly InventarioDBContext _context;
        private readonly HttpClient _httpClient;

        public ExistenciasController(
            InventarioDBContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();

            _httpClient.BaseAddress = new Uri("http://localhost:5145/");
        }

        // GET: api/Existencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Existencia>>> ObtenerExistencias()
        {
            return await _context.Existencias
                .Include(e => e.BodegaRelacion)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Existencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Existencia>> ObtenerExistencia(int id)
        {
            var existencia = await _context.Existencias
                .Include(e => e.BodegaRelacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdExistencia == id);

            if (existencia == null)
            {
                return NotFound("Existencia no encontrada.");
            }

            return Ok(existencia);
        }

        // GET: api/Existencias/por-producto/5
        [HttpGet("por-producto/{idProducto}")]
        public async Task<ActionResult<IEnumerable<Existencia>>> ObtenerPorProducto(int idProducto)
        {
            var existencias = await _context.Existencias
                .Include(e => e.BodegaRelacion)
                .AsNoTracking()
                .Where(e => e.IdProducto == idProducto)
                .ToListAsync();

            return Ok(existencias);
        }

        // POST: api/Existencias
        [HttpPost]
        public async Task<ActionResult<Existencia>> AgregarExistencia(Existencia existencia)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Consultar Catalogo.Api para verificar el producto
            try
            {
                var respuesta = await _httpClient.GetAsync(
                    $"api/Productos/{existencia.IdProducto}");

                if (respuesta.StatusCode == HttpStatusCode.NotFound)
                {
                    return BadRequest("El producto no existe en Catalogo.Api.");
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    return StatusCode(503,
                        "Catalogo.Api no pudo validar el producto.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(503,
                    "No se pudo establecer conexión con Catalogo.Api.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503,
                    "La consulta a Catalogo.Api excedió el tiempo de espera.");
            }

            // Verificar que la bodega exista
            var bodegaExiste = await _context.Bodegas
                .AnyAsync(b => b.IdBodega == existencia.IdBodega);

            if (!bodegaExiste)
            {
                return BadRequest("La bodega indicada no existe.");
            }

            // Evitar duplicar el producto en la misma bodega
            var duplicada = await _context.Existencias.AnyAsync(e =>
                e.IdProducto == existencia.IdProducto &&
                e.IdBodega == existencia.IdBodega);

            if (duplicada)
            {
                return Conflict(
                    "Ya existe una existencia para este producto en esa bodega.");
            }

            _context.Existencias.Add(existencia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerExistencia),
                new { id = existencia.IdExistencia },
                existencia);
        }

        // PUT: api/Existencias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarExistencia(
            int id, Existencia existencia)
        {
            if (id != existencia.IdExistencia)
            {
                return BadRequest("El ID de la URL no coincide con el ID del registro.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.Existencias.FindAsync(id);

            if (existente == null)
            {
                return NotFound("Existencia no encontrada.");
            }

            // Validar producto en Catalogo.Api
            try
            {
                var respuesta = await _httpClient.GetAsync(
                    $"api/Productos/{existencia.IdProducto}");

                if (respuesta.StatusCode == HttpStatusCode.NotFound)
                {
                    return BadRequest("El producto no existe en Catalogo.Api.");
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    return StatusCode(503,
                        "Catalogo.Api no pudo validar el producto.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(503,
                    "No se pudo establecer conexión con Catalogo.Api.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503,
                    "La consulta a Catalogo.Api excedió el tiempo de espera.");
            }

            var bodegaExiste = await _context.Bodegas
                .AnyAsync(b => b.IdBodega == existencia.IdBodega);

            if (!bodegaExiste)
            {
                return BadRequest("La bodega indicada no existe.");
            }

            var duplicada = await _context.Existencias.AnyAsync(e =>
                e.IdExistencia != id &&
                e.IdProducto == existencia.IdProducto &&
                e.IdBodega == existencia.IdBodega);

            if (duplicada)
            {
                return Conflict(
                    "Ya existe una existencia para este producto en esa bodega.");
            }

            _context.Entry(existente).CurrentValues.SetValues(existencia);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Existencias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarExistencia(int id)
        {
            var existencia = await _context.Existencias.FindAsync(id);

            if (existencia == null)
            {
                return NotFound("Existencia no encontrada.");
            }

            _context.Existencias.Remove(existencia);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
