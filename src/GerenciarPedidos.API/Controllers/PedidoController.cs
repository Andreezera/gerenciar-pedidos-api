using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciarPedidos.API.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidoController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidoController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePedido([FromBody] PedidoCreateDto pedido)
    {
        var novoPedido = await _pedidoService.CreatePedidoAsync(pedido);
        return StatusCode(201, new { id = novoPedido.Id, status = "Criado" });
    }

    /// <summary>
    /// Obtém um pedido pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPedidoById(int id)
    {
        var pedido = await _pedidoService.GetPedidoByIdAsync(id);
        if (pedido == null)
            return NotFound();

        return Ok(pedido);
    }

    /// <summary>
    /// Lista pedidos filtrando por status
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListPedidosByStatus([FromQuery] string status)
    {
        var pedidos = await _pedidoService.ListPedidosByStatusAsync(status);
        return Ok(pedidos);
    }
}