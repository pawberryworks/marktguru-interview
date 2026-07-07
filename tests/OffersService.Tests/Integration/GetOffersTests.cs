using System.Net;
using System.Net.Http.Json;
using OffersService.DTOs;
using OffersService.Tests.Helpers;
using Xunit;

namespace OffersService.Tests.Integration;

public class GetOffersTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public GetOffersTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetOffers_ReturnsPaginatedResult_WithData()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/offers?status=all&page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PaginatedOffersResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body.Page);
        Assert.Equal(20, body.PageSize);
        Assert.True(body.Total >= 0);
        Assert.NotNull(body.Items);
        // Seed data includes 3 offers
        Assert.Equal(3, body.Total);
        Assert.Equal(3, body.Items.Count());
    }

    [Fact]
    public async Task GetOffers_ReturnsEmptyResult_WhenNoMatches()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/offers?status=all&productId=9999");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PaginatedOffersResponse>();
        Assert.NotNull(body);
        Assert.Equal(0, body.Total);
        Assert.Empty(body.Items);
    }

    [Fact]
    public async Task GetOffers_ReturnsBadRequest_WhenPageSizeTooLarge()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/offers?pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
