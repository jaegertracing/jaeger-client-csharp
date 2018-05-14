using System.Linq;
using System.Net;
using System.Net.Sockets;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Jaeger.Core.Util;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TracerTagsTests
    {
        [Fact]
        public void TestTracerTags()
        {
            InMemoryReporter spanReporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("x")
                .WithReporter(spanReporter)
                .WithSampler(new ConstSampler(true))
                .WithZipkinSharedRpcSpan()
                .WithTag("tracer.tag.str", "y")
                .Build();

            Span span = (Span)tracer.BuildSpan("root").Start();

            // span should only contain sampler tags and no tracer tags
            Assert.Equal(2, span.GetTags().Count);
            Assert.True(span.GetTags().ContainsKey("sampler.type"));
            Assert.True(span.GetTags().ContainsKey("sampler.param"));
            Assert.False(span.GetTags().ContainsKey("tracer.tag.str"));
        }

        [Fact]
        public void TestDefaultHostTags()
        {
            InMemoryReporter spanReporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("x")
                .WithReporter(spanReporter)
                .Build();

            string hostname = tracer.GetHostName();
            string hostIPv4 = Dns.GetHostAddresses(hostname).First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

            Assert.Equal(hostname, tracer.Tags[Constants.TracerHostnameTagKey]);
            Assert.Equal(hostIPv4, tracer.Tags[Constants.TracerIpTagKey]);
            Assert.Equal(Utils.IpToInt(hostIPv4), tracer.IPv4);
        }

        [Fact]
        public void TestDeclaredHostTags()
        {
            InMemoryReporter spanReporter = new InMemoryReporter();
            string hostname = "myhost";
            string ip = "1.1.1.1";
            Tracer tracer = new Tracer.Builder("x")
                .WithReporter(spanReporter)
                .WithTag(Constants.TracerHostnameTagKey, hostname)
                .WithTag(Constants.TracerIpTagKey, ip)
                .Build();
            Assert.Equal(hostname, tracer.Tags[Constants.TracerHostnameTagKey]);
            Assert.Equal(ip, tracer.Tags[Constants.TracerIpTagKey]);
            Assert.Equal(Utils.IpToInt(ip), tracer.IPv4);
        }

        [Fact]
        public void TestEmptyDeclaredIpTag()
        {
            InMemoryReporter spanReporter = new InMemoryReporter();
            string ip = "";
            Tracer tracer = new Tracer.Builder("x")
                    .WithReporter(spanReporter)
                    .WithTag(Constants.TracerIpTagKey, ip)
                    .Build();
            Assert.Equal(0, tracer.IPv4);
        }

        [Fact]
        public void TestShortDeclaredIpTag()
        {
            InMemoryReporter spanReporter = new InMemoryReporter();
            string ip = ":19";
            Tracer tracer = new Tracer.Builder("x")
                    .WithReporter(spanReporter)
                    .WithTag(Constants.TracerIpTagKey, ip)
                    .Build();
            Assert.Equal(0, tracer.IPv4);
        }
    }
}
