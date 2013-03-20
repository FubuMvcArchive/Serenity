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

        private readonly IList<Action<BindingRegistry>> _bindingRegistrations = new List<Action<BindingRegistry>>(); 

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
        }

        private IApplicationUnderTest buildApplication()
        {
			var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();
	        WebDriverSettings.Import(settings);

            FubuMvcPackageFacility.PhysicalRootPath = _settings.PhysicalPath;
            var runtime = _runtimeSource();
            var application = _hosting.Value.Start(_settings, runtime, WebDriverSettings.GetBrowserLifecyle());

            _binding = application.Services.GetInstance<BindingRegistry>();
            _bindingRegistrations.Each(x => x(_binding));

            configureApplication(application, _binding);

            runtime.Facility.Register(typeof(IApplicationUnderTest), ObjectDef.ForValue(application));

            return application;
        }

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