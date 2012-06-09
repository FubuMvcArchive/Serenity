using System;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.OwinHost;
using StoryTeller.Engine;

namespace Serenity
{
    public class InProcessSerenitySystem<TSystem> : BasicSystem where TSystem : IApplicationSource, new()
    {
        private readonly Lazy<IApplicationUnderTest> _application;
        private ApplicationSettings _settings;

        public InProcessSerenitySystem()
        {
            _application = new Lazy<IApplicationUnderTest>(() =>
            {
                _settings = findApplicationSettings();
                _settings.Port = PortFinder.FindPort(_settings.Port);
                _settings.RootUrl = "http://localhost:" + _settings.Port;

                return new InProcessApplicationUnderTest<TSystem>(_settings);
            });
        }

        public ApplicationSettings Settings
        {
            get { return _settings; }
        }

        protected virtual ApplicationSettings findApplicationSettings()
        {
            return ApplicationSettings.ReadFor<TSystem>();
        }

        public override void RegisterServices(ITestContext context)
        {
            if (_application.IsValueCreated)
            {
                context.Store(_application.Value);
                context.Store(new NavigationDriver(_application.Value));
            }
        }

        public override sealed void Setup()
        {

            beforeExecutingTest(_application.Value);
        }


        protected virtual void beforeExecutingTest(IApplicationUnderTest application)
        {
        }

        public T Get<T>()
        {
            return _application.Value.GetInstance<T>();
        }

        public override object Get(Type type)
        {
            if (type == typeof (IApplicationUnderTest))
            {
                return _application;
            }

            return _application.Value.GetInstance(type);
        }

        public override sealed void TeardownEnvironment()
        {
            if (_application.IsValueCreated) _application.Value.Teardown();

            shutDownSystem();
        }

        protected virtual void shutDownSystem()
        {
        }


    }
}