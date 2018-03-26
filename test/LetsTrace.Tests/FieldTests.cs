using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace LetsTrace.Tests
{
    public class FieldTests
    {
        private readonly string _key;

        public FieldTests()
        {
            _key = "Key";
        }

        private Field<T> ExecuteFieldTest<T>(T value) where T : IConvertible
        {
            var field1 = value.ToField<T>(_key);
            var field2 = new KeyValuePair<string, object>(_key, value).ToField();
            Assert.IsType<Field<T>>(field1);
            Assert.Equal(_key, field1.Key);
            Assert.Equal(Type.GetTypeCode(typeof(T)), field1.TypeCode);
            Assert.Equal(value, field1.Value);
            Assert.Equal(value.ToString(CultureInfo.CurrentCulture), field1.StringValue);

            Assert.False(ReferenceEquals(field1, field2));
            Assert.True(field1.Equals(field2));
            Assert.True(field2.Equals(field1));
            return field1;
        }

        [Fact]
        public void Field_Null()
        {
            var field2 = new KeyValuePair<string, object>(_key, null).ToField();
            Assert.Equal("", field2.StringValue);
        }

        [Fact]
        public void Field_String()
        {
            var value = "HelloWorld";
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());

            value = "42";
            field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, string>());
        }

        [Fact]
        public void Field_Double()
        {
            var value = (double)1.5;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<double>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(2, field.ValueAs<int, double>());
        }

        [Fact]
        public void Field_Decimal()
        {
            var value = (decimal)1.5;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<decimal>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(2, field.ValueAs<int, decimal>());
        }

        [Fact]
        public void Field_Boolean()
        {
            var value = (bool)true;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<bool>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(1, field.ValueAs<int, bool>());
        }

        [Fact]
        public void Field_UInt16()
        {
            var value = (ushort)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<ushort>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, ushort>());
        }

        [Fact]
        public void Field_UInt32()
        {
            var value = (uint)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<uint>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, uint>());
        }

        [Fact]
        public void Field_UInt64()
        {
            var value = (ulong)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<ulong>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, ulong>());
        }

        [Fact]
        public void Field_Int16()
        {
            var value = (short)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<short>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, short>());
        }

        [Fact]
        public void Field_Int32()
        {
            var value = (int)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<int>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<long>());
            Assert.Equal(42L, field.ValueAs<long, int>());
        }

        [Fact]
        public void Field_Int64()
        {
            var value = (long)42;
            var field = ExecuteFieldTest(value);
            Assert.Equal(value, field.ValueAs<long>());
            Assert.Null(field.ValueAs<string>());
            Assert.Equal(0, field.ValueAs<int>());
            Assert.Equal(42, field.ValueAs<int, long>());
        }
    }
}
