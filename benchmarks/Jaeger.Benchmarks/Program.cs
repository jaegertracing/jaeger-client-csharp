using BenchmarkDotNet.Running;

namespace Jaeger.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            switch (args[0])
            {
                case "RemoteReporter":
                    new RemoteReporterBenchmark().RunReport().GetAwaiter().GetResult();
                    break;
                default:
                    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
                    break;
            }
        }
    }
}
