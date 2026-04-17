using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkTracker.IntegrationTests.Infrastructure;

// The API is configured with JsonStringEnumConverter server-side, but HttpClient's built-in
// ReadFromJsonAsync uses JsonSerializerOptions.Default which doesn't include it. Use these
// extensions (or TestJsonOptions) in integration tests so client-side deserialization of
// enum responses matches what the API emits.
public static class TestJsonExtensions
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static Task<T?> ReadApiJsonAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
        => content.ReadFromJsonAsync<T>(Options, cancellationToken);

    public static Task<HttpResponseMessage> PostApiJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
        => client.PostAsJsonAsync(requestUri, value, Options, cancellationToken);

    public static Task<HttpResponseMessage> PutApiJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
        => client.PutAsJsonAsync(requestUri, value, Options, cancellationToken);

    public static Task<T?> GetApiFromJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken cancellationToken = default)
        => client.GetFromJsonAsync<T>(requestUri, Options, cancellationToken);
}
