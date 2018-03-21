using System;
using System.Collections.Generic;

using JaegerTag = Jaeger.Thrift.Tag;
using JaegerTagType = Jaeger.Thrift.TagType;

namespace LetsTrace.Jaeger.Transport
{
    public static class JaegerFieldExtensions
    {
        public static void Marshal(this Field field, List<JaegerTag> tags) {
            // special case that we can't handle via typecodes
            if (field is Field<byte[]>) {
                tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.BINARY, VBinary = field.ValueAs<byte[]>() });
                return;
            }
            
            switch(field.TypeCode)
            {
                case TypeCode.String:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.STRING, VStr = field.ValueAs<string>() });
                    break;
                case TypeCode.Double:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.DOUBLE, VDouble = field.ValueAs<double>() });
                    break;
                case TypeCode.Decimal:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.DOUBLE, VDouble = field.ValueAs<double, decimal>() });
                    break;
                case TypeCode.Boolean:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.BOOL, VBool = field.ValueAs<bool>() });
                    break;
                case TypeCode.UInt16:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, UInt16>() });
                    break;
                case TypeCode.UInt32:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, UInt32>() });
                    break;
                case TypeCode.UInt64:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, UInt64>() });
                    break;
                case TypeCode.Int16:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, Int16>() });
                    break;
                case TypeCode.Int32:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, Int32>() });
                    break;
                case TypeCode.Int64:
                    tags.Add(new JaegerTag{ Key = field.Key, VType = JaegerTagType.LONG, VLong = field.ValueAs<long, Int64>() });
                    break;
            }
        }
    }
}
