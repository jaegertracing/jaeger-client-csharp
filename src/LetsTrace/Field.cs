using System;
using System.Diagnostics.CodeAnalysis;

namespace LetsTrace
{
    public abstract class Field : IEquatable<Field>
    {
        public string Key { get; set; }
        public TypeCode TypeCode { get; protected set; }
        [ExcludeFromCodeCoverage]
        public virtual string StringValue { get; }

        public bool Equals(Field other) => other.Key == Key && other.TypeCode == TypeCode && other.StringValue == StringValue;
    }

    public class Field<T> : Field, IEquatable<Field<T>>
    {
        private T _value;
        public T Value { 
            get => _value;
            set {
                _value = value;
                TypeCode = Type.GetTypeCode(value.GetType());
            }
        }

        public override string StringValue => _value.ToString();

        public bool Equals(Field<T> other) => Value != null && Value.Equals(other.Value) && base.Equals(other);
    }
}
