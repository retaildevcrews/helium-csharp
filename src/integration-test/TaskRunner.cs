using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public class TaskRunner
    {
        public Task Task;
        public CancellationTokenSource TokenSource;
    }
}
