using System;
using System.Diagnostics;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.SelfHost;
using Serenity.StoryTeller;
using StoryTeller;
using StoryTeller.Engine;
using FubuCore;

namespace Serenity
{

    public interface ISerenityHosting
    {
        IApplicationUnderTest Start(ApplicationSettings settings, FubuRuntime runtime, IBrowserLifecycle lifecycle);
        void Shutdown();
    }

    public class ExternalHosting : ISerenityHosting
    {
        public IApplicationUnderTest Start(ApplicationSettings settings, FubuRuntime runtime, IBrowserLifecycle lifecycle)
        {
            var application = new ApplicationUnderTest(runtime, settings, lifecycle);
            application.Ping();

            return application;
        }

        public void Shutdown()
        {
            // Nothing
        }
    }

    public class SelfHosting : ISerenityHosting
    {
        private SelfHostHttpServer _server;

        public IApplicationUnderTest Start(ApplicationSettings settings, FubuRuntime runtime, IBrowserLifecycle lifecycle)
        {
            _server = new SelfHostHttpServer(settings.Port, settings.PhysicalPath);
            _server.Start(runtime);

            settings.RootUrl = _server.BaseAddress;

            return new ApplicationUnderTest(runtime, settings, lifecycle);
        }

        public void Shutdown()
        {
            if (_server != null) _server.SafeDispose();

            _server = null;
        }
    }

    public class FubuMvcSystem : ISystem
    {
        private readonly ApplicationSettings _settings;
        private readonly Func<FubuRuntime> _runtimeSource;
        private readonly ISerenityHosting _hosting;
        private Lazy<IApplicationUnderTest> _application;
        private BindingRegistry _binding;

        public FubuMvcSystem(ApplicationSettings settings, Func<FubuRuntime> runtimeSource)
        {
            _settings = settings;
            _runtimeSource = runtimeSource;
            _hosting = settings.RootUrl.IsEmpty() ? (ISerenityHosting) new SelfHosting() : new ExternalHosting();

            resetApplication();
        }

        private void resetApplication()
        {
            shutdownApplication();

            _application = new Lazy<IApplicationUnderTest>(buildApplication);
        }

        private void shutdownApplication()
        {
            if (_application != null && _application.IsValueCreated)
            {
                _application.Value.Teardown();
            }
        }

        private IApplicationUnderTest buildApplication()
        {
			var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();
	        WebDriverSettings.Current.Browser = settings.Browser;

            FubuMvcPackageFacility.PhysicalRootPath = _settings.PhysicalPath;
            var runtime = _runtimeSource();
            var application = _hosting.Start(_settings, runtime, WebDriverSettings.GetBrowserLifecyle());

            _binding = application.Services.GetInstance<BindingRegistry>();
            configureApplication(application, _binding);

            runtime.Facility.Register(typeof(IApplicationUnderTest), ObjectDef.ForValue(application));

            return application;
        }

        protected virtual void configureApplication(IApplicationUnderTest application, BindingRegistry binding)
        {
            
        }

        public void Dispose()
        {
            shutdownApplication();
            _hosting.Shutdown();
        }

        public IExecutionContext CreateContext()
        {
            return new FubuMvcContext(_application.Value, _binding);
        }

        public void Recycle()
        {
            _hosting.Shutdown();
            resetApplication();
        }
    }

    public class FubuMvcContext : IExecutionContext
    {
        private readonly IApplicationUnderTest _application;
        private readonly BindingRegistry _binding;

        public FubuMvcContext(IApplicationUnderTest application, BindingRegistry binding)
        {
            _application = application;
            _binding = binding;
        }

        public void Dispose()
        {

        }

        public IServiceLocator Services
        {
            get { return _application.Services; }
        }

        public BindingRegistry BindingRegistry
        {
            get { return _binding; }
        }
    }


    public class FubuMvcSystem<T> : FubuMvcSystem where T : IApplicationSource, new()
    {
        public FubuMvcSystem() : base(DetermineSettings(), () => new T().BuildApplication().Bootstrap())
        {
        }

        public static ApplicationSettings DetermineSettings()
        {
            try
            {
                return ApplicationSettings.ReadFor<T>() ?? DefaultSettings();
            }
            // So wrong...
            catch(ArgumentOutOfRangeException)
            {
                return DefaultSettings();
            }
        }

        private static ApplicationSettings DefaultSettings()
        {
            var sourceFolder = AppDomain.CurrentDomain.BaseDirectory.ParentDirectory().ParentDirectory().ParentDirectory();
            var applicationFolder = sourceFolder.AppendPath(typeof (T).Assembly.GetName().Name);

            return new ApplicationSettings
            {
                ApplicationSourceName = typeof (T).AssemblyQualifiedName,
                PhysicalPath = applicationFolder,
                Port = 5500 // just a starting point
            };
        }
    }







}