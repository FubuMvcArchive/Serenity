using System;
using System.Threading.Tasks;
using Bottles.Services.Remote;

namespace Serenity
{
    public class RemoteSubSystem : ISubSystem
    {
        private readonly Func<RemoteServiceRunner> _source;
        private RemoteServiceRunner _runner;

        public RemoteSubSystem(Func<RemoteServiceRunner> source)
        {
            _source = source;
        }

        public Task Start()
        {
            return Task.Factory.StartNew(() => {
                _runner = _source();
                _runner.WaitForMessage<LoaderStarted>();
            });
        }

        public Task Stop()
        {
            return Task.Factory.StartNew(() => {
                if (_runner != null) _runner.Dispose();
            });
        }

        public RemoteServiceRunner Runner
        {
            get { return _runner; }
        }
    }
}