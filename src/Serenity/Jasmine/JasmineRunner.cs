using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.SelfHost;
using OpenQA.Selenium;

namespace Serenity.Jasmine
{
    public class JasmineRunner : ISpecFileListener
    {
        private readonly JasmineInput _input;
        private SerenityJasmineApplication _application;
        private ApplicationUnderTest _applicationUnderTest;
        private JasmineConfiguration _configuration;
        private NavigationDriver _driver;
        private SelfHostHttpServer _server;
        private AssetFileWatcher _watcher;


        public JasmineRunner(JasmineInput input)
        {
            _input = input;
            _input.PortFlag = PortFinder.FindPort(input.PortFlag);
        }

        #region ISpecFileListener Members

        void ISpecFileListener.Changed()
        {
            _applicationUnderTest.Driver.Navigate().Refresh();
        }

        void ISpecFileListener.Deleted()
        {
            recycleServer();

            _applicationUnderTest.Driver.Navigate().GoToUrl(_applicationUnderTest.RootUrl);
        }

        void ISpecFileListener.Added()
        {
            Recycle();
        }

        public void Recycle()
        {
            recycleServer();
            _applicationUnderTest.Driver.Navigate().Refresh();
        }

        #endregion

        private void recycleServer()
        {
            FubuRuntime runtime = _application.BuildApplication().Bootstrap();
            _server.Recycle(runtime)
                .ContinueWith(t => { watchAssetFiles(runtime); }).Wait();
        }

        public void OpenInteractive()
        {
            buildApplication();

            run();

            _driver.NavigateToHome();
        }

        private string runningFolder()
        {
            var file = Assembly.GetExecutingAssembly().CodeBase;
            if (file != null)
            {
                file = file.Replace("file:///", "");
                return file.ParentDirectory();
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public bool RunAllSpecs()
        {
            string title = "Running Jasmine specs for project at " + _input.SerenityFile;
            Console.WriteLine(title);
            string line = "".PadRight(title.Length, '-');

            Console.WriteLine(line);

            buildApplication();
            bool returnValue = true;

            _server = new SelfHostHttpServer(_input.PortFlag);
            _server.Start(_application.BuildApplication().Bootstrap(), runningFolder());

            _driver.NavigateTo<JasminePages>(x => x.AllSpecs());

            IWebDriver browser = _applicationUnderTest.Driver;
            Wait.Until(() => browser.FindElement(By.ClassName("finished-at")).Text.IsNotEmpty(),
                       timeoutInMilliseconds: _input.TimeoutFlag*1000);
            ReadOnlyCollection<IWebElement> failures = browser.FindElements(By.CssSelector("div.suite.failed"));

            if (_input.Mode == JasmineMode.run && _input.VerboseFlag)
            {
                browser.As<IJavaScriptExecutor>().ExecuteScript("$('#jasmine-reporter').show();");
                ReadOnlyCollection<IWebElement> logs = browser.FindElements(By.ClassName("jasmine-reporter-item"));
                logs.Each(message => Console.WriteLine(message.Text));
                browser.As<IJavaScriptExecutor>().ExecuteScript("$('#jasmine-reporter').hide();");
            }

            if (failures.Any())
            {
                returnValue = false;

                Console.WriteLine(line);
                writeFailures(failures);
            }

            Console.WriteLine();
            Console.WriteLine(line);
            writeTotals(browser);

            browser.Quit();
            browser.SafeDispose();
            _server.Dispose();

            return returnValue;
        }

        private static void writeTotals(IWebDriver browser)
        {
            string totals = browser.FindElement(By.CssSelector("div.jasmine_reporter a.description")).Text;

            Console.WriteLine(totals);
        }


        private static void writeFailures(IEnumerable<IWebElement> failures)
        {
            failures.Each(suite => {
                Console.WriteLine(suite.FindElement(By.CssSelector("a.description")).Text);

                suite.FindElements(By.CssSelector("div.spec.failed a.description"))
                    .Each(spec => Console.WriteLine("    " + spec.Text));
            });
        }

        private void run()
        {
            _server = new SelfHostHttpServer(_input.PortFlag);
            FubuRuntime runtime = _application.BuildApplication().Bootstrap();
            _server.Start(runtime, runningFolder());
            watchAssetFiles(runtime);
        }

        private void watchAssetFiles(FubuRuntime runtime)
        {
            if (_watcher == null)
            {
                _watcher = runtime.Facility.Get<AssetFileWatcher>();
                _watcher.StartWatching(this, _configuration);
            }
        }


        private void buildApplication()
        {
            _application = new SerenityJasmineApplication();
            var fileSystem = new FileSystem();
            var loader = new JasmineConfigLoader(fileSystem);
            var configurator = new JasmineConfigurator(fileSystem, loader);
            _configuration = configurator.Configure(_input.SerenityFile, _application);


            var applicationSettings = new ApplicationSettings
            {
                RootUrl = "http://localhost:" + _input.PortFlag
            };

            IBrowserLifecycle browserBuilder = _input.GetBrowser();

            _applicationUnderTest = new ApplicationUnderTest(_application, applicationSettings, browserBuilder);

            _driver = new NavigationDriver(_applicationUnderTest);
        }

        public void Close()
        {
            _server.SafeDispose();

            try
            {
                _applicationUnderTest.Browser.Driver.Quit();
            }
            catch (Exception)
            {
                Console.WriteLine("Error while trying to close the browser window");
            }
            _applicationUnderTest.Driver.SafeDispose();
        }
    }
}