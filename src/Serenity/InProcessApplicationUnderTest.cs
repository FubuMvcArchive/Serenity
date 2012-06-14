using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FubuKayak;
using FubuMVC.Core;
using FubuMVC.Core.Endpoints;
using FubuMVC.Core.Http;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Urls;

using OpenQA.Selenium;
using FubuCore;

namespace Serenity
{
    public class InProcessApplicationUnderTest<TSystem> : IApplicationUnderTest where TSystem : IApplicationSource, new()
    {
        private readonly ApplicationSettings _settings;
        private readonly Lazy<FubuRuntime> _runtime;
        private readonly Lazy<IUrlRegistry> _urls;
        private readonly IList<Action> _disposals = new List<Action>();
        private Listener _listener;
        private Thread _listeningThread;
        private IBrowserLifecycle _browser;

        public InProcessApplicationUnderTest(ApplicationSettings settings)
        {
            _settings = settings;

            _runtime = new Lazy<FubuRuntime>(() =>
            {
                FubuMvcPackageFacility.PhysicalRootPath = settings.GetApplicationFolder();

                // TODO -- add some diagnostics here
                var runtime = new TSystem().BuildApplication().Bootstrap();
                runtime.Facility.Register(typeof(ICurrentHttpRequest), ObjectDef.ForValue(new StubCurrentHttpRequest()
                {
                    ApplicationRoot = "http://localhost:" + settings.Port
                }));

                runtime.Facility.Register(typeof(IApplicationUnderTest), ObjectDef.ForValue(this));

                return runtime;
            });

            _urls = new Lazy<IUrlRegistry>(() => _runtime.Value.Facility.Get<IUrlRegistry>());

            _browser = WebDriverSettings.GetBrowserLifecyle();
        }

        private ManualResetEvent startListener(ApplicationSettings settings, FubuRuntime runtime)
        {
            var reset = new ManualResetEvent(false);

            _listeningThread = new Thread(o =>
            {
                _listener = new Listener(settings.Port);
                _listener.Start(runtime, () => reset.Set());
            });

            _listeningThread.Name = "Serenity:Kayak:Thread";
            _listeningThread.Start();

            return reset;
        }




        public string Name
        {
            get { return typeof(TSystem).Name; }
        }

        public IUrlRegistry Urls
        {
            get { return _urls.Value; }
        }

        public IWebDriver Driver
        {
            get
            {
                start();

                return _browser.Driver;
            }
        }


        public string RootUrl
        {
            get { return _settings.RootUrl; }
        }

        public T GetInstance<T>()
        {
            return _runtime.Value.Facility.Get<T>();
        }

        public object GetInstance(Type type)
        {
            return _runtime.Value.Facility.Get<IServiceLocator>().GetInstance(type);
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _runtime.Value.Facility.GetAll<T>();
        }

        public IBrowserLifecycle Browser
        {
            get { return _browser; }
        }

        private void start()
        {
            if (_listener != null) return;

            var reset = startListener(_settings, _runtime.Value);

            _disposals.Add(() =>
            {
                _listener.Stop();
                _listener.SafeDispose();

                _listeningThread.Join(3000);
            });

            reset.WaitOne();
        }

        public void Ping()
        {
            // NO-OP
        }

        public void Teardown()
        {
            _browser.Dispose();
        }

        public NavigationDriver Navigation
        {
            get
            {
                start();
                return new NavigationDriver(this);
            }
        }

        public EndpointDriver Endpoints()
        {
            start();
            return new EndpointDriver(Urls);
        }
    }
}