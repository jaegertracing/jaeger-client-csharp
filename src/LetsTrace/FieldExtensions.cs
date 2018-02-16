using System;
using System.Collections.Generic;
using System.ComponentModel;

using JaegerTag = Jaeger.Thrift.Tag;
using JaegerTagType = Jaeger.Thrift.TagType;

namespace LetsTrace
{
    public static class FieldExtensions
    {
        // ValueAs safely casts a Field object to the type T and returns the
        // value. If Field cannot be cast to the object of type T it will
        // return the default value of type T
        public static T ValueAs<T>(this Field field)
        {
            if (field is Field<T>)
            {
                return ((Field<T>)field).Value;
            }

            return default(T);
        }

        // ValueAs safely casts a Field object to the type FieldType and then
        // returns the value of FieldType as type ConvertTo. If Field cannot be
        // cast to the object of type FieldType it will return the default
        // value of type FieldType as type ConvertTo
        public static ConvertTo ValueAs<ConvertTo, FieldType>(this Field field)
        {
            var value = field.ValueAs<FieldType>();
            return (ConvertTo)Convert.ChangeType(value, typeof(ConvertTo));
        }

        public static List<Field> ToFieldList(this IEnumerable<KeyValuePair<string, object>> kvPairs)
        {
            var fieldList = new List<Field>();

            foreach (var kv in kvPairs)
            {
                fieldList.Add(kv.ToField());
            }

            return fieldList;
        }

        public static Field<T> ToField<T>(this object convertMe, string key)
        {
            return new Field<T> { Key = key, Value = (T)convertMe };
        }

        public static Field ToField(this KeyValuePair<string, object> kv)
        {
            // special case that we can't handle via typecodes
            if (kv.Value is byte[]) {
                return kv.Value.ToField<byte[]>(kv.Key);
            }

            var typeCode = Type.GetTypeCode(kv.Value.GetType());
            
            switch(typeCode)
            {
                case TypeCode.String:
                    return kv.Value.ToField<string>(kv.Key);
                case TypeCode.Double:
                    return kv.Value.ToField<double>(kv.Key);
                case TypeCode.Decimal:
                    return kv.Value.ToField<decimal>(kv.Key);
                case TypeCode.Boolean:
                    return kv.Value.ToField<bool>(kv.Key);
                case TypeCode.UInt16:
                    return kv.Value.ToField<UInt16>(kv.Key);
                case TypeCode.UInt32:
                    return kv.Value.ToField<UInt32>(kv.Key);
                case TypeCode.UInt64:
                    return kv.Value.ToField<UInt64>(kv.Key);
                case TypeCode.Int16:
                    return kv.Value.ToField<Int16>(kv.Key);
                case TypeCode.Int32:
                    return kv.Value.ToField<Int32>(kv.Key);
                case TypeCode.Int64:
                    return kv.Value.ToField<Int64>(kv.Key);
                default:
                    return new Field<string> { Key = kv.Key, Value = kv.Value.ToString() };
            }
        }
    }
}
