using Microsoft.Extensions.Logging;

namespace Cards.Application;

public partial class CardService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Created card {CardId} named {CardName}")]
    private static partial void LogCardCreated(ILogger logger, int cardId, string cardName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rejected card {CardName} ({Reason}): {Detail}")]
    private static partial void LogCardRejected(ILogger logger, string cardName, string reason, string detail);
}