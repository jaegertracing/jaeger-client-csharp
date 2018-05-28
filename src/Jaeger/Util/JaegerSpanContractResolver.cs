using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Jaeger.Util
{
    class JaegerSpanContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Span) && property.PropertyName == nameof(Span.Tracer))
                property.ShouldSerialize = i => false;
            
            return property;
        }
    }
}
