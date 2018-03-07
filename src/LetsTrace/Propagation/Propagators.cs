namespace LetsTrace.Propagation
{
    public static class Propagators
    {
        public static readonly IPropagationRegistry Console = new ConsolePropagationRegistry();

        public static readonly IPropagationRegistry TextMap = new TextMapPropagationRegistry();
    }
}
