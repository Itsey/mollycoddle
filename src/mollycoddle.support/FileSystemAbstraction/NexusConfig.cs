namespace mollycoddle;

public record NexusConfig {
    public string? Username { get; init; }
    public string? Password { get; init; }
    public required string Url { get; init; }
    public required string Server { get; init; }

    public string BasePathUrl => Url[..(Url.LastIndexOf('/') + 1)];

    public required string FilenameUrl { get; init; }
    public string? SearchPath { get; init; }
    public string? Repository { get; init; }
}