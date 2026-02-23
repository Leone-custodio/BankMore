using System.Net;
using System.Net.Http.Json;
using BankMore.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankMore.IntegrationTests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetSaldo_SemToken_DeveRetornarUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/contacorrente/saldo");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ComDadosInvalidos_DeveRetornarUnauthorized()
        {
            var client = _factory.CreateClient();
            var loginRequest = new LoginRequest("12345678901", null, "senha_errada");

            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
