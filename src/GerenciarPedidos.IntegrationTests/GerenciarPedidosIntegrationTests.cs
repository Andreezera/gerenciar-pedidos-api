using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace GerenciarPedidos.IntegrationTests;

public class GerenciarPedidosIntegrationTests : IClassFixture<GerenciarPedidosTestApplication>
{
    private readonly HttpClient _client;

    public GerenciarPedidosIntegrationTests(GerenciarPedidosTestApplication factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarPedido_201()
    {
        // Arrange
        var novoPedido = new
        {
            clienteId = 1,
            itens = new[]
            {
                new { produtoId = 10, quantidade = 4, valor = 52.70 }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(novoPedido), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/pedidos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("status").GetString().Should().Be("Criado");
    }

    [Fact]
    public async Task ObterPedidoPorId_200()
    {
        // Arrange
        var novoPedido = new
        {
            clienteId = 2,
            itens = new[]
            {
                new { produtoId = 400, quantidade = 5, valor = 12.50 }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(novoPedido), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/pedidos", content);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int pedidoId = createdPedido.GetProperty("id").GetInt32();

        // Act
        var response = await _client.GetAsync($"/api/pedidos/{pedidoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetInt32().Should().Be(pedidoId);
    }

    [Fact]
    public async Task ListarPedidosPorStatus_200()
    {
        // Arrange
        var novoPedido = new
        {
            clienteId = 10,
            itens = new[]
            {
                new { produtoId = 72, quantidade = 5, valor = 400 }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(novoPedido), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/pedidos", content);

        // Act
        var response = await _client.GetAsync("/api/pedidos?status=Criado");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pedidos = await response.Content.ReadFromJsonAsync<JsonElement>();
        pedidos.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CriarPedidoDuplicado_400()
    {
        // Arrange
        var novoPedido = new
        {
            clienteId = 4,
            itens = new[]
            {
                new { produtoId = 3003, quantidade = 1, valor = 49.90 }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(novoPedido), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/pedidos", content);

        // Act
        var response = await _client.PostAsync("/api/pedidos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPedidoInexistente_404()
    {
        // Act
        var response = await _client.GetAsync("/api/pedidos/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}