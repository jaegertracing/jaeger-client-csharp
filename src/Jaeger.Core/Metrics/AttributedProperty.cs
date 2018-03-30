using System.Reflection;

namespace Jaeger.Core.Metrics
{
    internal interface IAttributedProperty
    {
        PropertyInfo PropertyInfo { get; }
        FieldInfo BackingField { get; set; }
    }

    internal class AttributedProperty<T> : IAttributedProperty
    {
        public T Attribute { get; }
        public PropertyInfo PropertyInfo { get; }
        public FieldInfo BackingField { get; set; }

        public AttributedProperty(T attribute, PropertyInfo propertyInfo)
        {
            this.Attribute = attribute;
            this.PropertyInfo = propertyInfo;
        }
    }
}
