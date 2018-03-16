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
        public const string DefaultHostPort = "localhost:5778";

        private readonly string _hostPort;
        private readonly TBinaryProtocol.Factory _protocolFactory;

        /// <summary>
        /// This constructor expects running sampling manager on <value>DefaultHostPort</value>
        /// </summary>
        public HttpSamplingManager() : this(DefaultHostPort)
        {
        }

        public HttpSamplingManager(string hostPort)
        {
            _hostPort = hostPort ?? DefaultHostPort;
            _protocolFactory = new TBinaryProtocol.Factory();
        }

        public SamplingStrategyResponse GetSamplingStrategy(string serviceName)
        {
            try
            {
                var samplerUri = new UriBuilder("http", _hostPort) {Query = $"service={serviceName}"}.Uri;
                var httpTransport = new THttpClientTransport(samplerUri, null);
                var protocol = _protocolFactory.GetProtocol(httpTransport);
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
