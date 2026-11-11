
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasStockController : ControllerBase
    {
        private readonly InventarioDBContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReservasStockController(
            InventarioDBContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/ReservasStock
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaStock>>> GetReservas()
        {
            return await _context.ReservasStock
                .Include(r => r.BodegaRelacion)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/ReservasStock/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaStock>> GetReserva(int id)
        {
            var reserva = await _context.ReservasStock
                .Include(r => r.BodegaRelacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound("La reserva no existe.");

            return Ok(reserva);
        }

        // POST: api/ReservasStock
        [HttpPost]
        public async Task<ActionResult<ReservaStock>> PostReserva(
            ReservaStock reserva)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (reserva.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            // Validar que el producto exista en Catalogo.Api
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5145/");

            try
            {
                var respuesta = await client.GetAsync(
                    $"api/Productos/{reserva.IdProducto}");

                if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return BadRequest("El producto no existe en el catálogo.");

                if (!respuesta.IsSuccessStatusCode)
                    return StatusCode(503, "No se pudo consultar el servicio de catálogo.");
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "El servicio de catálogo no está disponible.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503, "El servicio de catálogo tardó demasiado en responder.");
            }

            // Validar que la bodega exista
            var bodega = await _context.Bodegas
                .FindAsync(reserva.IdBodega);

            if (bodega == null)
                return BadRequest("La bodega no existe.");

            // Consultar existencia del producto en la bodega
            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e =>
                    e.IdProducto == reserva.IdProducto &&
                    e.IdBodega == reserva.IdBodega);

            if (existencia == null)
                return BadRequest("No existe stock registrado para este producto en la bodega.");

            // Calcular las reservas que todavía ocupan stock
            var stockReservado = await _context.ReservasStock
                .Where(r =>
                    r.IdProducto == reserva.IdProducto &&
                    r.IdBodega == reserva.IdBodega &&
                    (r.Estado == "Pendiente" || r.Estado == "Confirmada"))
                .SumAsync(r => (int?)r.Cantidad) ?? 0;

            var disponible = existencia.Stock - stockReservado;

            if (reserva.Cantidad > disponible)
                return BadRequest($"Stock insuficiente. Disponible: {disponible} unidades.");

            // Registrar la reserva
            reserva.Estado = "Pendiente";

            _context.ReservasStock.Add(reserva);
            await _context.SaveChangesAsync();

            // Cargar la información de la bodega
            await _context.Entry(reserva)
                .Reference(r => r.BodegaRelacion)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetReserva),
                new { id = reserva.IdReserva },
                reserva);
        }

        // POST: api/ReservasStock/Automatica
        [HttpPost("Automatica")]
        public async Task<ActionResult<ReservaStock>> PostReservaAutomatica(
            [FromQuery] int idProducto,
            [FromQuery] int cantidad)
        {
            if (idProducto <= 0 || cantidad <= 0)
                return BadRequest("El producto y la cantidad deben ser válidos.");

            // Validar producto en Catalogo.Api
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5145/");

            try
            {
                var respuesta = await client.GetAsync(
                    $"api/Productos/{idProducto}");

                if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return BadRequest("El producto no existe en el catálogo.");

                if (!respuesta.IsSuccessStatusCode)
                    return StatusCode(503, "No se pudo consultar el catálogo.");
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "El servicio de catálogo no está disponible.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503, "El servicio de catálogo tardó demasiado.");
            }

            // Buscar bodegas que tengan registrado el producto
            var existencias = await _context.Existencias
                .Where(e => e.IdProducto == idProducto)
                .OrderBy(e => e.IdBodega)
                .ToListAsync();

            foreach (var existencia in existencias)
            {
                // Calcular las reservas activas de cada bodega
                var reservado = await _context.ReservasStock
                    .Where(r =>
                        r.IdProducto == idProducto &&
                        r.IdBodega == existencia.IdBodega &&
                        (r.Estado == "Pendiente" ||
                         r.Estado == "Confirmada"))
                    .SumAsync(r => (int?)r.Cantidad) ?? 0;

                int disponible = existencia.Stock - reservado;

                if (disponible >= cantidad)
                {
                    var reserva = new ReservaStock
                    {
                        IdProducto = idProducto,
                        IdBodega = existencia.IdBodega,
                        Cantidad = cantidad,
                        Estado = "Pendiente"
                    };

                    _context.ReservasStock.Add(reserva);
                    await _context.SaveChangesAsync();

                    // Cargar la información de la bodega
                    await _context.Entry(reserva)
                        .Reference(r => r.BodegaRelacion)
                        .LoadAsync();

                    return CreatedAtAction(
                        nameof(GetReserva),
                        new { id = reserva.IdReserva },
                        reserva);
                }
            }

            return BadRequest(
                "No existe una bodega con suficiente stock disponible.");
        }

        // PUT: api/ReservasStock/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(
            int id, ReservaStock reserva)
        {
            if (id != reserva.IdReserva)
                return BadRequest("El ID de la reserva no coincide.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reservaExistente = await _context.ReservasStock
                .FindAsync(id);

            if (reservaExistente == null)
                return NotFound("La reserva no existe.");

            if (reserva.Estado != "Pendiente" &&
                reserva.Estado != "Confirmada" &&
                reserva.Estado != "Cancelada")
            {
                return BadRequest(
                    "El estado debe ser Pendiente, Confirmada o Cancelada.");
            }

            // Solo se permite cambiar el estado de la reserva
            reservaExistente.Estado = reserva.Estado;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ReservasStock/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.ReservasStock.FindAsync(id);

            if (reserva == null)
                return NotFound("La reserva no existe.");

            if (reserva.Estado != "Cancelada")
                return BadRequest(
                    "Primero debe cancelar la reserva antes de eliminarla.");

            _context.ReservasStock.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
