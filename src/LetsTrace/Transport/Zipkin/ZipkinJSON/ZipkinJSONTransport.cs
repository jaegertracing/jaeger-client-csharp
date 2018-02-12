using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using LetsTrace.Util;
using Newtonsoft.Json;

namespace LetsTrace.Transport.Zipkin.ZipkinJSON
{
    public enum KindEnum
    { 
        [EnumMember(Value = "CLIENT")]
        CLIENT = 1,
        
        [EnumMember(Value = "SERVER")]
        SERVER = 2,
        
        [EnumMember(Value = "PRODUCER")]
        PRODUCER = 3,
        
        [EnumMember(Value = "CONSUMER")]
        CONSUMER = 4
    }
    
    public class ZipkinJSONTransport : ITransport
    {
        public Uri Uri { get; }

        private List<Span> _buffer = new List<Span>(); // TODO: look into making this thread safe
        private int _bufferSize = 10;
        private static readonly HttpClient HttpClient = new HttpClient();
        
        public ZipkinJSONTransport(Uri uri, int bufferSize)
        {
            Uri = uri;
            _bufferSize = bufferSize;
        }

        public int Append(ILetsTraceSpan span)
        {
            var convertedSpan = ConvertSpan(span);

            _buffer.Add(convertedSpan);

            if (_buffer.Count > _bufferSize) {
                return Flush();
            }

            return 0;
        }

        public void Dispose()
        {
            Flush();
        }

        public int Flush()
        {
            var count = _buffer.Count;
            var serialized = JsonConvert.SerializeObject(_buffer);
            var response = HttpClient.PostAsync(Uri, new StringContent(serialized, Encoding.UTF8, "application/json")).ConfigureAwait(false).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            _buffer.Clear();

            return count;
        }

        private static KindEnum GetSpanKindEnumValue(string spanKind)
        {
            switch (spanKind)
            {
                case OpenTracing.Tags.SpanKindClient:
                    return KindEnum.CLIENT;
                case OpenTracing.Tags.SpanKindConsumer:
                    return KindEnum.CONSUMER;
                case OpenTracing.Tags.SpanKindProducer:
                    return KindEnum.PRODUCER;
                case OpenTracing.Tags.SpanKindServer:
                    return KindEnum.SERVER;
                default:
                    return 0;
            }
        }

        internal static Span ConvertSpan(ILetsTraceSpan span) {
            var context = (ILetsTraceSpanContext) span.Context;
            var endpoint = new Endpoint {
                ServiceName = span.Tracer.ServiceName,
                Ipv4 = span.Tracer.HostIPv4
            };
            var annotations = new List<Annotation>();
            span.Logs.ForEach(log => {
                var annotation = new Annotation { 
                    Timestamp = log.Timestamp.ToUnixTimeMicroseconds(),
                    Value = log.Message ?? log.Fields.ToString()
                };
                annotations.Add(annotation);
            });

            var tags = span.Tags.Where(t => t.Key != OpenTracing.Tags.SpanKind).ToDictionary(t => t.Key, t => t.Value);
            var spanKind = span.Tags.Where(t => t.Key == OpenTracing.Tags.SpanKind).FirstOrDefault().Value;

            var convertedSpan = new Span {
                TraceId = context.TraceId.ToString(),
                Name = span.OperationName.ToLower(),
                ParentId = context.ParentId.ToString(),
                Id = context.SpanId.ToString(),
                Timestamp = span.StartTimestamp.ToUnixTimeMicroseconds(),
                Duration = span.FinishTimestamp?.ToUnixTimeMicroseconds() - span.StartTimestamp.ToUnixTimeMicroseconds(),
                LocalEndpoint = endpoint,
                Annotations = annotations,
                Tags = tags,
                Kind = GetSpanKindEnumValue(spanKind)
            };

            // TODO: support debug
            // TODO: support shared
            // TODO: support remote endpoint

            return convertedSpan;
        }
    }
}