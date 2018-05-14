using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Core.Reporters
{
    /// <summary>
    /// <see cref="IReporter"/> is the interface <see cref="Tracer"/> uses to report finished spans to something that
    /// collects those spans. Default implementation is <see cref="RemoteReporter"/> that sends spans out of process.
    /// </summary>
    public interface IReporter
    {
        void Report(Span span);

        /// <summary>
        /// Release any resources used by the reporter.
        /// </summary>
        /// <remarks>
        /// We don't use <see cref="System.IDisposable"/> because the <see cref="Tracer"/> should
        /// be able to close the reporter. If we would use <see cref="System.IDisposable"/> then
        /// the <see cref="Tracer"/> would call Dispose on a member it did not create itself.
        /// </remarks>
        Task CloseAsync(CancellationToken cancellationToken);
    }
}
