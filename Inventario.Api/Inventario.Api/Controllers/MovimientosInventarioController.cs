
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosInventarioController : ControllerBase
    {
        private readonly InventarioDBContext _context;
        private readonly HttpClient _httpClient;

        public MovimientosInventarioController(
            InventarioDBContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();

            _httpClient.BaseAddress = new Uri("http://localhost:5145/");
        }

        // GET: api/MovimientosInventario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoInventario>>> ObtenerMovimientos()
        {
            return await _context.MovimientosInventario
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MovimientosInventario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoInventario>> ObtenerMovimiento(int id)
        {
            var movimiento = await _context.MovimientosInventario
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return NotFound("Movimiento no encontrado.");
            }

            return Ok(movimiento);
        }

        // POST: api/MovimientosInventario
        [HttpPost]
        public async Task<ActionResult<MovimientoInventario>> AgregarMovimiento(
            MovimientoInventario movimiento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (movimiento.Tipo != "Entrada" &&
                movimiento.Tipo != "Salida")
            {
                return BadRequest("El tipo debe ser Entrada o Salida.");
            }

            // Validar producto en Catalogo.Api
            try
            {
                var respuesta = await _httpClient.GetAsync(
                    $"api/Productos/{movimiento.IdProducto}");

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
                    "No se pudo conectar con Catalogo.Api.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503,
                    "La consulta a Catalogo.Api excedió el tiempo de espera.");
            }

            // Validar bodega
            var bodegaExiste = await _context.Bodegas
                .AnyAsync(b => b.IdBodega == movimiento.IdBodega);

            if (!bodegaExiste)
            {
                return BadRequest("La bodega no existe.");
            }

            // Buscar existencia del producto en la bodega
            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e =>
                    e.IdProducto == movimiento.IdProducto &&
                    e.IdBodega == movimiento.IdBodega);

            if (movimiento.Tipo == "Entrada")
            {
                if (existencia == null)
                {
                    existencia = new Existencia
                    {
                        IdProducto = movimiento.IdProducto,
                        IdBodega = movimiento.IdBodega,
                        Stock = movimiento.Cantidad
                    };

                    _context.Existencias.Add(existencia);
                }
                else
                {
                    existencia.Stock += movimiento.Cantidad;
                }
            }
            else
            {
                if (existencia == null ||
                    existencia.Stock < movimiento.Cantidad)
                {
                    return BadRequest("No existe suficiente stock para realizar la salida.");
                }

                existencia.Stock -= movimiento.Cantidad;
            }

            if (movimiento.Fecha == default)
            {
                movimiento.Fecha = DateTime.Now;
            }

            _context.MovimientosInventario.Add(movimiento);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerMovimiento),
                new { id = movimiento.IdMovimiento },
                movimiento);
        }

        // DELETE: api/MovimientosInventario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMovimiento(int id)
        {
            var movimiento = await _context.MovimientosInventario
                .FindAsync(id);

            if (movimiento == null)
            {
                return NotFound("Movimiento no encontrado.");
            }

            return BadRequest(
                "No se permite eliminar un movimiento porque afecta el historial de inventario.");
        }
    }
}
