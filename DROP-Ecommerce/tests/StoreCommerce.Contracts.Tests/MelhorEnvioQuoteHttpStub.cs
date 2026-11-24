using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

internal sealed class MelhorEnvioQuoteHttpStub : HttpMessageHandler
{
    public int Calls { get; private set; }
    public JsonElement LastRequest { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://sandbox.melhorenvio.com.br/api/v2/me/shipment/calculate", request.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("test-shipping-access-token", request.Headers.Authorization?.Parameter);
        Assert.Equal("DRPCommerce (integrations@example.invalid)", request.Headers.UserAgent.ToString());
        Calls++;
        LastRequest = await request.Content!.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                [
                    { "id": 1, "name": "PAC", "currency": "R$", "error": "Indisponível" },
                    {
                        "id": 2, "name": "SEDEX", "price": "12.50", "custom_price": "18.75", "currency": "R$",
                        "delivery_time": 2, "custom_delivery_time": 4,
                        "company": { "id": 1, "name": "Correios" },
                        "packages": [
                            { "weight": "1.5", "insurance_value": "10.00", "dimensions": { "height": 11, "width": 12, "length": 13 } },
                            { "weight": "1.5", "insurance_value": "10.00", "dimensions": { "height": 14, "width": 15, "length": 16 } }
                        ]
                    }
                ]
                """, Encoding.UTF8, "application/json")
        };
    }
}
