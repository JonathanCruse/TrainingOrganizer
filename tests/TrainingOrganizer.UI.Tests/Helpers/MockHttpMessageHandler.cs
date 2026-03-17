using System.Net;
using System.Net.Http.Json;

namespace TrainingOrganizer.UI.Tests.Helpers;

public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new();

    public List<HttpRequestMessage> SentRequests { get; } = [];

    public void RespondWithJson<T>(string pathAndQuery, T content)
    {
        _responses[pathAndQuery] = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(content)
        };
    }

    public void RespondWith(string pathAndQuery, HttpStatusCode statusCode)
    {
        _responses[pathAndQuery] = new HttpResponseMessage(statusCode);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        var key = request.RequestUri!.PathAndQuery;

        // Exact match first
        if (_responses.TryGetValue(key, out var response))
            return Task.FromResult(response);

        // Prefix match (path without query string) for URLs with dynamic params
        var path = request.RequestUri!.AbsolutePath;
        var prefixMatch = _responses.FirstOrDefault(r => r.Key == path);
        if (prefixMatch.Value is not null)
            return Task.FromResult(prefixMatch.Value);

        var available = string.Join(", ", _responses.Keys.Select(k => $"'{k}'"));
        throw new InvalidOperationException(
            $"No mock response for '{request.Method} {key}'. Available: [{available}]");
    }
}
