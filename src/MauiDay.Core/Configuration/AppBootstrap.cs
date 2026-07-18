namespace MauiDay.Core.Configuration;

public sealed class AppBootstrap
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required string ActiveEventId { get; init; }

    public required IReadOnlyList<EventDescriptor> Events { get; init; }
}

public sealed class EventDescriptor
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required Uri ConfigUrl { get; init; }

    public required string BundledConfigAsset { get; init; }
}
