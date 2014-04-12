using System.Threading;

namespace Sourcerer.DomainConcepts
{
    internal static class DomainOperationMutex
    {
        private static readonly SemaphoreSlim _globalMutex = new SemaphoreSlim(1, 1);

        public static void Wait()
        {
            _globalMutex.Wait();
        }

        public static void Release()
        {
            _globalMutex.Release();
        }
    }
}