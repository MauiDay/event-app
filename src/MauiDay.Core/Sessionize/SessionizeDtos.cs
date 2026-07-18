namespace MauiDay.Core.Sessionize;

public sealed class SessionizeAllDto
{
    public IReadOnlyList<SessionizeSessionDto> Sessions { get; init; } = [];

    public IReadOnlyList<SessionizeSpeakerDto> Speakers { get; init; } = [];

    public IReadOnlyList<SessionizeRoomDto> Rooms { get; init; } = [];
}

public sealed class SessionizeSessionDto
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string StartsAt { get; init; }

    public required string EndsAt { get; init; }

    public bool IsServiceSession { get; init; }

    public IReadOnlyList<string> Speakers { get; init; } = [];

    public int? RoomId { get; init; }
}

public sealed class SessionizeSpeakerDto
{
    public required string Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? FullName { get; init; }

    public string? Bio { get; init; }

    public string? TagLine { get; init; }

    public string? ProfilePicture { get; init; }

    public IReadOnlyList<SessionizeLinkDto> Links { get; init; } = [];

    public IReadOnlyList<int> Sessions { get; init; } = [];
}

public sealed class SessionizeLinkDto
{
    public string? Title { get; init; }

    public string? Url { get; init; }
}

public sealed class SessionizeRoomDto
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public int Sort { get; init; }
}
