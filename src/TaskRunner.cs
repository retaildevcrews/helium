using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public class TaskRunner
    {
        public Task Task { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }
}
