namespace Jaeger.Core.Baggage
{
    /// <summary>
    /// <see cref="IBaggageRestrictionManager"/> is an interface for a class that manages baggage
    /// restrictions for baggage keys. The manager will return a <see cref="Restriction"/>
    /// for a specific baggage key which will determine whether the baggage key is
    /// allowed for the current service and any other applicable restrictions on the
    /// baggage value.
    /// </summary>
    public interface IBaggageRestrictionManager
    {
        Restriction GetRestriction(string service, string key);
    }
}