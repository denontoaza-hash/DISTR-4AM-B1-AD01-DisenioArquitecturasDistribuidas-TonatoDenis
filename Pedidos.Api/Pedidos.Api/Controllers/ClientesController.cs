
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly PedidosDBContext _context;

        public ClientesController(PedidosDBContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("El cliente no existe.");

            return Ok(cliente);
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar cédula duplicada
            var cedulaExiste = await _context.Clientes
                .AnyAsync(c => c.Cedula == cliente.Cedula);

            if (cedulaExiste)
                return BadRequest("La cédula ya se encuentra registrada.");

            // Validar correo duplicado
            var correoExiste = await _context.Clientes
                .AnyAsync(c => c.Correo == cliente.Correo);

            if (correoExiste)
                return BadRequest("El correo ya se encuentra registrado.");

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCliente),
                new { id = cliente.IdCliente },
                cliente);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.IdCliente)
                return BadRequest("El ID no coincide.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clienteExistente = await _context.Clientes.FindAsync(id);

            if (clienteExistente == null)
                return NotFound("El cliente no existe.");

            // Validar cédula duplicada
            var cedulaExiste = await _context.Clientes
                .AnyAsync(c => c.Cedula == cliente.Cedula &&
                               c.IdCliente != id);

            if (cedulaExiste)
                return BadRequest("La cédula ya está registrada por otro cliente.");

            // Validar correo duplicado
            var correoExiste = await _context.Clientes
                .AnyAsync(c => c.Correo == cliente.Correo &&
                               c.IdCliente != id);

            if (correoExiste)
                return BadRequest("El correo ya está registrado por otro cliente.");

            clienteExistente.Nombres = cliente.Nombres;
            clienteExistente.Cedula = cliente.Cedula;
            clienteExistente.Correo = cliente.Correo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("El cliente no existe.");

            var tienePedidos = await _context.Pedidos
                .AnyAsync(p => p.IdCliente == id);

            if (tienePedidos)
                return BadRequest(
                    "No se puede eliminar el cliente porque tiene pedidos registrados.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
