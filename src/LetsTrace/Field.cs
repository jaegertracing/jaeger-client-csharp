using System;

namespace LetsTrace
{
    public abstract class Field : IEquatable<Field>
    {
        public string Key { get; set; }
        public TypeCode TypeCode { get; protected set; }
        public virtual string StringValue { get; }

        public bool Equals(Field other) => other.Key == Key && other.TypeCode == TypeCode && other.StringValue == StringValue;
    }

    public class Field<T> : Field, IEquatable<Field<T>>
    {
        private T _value;
        public T Value { 
            get { return _value; } 
            set {
                _value = value;
                TypeCode = Type.GetTypeCode(value.GetType());
            }
        }

        public override string StringValue { get { return _value.ToString(); } }

        public bool Equals(Field<T> other) => Value is T && Value.Equals(other.Value) && base.Equals(other);
    }
}