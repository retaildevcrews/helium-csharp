using System.Threading;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    public class TaskRunner
    {
        public Task Task;
        public CancellationTokenSource TokenSource;
    }
}
