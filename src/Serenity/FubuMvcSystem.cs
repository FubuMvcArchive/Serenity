using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bottles.Services.Messaging.Tracking;
using Bottles.Services.Remote;
using FubuCore.Binding;
using FubuCore.CommandLine;
using FubuCore.Conversion;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.OwinHost;
using HtmlTags;
using Serenity.Fixtures.Handlers;
using StoryTeller;
using StoryTeller.Engine;
using FubuCore;
using StoryTeller.Workspace;
using StructureMap;

namespace Serenity
{
    public interface IContextualInfoProvider
    {
        void Reset();
        IEnumerable<HtmlTag> GenerateReports();
    }

    public interface IRemoteSubsystems
    {
        RemoteSubSystem RemoteSubSystemFor(string name);
        IEnumerable<RemoteSubSystem> RemoteSubSystems { get; }
    }

    public interface IFubuMvcSystem
    {
        BindingRegistry Binding { get; }
        IApplicationUnderTest Application { get; }
        IEnumerable<IContextualInfoProvider> ContextualProviders { get; }
    }

    public class FubuMvcSystem : ISystem, ISubSystem, IRemoteSubsystems, IFubuMvcSystem
    {
        private readonly ApplicationSettings _settings;
        private readonly Func<FubuRuntime> _runtimeSource;
        private BindingRegistry _binding;
        private bool? _externalHosting;

        // TODO -- this reoccurs so often that we might as well put something in FubuCore for it
        private readonly IList<Action<BindingRegistry>> _bindingRegistrations = new List<Action<BindingRegistry>>();

        private readonly IList<Action<IApplicationUnderTest>> _applicationAlterations =
            new List<Action<IApplicationUnderTest>>();

        private readonly IList<ISubSystem> _subSystems = new List<ISubSystem>();

        public FubuMvcSystem(ApplicationSettings settings, Func<FubuRuntime> runtimeSource)
        {
            _settings = settings;
            _runtimeSource = runtimeSource;

            _subSystems.Add(this);

            OnContextCreation(StartListeningForMessages);
        }

        public BrowserType? DefaultBrowser { get; set; }

        private readonly Cache<string, RemoteSubSystem> _remoteSubSystems = new Cache<string, RemoteSubSystem>();
        private ISerenityHosting _hosting;
        protected IApplicationUnderTest _application;
        private IEnumerable<IContextualInfoProvider> _contextualProviders;

        /// <summary>
        /// *IF* your underlying container is StructureMap, this is a convenience method
        /// to modify the underlying container on Serenity system start up time
        /// </summary>
        /// <param name="configuration"></param>
        public void ModifyContainer(Action<ConfigurationExpression> configuration)
        {
            _applicationAlterations.Add(app =>
            {
                var container = app.Services.GetInstance<IContainer>();
                container.Configure(configuration);
            });
        }

        public RemoteSubSystem RemoteSubSystemFor(string name)
        {
            return _remoteSubSystems[name];
        }

        public void AddRemoteSubSystem(string name, Action<RemoteDomainExpression> configuration)
        {
            var system = new RemoteSubSystem(() => new RemoteServiceRunner(x =>
            {
                x.Properties[FubuMode.Testing] = true.ToString();
                configuration(x);
            }));

            _remoteSubSystems[name] = system;

            _subSystems.Add(system);
        }

        public void AddSubSystem<T>() where T : ISubSystem, new()
        {
            AddSubSystem(new T());
        }

        public void AddSubSystem(ISubSystem subSystem)
        {
            _subSystems.Add(subSystem);
        }

        public void StartListeningForMessages()
        {
            MessageHistory.StartListening(_remoteSubSystems.Select(x => x.Runner).ToArray());
        }

        public IEnumerable<ISubSystem> SubSystems
        {
            get { return _subSystems; }
        }

        public IApplicationUnderTest Application
        {
            get { return _application; }
        }

        /// <summary>
        /// Catch all method to call any service from the running application when
        /// the application restarts
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="startup"></param>
        public void OnStartup<TService>(Action<TService> startup)
        {
            _applicationAlterations.Add(app => startup(app.Services.GetInstance<TService>()));
        }

        /// <summary>
        /// Catch all method to perform any action from the running application when 
        /// the application restarts
        /// </summary>
        /// <param name="action"></param>
        public void OnStartup(Action action)
        {
            _applicationAlterations.Add(app => action());
        }

        public IEnumerable<RemoteSubSystem> RemoteSubSystems
        {
            get { return _subSystems.OfType<RemoteSubSystem>(); }
        }

        /// <summary>
        /// Register a policy about what to do after navigating the browser to handle issues
        /// like being redirected to a login screen
        /// </summary>
        public IAfterNavigation AfterNavigation
        {
            set { _applicationAlterations.Add(aut => aut.Navigation.AfterNavigation = value); }
        }

        /// <summary>
        /// Be aware that this will ONLY work with StructureMap as the underlying container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddContextualProvider<T>() where T : IContextualInfoProvider
        {
            ModifyContainer(x => x.For<IContextualInfoProvider>().Add<T>());
        }

        /// <summary>
        /// Be aware that this will ONLY work with StructureMap as the underlying container
        /// </summary>
        public void AddContextualProvider(IContextualInfoProvider provider)
        {
            ModifyContainer(x => x.For<IContextualInfoProvider>().Add(provider));
        }

        /// <summary>
        /// Add a new converter strategy to customize how Storyteller will convert a string
        /// into a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddConverter<T>() where T : IObjectConverterFamily, new()
        {
            AddConverter(new T());
        }

        /// <summary>
        /// Add a new converter strategy to customize how Storyteller will convert a string
        /// into a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddConverter(IObjectConverterFamily family)
        {
            _bindingRegistrations.Add(registry => registry.Converters.RegisterConverterFamily(family));
        }

        /// <summary>
        /// Add an element handler to the ElementHandlers collection for driving
        /// IWebElement's with WebDriver
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddElementHandler<T>() where T : IElementHandler, new()
        {
            AddElementHandler(new T());
        }

        /// <summary>
        /// Add an element handler to the ElementHandlers collection for driving
        /// IWebElement's with WebDriver
        /// </summary>
        public void AddElementHandler(IElementHandler handler)
        {
            ElementHandlers.Handlers.Add(handler);
        }

        public BrowserType ChooseBrowserType()
        {
            if (Project.Current != null && Project.Current.Profile.IsNotEmpty())
            {
                BrowserType profileType = BrowserType.IE;
                if (BrowserType.TryParse(Project.Current.Profile, true, out profileType))
                {
                    return profileType;
                }
            }

            if (DefaultBrowser != null) return DefaultBrowser.Value;

            return WebDriverSettings.Current.Browser;
        }

        // TODO -- like to make this go away
        [Obsolete("This method should not NEED to be used.  Please favor the OnStartup registrations instead")]
        protected virtual void configureApplication(IApplicationUnderTest application, BindingRegistry binding)
        {

        }

        public void Dispose()
        {
            stopAll();
            _isDisposed = true;
        }

        protected virtual void stopAll()
        {
            var succeeded = Task.WaitAll(_subSystems.Select(x => x.Stop()).ToArray(), TimeSpan.FromMinutes(1));

            if (!succeeded)
            {
                // TODO: Replace with a logger
                ConsoleWriter.Write(ConsoleColor.Yellow, "WARNING: Failed to stop FubuMvcSystem and/or registered subsystems");
            }
        }

        public virtual IExecutionContext CreateContext()
        {
            if (_application == null)
            {
                startAll();
            }

            var context = new FubuMvcContext(this);

            _contextCreationActions.Each(x => x());

            return context;
        }

        protected virtual void startAll()
        {
            Task.WaitAll(_subSystems.Select(x => x.Start()).ToArray());
        }

        ~FubuMvcSystem()
        {
            if (_isDisposed) return;

            this.SafeDispose();
        }

        public void Recycle()
        {
            stopAll();
            startAll();
        }

        Task ISubSystem.Start()
        {
            return Task.Factory.StartNew(() =>
            {
                var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();
                WebDriverSettings.Import(settings);

                FubuMvcPackageFacility.PhysicalRootPath = _settings.PhysicalPath;
                _runtime = _runtimeSource();

                var browserLifecycle = WebDriverSettings.GetBrowserLifecyle(ChooseBrowserType());
                SetupApplicationHost();

                _application = _hosting.Start(_settings, _runtime, browserLifecycle);
                _applicationAlterations.Each(x => x(_application));

                _binding = _application.Services.GetInstance<BindingRegistry>();
                _bindingRegistrations.Each(x => x(_binding));

                configureApplication(_application, _binding);

                _contextualProviders = _runtime.Factory.GetAll<IContextualInfoProvider>();

                _runtime.Facility.Register(typeof (IApplicationUnderTest), ObjectDef.ForValue(_application));
                _runtime.Facility.Register(typeof (IRemoteSubsystems), ObjectDef.ForValue(this));
            });
        }

        private void SetupApplicationHost()
        {
            if (_externalHosting == null)
            {
                _externalHosting = !_settings.RootUrl.IsEmpty();
            }

            _hosting = _externalHosting.Value ? (ISerenityHosting) new ExternalHosting() : new KatanaHosting();
        }

        Task ISubSystem.Stop()
        {
            return Task.Factory.StartNew(() =>
            {
                if (_runtime != null)
                {
                    _runtime.SafeDispose();
                    _runtime = null;
                }
                if (_application != null)
                {
                    _application.Teardown();
                    _application = null;
                }
                if (_hosting != null)
                {
                    _hosting.Shutdown();
                    _hosting = null;
                }
            });
        }


        private readonly IList<Action> _contextCreationActions = new List<Action>();
        private FubuRuntime _runtime;
        private bool _isDisposed;

        /// <summary>
        /// Perform an action immediately after a new execution context
        /// is created immediately before the test itself is executed
        /// </summary>
        /// <param name="action"></param>
        public void OnContextCreation(Action action)
        {
            _contextCreationActions.Add(action);
        }

        /// <summary>
        /// Perform an action using a service resolved from the application 
        /// immediately after a new execution context
        /// is created immediately before the test itself is executed
        /// </summary>
        /// <param name="action"></param>
        public void OnContextCreation<T>(Action<T> action)
        {
            _contextCreationActions.Add(() => action(Application.Services.GetInstance<T>()));
        }

        public ApplicationSettings Settings
        {
            get { return _settings; }
        }

        BindingRegistry IFubuMvcSystem.Binding
        {
            get { return _binding; }
        }

        public virtual IEnumerable<IContextualInfoProvider> ContextualProviders
        {
            get { return _contextualProviders; }
        }
    }

    public class FubuMvcSystem<T> : FubuMvcSystem where T : IApplicationSource, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parallelDirectory">Use to override the physical root path of the web application to a directory parallel to the testing project if it cannot be derived from the assembly name of the application source type.</param>
        /// <param name="physicalPath">Use to override the physical root path of the web application if it cannot be derived from the assembly name of the application source type.</param>
        public FubuMvcSystem(string parallelDirectory = null, string physicalPath = null)
            : base(DetermineSettings(parallelDirectory, physicalPath), () => new T().BuildApplication().Bootstrap())
        {
        }

        public FubuMvcSystem(ApplicationSettings settings)
            : base(settings, () => new T().BuildApplication().Bootstrap())
        {
        }

        public static ApplicationSettings DetermineSettings(string parallelDirectory = null, string physicalPath = null)
        {
            try
            {
                return ApplicationSettings.ReadFor<T>() ?? DefaultSettings(parallelDirectory, physicalPath);
            }
                // So wrong...
            catch (ArgumentOutOfRangeException)
            {
                return DefaultSettings();
            }
        }

        private static ApplicationSettings DefaultSettings(string parallelDirectory = null, string physicalPath = null)
        {
            if (physicalPath.IsEmpty())
            {
                var sourceFolder = AppDomain.CurrentDomain.BaseDirectory.ParentDirectory().ParentDirectory().ParentDirectory();
                physicalPath = sourceFolder.AppendPath(parallelDirectory ?? typeof(T).Assembly.GetName().Name);
            }



            return new ApplicationSettings
            {
                ApplicationSourceName = typeof (T).AssemblyQualifiedName,
                PhysicalPath = physicalPath,
                Port = PortFinder.FindPort(5500) // just a starting point
            };
        }
    }
}