
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using System.Net.Http.Json;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesPedidoController : ControllerBase
    {
        private readonly PedidosDBContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public DetallesPedidoController(
            PedidosDBContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/DetallesPedido
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetalles()
        {
            return await _context.DetallesPedido
                .Include(d => d.PedidoRelacion)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/DetallesPedido/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetallePedido>> GetDetalle(int id)
        {
            var detalle = await _context.DetallesPedido
                .Include(d => d.PedidoRelacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDetalle == id);

            if (detalle == null)
                return NotFound("El detalle del pedido no existe.");

            return Ok(detalle);
        }

        // GET: api/DetallesPedido/Pedido/5
        [HttpGet("Pedido/{idPedido}")]
        public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetallesPorPedido(
            int idPedido)
        {
            var pedidoExiste = await _context.Pedidos
                .AnyAsync(p => p.IdPedido == idPedido);

            if (!pedidoExiste)
                return NotFound("El pedido no existe.");

            var detalles = await _context.DetallesPedido
                .Include(d => d.PedidoRelacion)
                .AsNoTracking()
                .Where(d => d.IdPedido == idPedido)
                .ToListAsync();

            return Ok(detalles);
        }

        // POST: api/DetallesPedido
        [HttpPost]
        public async Task<ActionResult<DetallePedido>> PostDetalle(
            DetallePedido detalle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (detalle.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            if (detalle.PrecioUnitario < 0)
                return BadRequest("El precio no puede ser negativo.");

            // Validar que el pedido exista
            var pedido = await _context.Pedidos
                .FindAsync(detalle.IdPedido);

            if (pedido == null)
                return BadRequest("El pedido no existe.");

            if (pedido.Estado != "Pendiente")
                return BadRequest("Solo se pueden agregar detalles a pedidos pendientes.");

            // Consultar el producto en Catalogo.Api
            var catalogoClient = _httpClientFactory.CreateClient();
            catalogoClient.BaseAddress = new Uri("http://localhost:5145/");

            try
            {
                var productoResponse = await catalogoClient.GetAsync(
                    $"api/Productos/{detalle.IdProducto}");

                if (productoResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return BadRequest("El producto no existe en el catálogo.");

                if (!productoResponse.IsSuccessStatusCode)
                    return StatusCode(503, "El servicio de catálogo no está disponible.");

                // Obtener el precio real del producto
                var producto = await productoResponse.Content
                    .ReadFromJsonAsync<ProductoCatalogo>();

                if (producto == null)
                    return StatusCode(503, "No se pudo obtener la información del producto.");

                if (producto.estado == false)
                    return BadRequest("El producto se encuentra inactivo.");

                detalle.PrecioUnitario = producto.precio;

                // Solicitar reserva automática de stock a Inventario.Api
                var inventarioClient = _httpClientFactory.CreateClient();

                var reservaResponse = await inventarioClient.PostAsync(
                    $"http://localhost:5056/api/ReservasStock/Automatica?idProducto={detalle.IdProducto}&cantidad={detalle.Cantidad}",
                    null);

                if (!reservaResponse.IsSuccessStatusCode)
                {
                    var mensaje = await reservaResponse.Content.ReadAsStringAsync();

                    if (reservaResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        return BadRequest(mensaje);

                    return StatusCode(503,
                        "No se pudo reservar el stock: " + mensaje);
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "No se pudo establecer comunicación con los microservicios.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503, "La solicitud a los microservicios tardó demasiado.");
            }

            // Guardar el detalle del pedido
            _context.DetallesPedido.Add(detalle);

            // Actualizar el total del pedido
            pedido.Total = await _context.DetallesPedido
                .Where(d => d.IdPedido == detalle.IdPedido)
                .SumAsync(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0;

            pedido.Total += detalle.Cantidad * detalle.PrecioUnitario;

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalle),
                new { id = detalle.IdDetalle },
                detalle);
        }

        // PUT: api/DetallesPedido/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalle(
            int id, DetallePedido detalle)
        {
            if (id != detalle.IdDetalle)
                return BadRequest("El ID del detalle no coincide.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var detalleExistente = await _context.DetallesPedido
                .FindAsync(id);

            if (detalleExistente == null)
                return NotFound("El detalle no existe.");

            var pedido = await _context.Pedidos
                .FindAsync(detalleExistente.IdPedido);

            if (pedido == null)
                return NotFound("El pedido no existe.");

            if (pedido.Estado != "Pendiente")
                return BadRequest("Solo se pueden modificar detalles de pedidos pendientes.");

            return BadRequest(
                "No se permite modificar directamente un detalle reservado. Debe cancelarse la reserva y registrar un nuevo detalle.");
        }

        // DELETE: api/DetallesPedido/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalle(int id)
        {
            var detalle = await _context.DetallesPedido.FindAsync(id);

            if (detalle == null)
                return NotFound("El detalle no existe.");

            return BadRequest(
                "No se permite eliminar directamente un detalle porque debe cancelarse primero su reserva en Inventario.Api.");
        }

        // Modelo auxiliar para leer la respuesta de Catalogo.Api
        private class ProductoCatalogo
        {
            public decimal precio { get; set; }
            public bool estado { get; set; }
        }
    }
}
