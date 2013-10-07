using System;
using Bottles.Services.Messaging;
using Bottles.Services.Remote;
using FubuCore;

namespace Serenity
{
    public interface ISubSystem
    {
        // TODO -- this should be in Storyteller itself(?)

        void Start(IServiceLocator services);
        void Stop();
    }

    public class RemoteSubSystem : ISubSystem
    {
        private readonly Func<RemoteServiceRunner> _source;
        private RemoteServiceRunner _runner;

        public RemoteSubSystem(Func<RemoteServiceRunner> source)
        {
            _source = source;
        }

        public void Start(IServiceLocator services)
        {
            _runner = _source();
            _runner.WaitForMessage<LoaderStarted>();
        }

        public void Stop()
        {
            _runner.Dispose();
        }

        public RemoteServiceRunner Runner
        {
            get { return _runner; }
        }
    }
}