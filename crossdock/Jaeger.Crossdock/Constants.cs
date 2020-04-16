namespace Jaeger.Crossdock
{
    public static class Constants
    {
        public const string TRANSPORT_HTTP = "HTTP";
        public const string TRANSPORT_DUMMY = "DUMMY";

        public const string BAGGAGE_KEY = "crossdock-baggage-key";
  
        public const string ENV_PROP_SENDER_TYPE = "SENDER";

        // DEFAULT_TRACER_SERVICE_NAME is the service name used by the tracer
        public const string DEFAULT_TRACER_SERVICE_NAME = "crossdock-csharp";

        // DEFAULT_SERVER_PORT_HTTP is the port where HTTP server runs
        public const string DEFAULT_SERVER_PORT_HTTP = "8081";
    }
}
