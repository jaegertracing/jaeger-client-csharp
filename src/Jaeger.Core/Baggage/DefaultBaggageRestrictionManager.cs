namespace Jaeger.Core.Baggage
{
    /// <summary>
    /// <see cref="DefaultBaggageRestrictionManager"/> is a manager that returns a <see cref="Restriction"/>
    /// that allows all baggage.
    /// </summary>
    public class DefaultBaggageRestrictionManager : IBaggageRestrictionManager
    {
        private readonly Restriction _restriction;

        public DefaultBaggageRestrictionManager()
            : this(Restriction.DefaultMaxValueLength)
        {
        }

        public DefaultBaggageRestrictionManager(int maxValueLength)
        {
            _restriction = new Restriction(true, maxValueLength);
        }

        public virtual Restriction GetRestriction(string service, string key)
        {
            return _restriction;
        }
    }
}