using System;

namespace LetsTrace
{
    public abstract class Field
    {
        public string Key { get; set; }
        public TypeCode TypeCode { get; protected set; }
        public virtual string StringValue { get; }
    }

    public class Field<T> : Field
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
    }
}
