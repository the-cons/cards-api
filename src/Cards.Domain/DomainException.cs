namespace Cards.Domain;

/// <summary>
/// A business rule was violated. Expected, not a bug.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
