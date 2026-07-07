using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OffersService.DTOs;
using OffersService.Tests.Helpers;
using Xunit;

namespace OffersService.Tests.Integration;

public class CreateRetailerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CreateRetailerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateRetailer_ReturnsCreated_WithLocationHeader()
    {
        var client = _factory.CreateClient();

        var dto = new RetailerCreateDto { Name = "New Retailer", Country = "GB" };

        var response = await client.PostAsJsonAsync("/api/retailers", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Location != null, "Location header should be present");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var id = doc.RootElement.GetProperty("id").GetInt32();

        Assert.EndsWith($"/api/retailers/{id}", response.Headers.Location!.ToString());

        // Verify resource exists by calling stats endpoint
        var statsResponse = await client.GetAsync($"/api/retailers/{id}/stats");
        Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);
    }

    [Fact]
    public async Task CreateRetailer_ReturnsConflict_WhenDuplicateName()
    {
        var client = _factory.CreateClient();

        // "Retailer A" is seeded in the in-memory DB
        var dto = new RetailerCreateDto { Name = "Retailer A", Country = "US" };

        var response = await client.PostAsJsonAsync("/api/retailers", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRetailer_ReturnsBadRequest_WhenInvalidCountry()
    {
        var client = _factory.CreateClient();

        var dto = new RetailerCreateDto { Name = "Invalid Country Retailer", Country = "USA" }; // country must be exactly 2 chars

        var response = await client.PostAsJsonAsync("/api/retailers", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
