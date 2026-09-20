using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoApi.Tests;

/// <summary>
/// End-to-end proof that the package is wired: a failed Result thrown in a controller becomes a
/// problem body with the status its key declares, through the real pipeline
/// (ProblemDetails + UseExceptionHandler + ResultExceptionHandler + the resx translator).
/// </summary>
public class ResultPipelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ResultPipelineTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task KeyWithHttpStatus404_AnswersNotFoundProblem()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/results/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Equal($"User '{id}' was not found.", body.GetProperty("detail").GetString());
        Assert.True(body.TryGetProperty("traceId", out _));

        var entry = body.GetProperty("validation")[0];
        Assert.Equal("User.NotFound", entry.GetProperty("key").GetString());
        Assert.Equal(id.ToString(), entry.GetProperty("parameters").GetProperty("Id").GetString());
    }

    [Fact]
    public async Task RequestValidator_Rejects_WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/results", new { name = "", email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.DoesNotContain("User.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task DutchCulture_TranslatesDetail()
    {
        var id = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/results/{id}");
        request.Headers.AcceptLanguage.ParseAdd("nl");

        var response = await _client.SendAsync(request);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"Gebruiker '{id}' is niet gevonden.", body.GetProperty("detail").GetString());
    }
}
