
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDBContext _context;

        public PedidosController(PedidosDBContext context)
        {
            _context = context;
        }

        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.ClienteRelacion)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.ClienteRelacion)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound("El pedido no existe.");

            return Ok(pedido);
        }

        // POST: api/Pedidos
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que el cliente exista
            var cliente = await _context.Clientes
                .FindAsync(pedido.IdCliente);

            if (cliente == null)
                return BadRequest("El cliente no existe.");

            if (pedido.Total < 0)
                return BadRequest("El total no puede ser negativo.");

            pedido.Fecha = pedido.Fecha == default
                ? DateTime.Now
                : pedido.Fecha;

            pedido.Estado = "Pendiente";

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPedido),
                new { id = pedido.IdPedido },
                pedido);
        }

        // PUT: api/Pedidos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.IdPedido)
                return BadRequest("El ID del pedido no coincide.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pedidoExistente = await _context.Pedidos
                .FindAsync(id);

            if (pedidoExistente == null)
                return NotFound("El pedido no existe.");

            // Validar que el cliente exista
            var cliente = await _context.Clientes
                .FindAsync(pedido.IdCliente);

            if (cliente == null)
                return BadRequest("El cliente no existe.");

            if (pedido.Total < 0)
                return BadRequest("El total no puede ser negativo.");

            if (pedido.Estado != "Pendiente" &&
                pedido.Estado != "Confirmado" &&
                pedido.Estado != "Cancelado")
            {
                return BadRequest(
                    "El estado debe ser Pendiente, Confirmado o Cancelado.");
            }

            pedidoExistente.IdCliente = pedido.IdCliente;
            pedidoExistente.Fecha = pedido.Fecha;
            pedidoExistente.Total = pedido.Total;
            pedidoExistente.Estado = pedido.Estado;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound("El pedido no existe.");

            // Verificar si tiene detalles registrados
            var tieneDetalles = await _context.DetallesPedido
                .AnyAsync(d => d.IdPedido == id);

            if (tieneDetalles)
                return BadRequest(
                    "No se puede eliminar el pedido porque tiene detalles registrados.");

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
