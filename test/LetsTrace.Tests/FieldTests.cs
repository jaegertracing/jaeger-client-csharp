using System;
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
            var field2 = value.ToField<T>(_key);
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
            var field = ExecuteFieldTest((double)0.5);
        }

        [Fact]
        public void Field_Decimal()
        {
            var field = ExecuteFieldTest((decimal)0.5);
        }

        [Fact]
        public void Field_Boolean()
        {
            var field = ExecuteFieldTest((bool)true);
        }

        [Fact]
        public void Field_UInt16()
        {
            var field = ExecuteFieldTest((ushort)42);
        }

        [Fact]
        public void Field_UInt32()
        {
            var field = ExecuteFieldTest((uint)42);
        }

        [Fact]
        public void Field_UInt64()
        {
            var field = ExecuteFieldTest((ulong)42);
        }

        [Fact]
        public void Field_Int16()
        {
            var field = ExecuteFieldTest((short)42);
        }

        [Fact]
        public void Field_Int32()
        {
            var field = ExecuteFieldTest((int)42);
        }

        [Fact]
        public void Field_Int64()
        {
            var field = ExecuteFieldTest((long)42);
        }
    }
}
