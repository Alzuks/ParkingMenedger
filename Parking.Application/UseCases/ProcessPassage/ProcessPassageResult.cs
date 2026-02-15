namespace Parking.Application.UseCases.ProcessPassage;

public enum PassageAction
{
    StoredOnly = 0,
    SessionOpened = 1,
    SessionClosed = 2,
    IgnoredNoPlate = 3
}

public sealed record ProcessPassageResult(
    PassageAction Action,
    long? SessionId = null,
    string? Reason = null
);
