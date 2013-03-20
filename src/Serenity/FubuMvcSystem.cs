using System;
using System.Collections.Generic;
using FubuCore.Binding;
using FubuCore.Conversion;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration.ObjectGraph;
using Serenity.Fixtures.Handlers;
using StoryTeller;
using StoryTeller.Engine;
using FubuCore;

namespace Serenity
{
    
    public interface ISubSystem
    {
        // TODO -- this should be in Storyteller itself(?)

        void Start(IServiceLocator services);
        void Stop();
    }

    public class FubuMvcSystem : ISystem
    {
        private readonly ApplicationSettings _settings;
        private readonly Func<FubuRuntime> _runtimeSource;
        private readonly Lazy<ISerenityHosting> _hosting;
	    private Func<ISerenityHosting> _createHosting;
        private Lazy<IApplicationUnderTest> _application;
        private BindingRegistry _binding;

        // TODO -- this reoccurs so often that we might as well put something in FubuCore for it
        private readonly IList<Action<BindingRegistry>> _bindingRegistrations = new List<Action<BindingRegistry>>(); 
        private readonly IList<Action<IApplicationUnderTest>> _applicationAlterations = new List<Action<IApplicationUnderTest>>();
        private readonly IList<ISubSystem> _subSystems = new List<ISubSystem>(); 

        public FubuMvcSystem(ApplicationSettings settings, Func<FubuRuntime> runtimeSource)
        {
            _settings = settings;
            _runtimeSource = runtimeSource;

			_createHosting = () =>
			{
				return settings.RootUrl.IsEmpty() ? (ISerenityHosting)new KatanaHosting() : new ExternalHosting();
			};

            _hosting = new Lazy<ISerenityHosting>(() => _createHosting());

            resetApplication();
        }


        public void AddSubSystem<T>() where T : ISubSystem, new()
        {
            AddSubSystem(new T());
        }

        public void AddSubSystem(ISubSystem subSystem)
        {
            _subSystems.Add(subSystem);
        }

        public IEnumerable<ISubSystem> SubSystems
        {
            get { return _subSystems; }
        }

        public IApplicationUnderTest Application
        {
            get { return _application.Value; }
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
        /// Register a policy about what to do after navigating the browser to handle issues
        /// like being redirected to a login screen
        /// </summary>
        public IAfterNavigation AfterNavigation
        {
            set
            {
                _applicationAlterations.Add(aut => aut.Navigation.AfterNavigation = value);
            }
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

            _subSystems.Each(x => x.Stop());
        }

        private IApplicationUnderTest buildApplication()
        {
			var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();
	        WebDriverSettings.Import(settings);

            FubuMvcPackageFacility.PhysicalRootPath = _settings.PhysicalPath;
            var runtime = _runtimeSource();
            var application = _hosting.Value.Start(_settings, runtime, WebDriverSettings.GetBrowserLifecyle());
            _applicationAlterations.Each(x => x(application));


            _binding = application.Services.GetInstance<BindingRegistry>();
            _bindingRegistrations.Each(x => x(_binding));

            configureApplication(application, _binding);

            runtime.Facility.Register(typeof(IApplicationUnderTest), ObjectDef.ForValue(application));

            // TODO -- add some registration stuff here?

            _subSystems.Each(x => x.Start(application.Services));

            return application;
        }

        // TODO -- like to make this go away
        protected virtual void configureApplication(IApplicationUnderTest application, BindingRegistry binding)
        {
            
        }

		public void UseHosting<T>()
			where T : ISerenityHosting, new()
		{
			UseHosting(new T());
		}

		public void UseHosting(ISerenityHosting hosting)
		{
			_createHosting = () => hosting;
		}

        public void Dispose()
        {
            shutdownApplication();
            _hosting.Value.Shutdown();
        }

        public IExecutionContext CreateContext()
        {
            return new FubuMvcContext(_application.Value, _binding);
        }

        public void Recycle()
        {
            _hosting.Value.Shutdown();
            resetApplication();
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