using System;
using System.Threading;
using Jaeger.Thrift.Agent;
using LetsTrace.Jaeger.Samplers.HTTP;
using LetsTrace.Samplers;
using Thrift.Protocols;
using Thrift.Transports.Client;
using SamplingStrategyResponse = LetsTrace.Samplers.HTTP.SamplingStrategyResponse;

namespace LetsTrace.Jaeger.Samplers
{
    public class HttpSamplingManager : ISamplingManager
    {
        public const string DEFAULT_HOST_PORT = "localhost:5778";
        private readonly string hostPort;
        private readonly TBinaryProtocol.Factory protocolFactory;

        /// <summary>
        /// This constructor expects running sampling manager on <value>DEFAULT_HOST_PORT</value>
        /// </summary>
        public HttpSamplingManager() : this(DEFAULT_HOST_PORT)
        {
        }

        public HttpSamplingManager(String hostPort)
        {
            this.hostPort = hostPort ?? DEFAULT_HOST_PORT;
            this.protocolFactory = new TBinaryProtocol.Factory();
        }

        public SamplingStrategyResponse GetSamplingStrategy(string serviceName)
        {
            try
            {
                var samplerUri = new UriBuilder("http", this.hostPort) {Query = $"service={serviceName}"}.Uri;
                var httpTransport = new THttpClientTransport(samplerUri, null);
                var protocol = protocolFactory.GetProtocol(httpTransport);
                var samplingManagerClient = new SamplingManager.Client(protocol);
                return samplingManagerClient.getSamplingStrategyAsync(serviceName, CancellationToken.None).Result.FromThrift();
            }
            catch (Exception e)
            {
                throw new Exception("http call to get sampling strategy from local agent failed.", e);
            }
        }
    }
}
