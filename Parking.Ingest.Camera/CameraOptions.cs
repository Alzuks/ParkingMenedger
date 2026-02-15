namespace Parking.Ingest.Camera;

public sealed class CameraOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }

    public string User { get; set; } = default!;
    public string Password { get; set; } = default!;
    public short Channel { get; set; }

    // byDirection: 1/2 -> In/Out
    public bool ForwardMeansIn { get; set; }

    // дедуп событий камеры (сек)
    public int DedupSeconds { get; set; }

    // диагностика
    public string? JpegDir { get; set; }
    public bool WriteCsv { get; set; }
    public string CsvFileName { get; set; } = default!;

}
