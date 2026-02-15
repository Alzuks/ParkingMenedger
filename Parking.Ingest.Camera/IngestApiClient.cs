using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Parking.Application.UseCases.ProcessPassage;

public sealed class IngestApiClient
{
    private readonly HttpClient _http;

    public IngestApiClient(HttpClient http, IOptions<IngestClientOptions> opt)
    {
        _http = http;
        _http.BaseAddress = new Uri(opt.Value.BaseUrl);
        _http.DefaultRequestHeaders.Add("X-Api-Key", opt.Value.ApiKey);
    }

    public async Task SendAsync(ProcessPassageCommand cmd, CancellationToken ct)
    {
        await _http.PostAsJsonAsync("/ingest/passage", new
        {
            cmd.OccurredAt,
            cmd.PlateRaw,
            cmd.Direction,
            cmd.JpegPath,
            cmd.Confidence
        }, ct);
    }
}
