using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FubuCore;
using FubuKayak;
using FubuMVC.Core;
using FubuMVC.OwinHost;
using OpenQA.Selenium;

namespace Serenity.Jasmine
{
    public class JasmineRunner : ISpecFileListener
    {
        private readonly JasmineInput _input;
        private readonly ManualResetEvent _reset = new ManualResetEvent(false);
        private SerenityJasmineApplication _application;
        private ApplicationUnderTest _applicationUnderTest;
        private NavigationDriver _driver;
        private FubuKayakApplication _kayak;
        private Thread _kayakLoop;
        private AssetFileWatcher _watcher;
    	private JasmineConfiguration _configuration;


        public JasmineRunner(JasmineInput input)
        {
            _input = input;
            _input.PortFlag = PortFinder.FindPort(input.PortFlag);
        }

        void ISpecFileListener.Changed()
        {
            _applicationUnderTest.Driver.Navigate().Refresh();
        }

        void ISpecFileListener.Deleted()
        {
            _kayak.Recycle(watchAssetFiles);
            // TODO -- make a helper method for this
            _applicationUnderTest.Driver.Navigate().GoToUrl(_applicationUnderTest.RootUrl);
        }

        void ISpecFileListener.Added()
        {
            Recycle();
        }

        public void Recycle()
        {
            _kayak.Recycle(watchAssetFiles);
            _applicationUnderTest.Driver.Navigate().Refresh();
        }

        public void OpenInteractive()
        {
            buildApplication();

            var threadStart = new ThreadStart(run);
            _kayakLoop = new Thread(threadStart);
            _kayakLoop.Start();


            _driver.NavigateToHome();

            _reset.WaitOne();
        }

        public bool RunAllSpecs()
        {
            var title = "Running Jasmine specs for project at " + _input.SerenityFile;
            Console.WriteLine(title);
            var line = "".PadRight(title.Length, '-');

            Console.WriteLine(line);

            buildApplication();
            var returnValue = true;

            _kayak = new FubuKayakApplication(_application);
            _kayak.RunApplication(_input.PortFlag, runtime =>
            {
                _driver.NavigateTo<JasminePages>(x => x.AllSpecs());

                var browser = _applicationUnderTest.Driver;
                Wait.Until(() => browser.FindElement(By.ClassName("finished-at")).Text.IsNotEmpty(), timeoutInMilliseconds: _input.TimeoutFlag * 1000);
                var failures = browser.FindElements(By.CssSelector("div.suite.failed"));

                if (_input.Mode == JasmineMode.run && _input.VerboseFlag)
                {
                    browser.As<IJavaScriptExecutor>().ExecuteScript("$('#jasmine-reporter').show();");
                    var logs = browser.FindElements(By.ClassName("jasmine-reporter-item"));
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
                _kayak.Stop();
            });


            return returnValue;
        }

        private static void writeTotals(IWebDriver browser)
        {
            var totals = browser.FindElement(By.CssSelector("div.jasmine_reporter a.description")).Text;

            Console.WriteLine(totals);
        }


        private static void writeFailures(IEnumerable<IWebElement> failures)
        {
            failures.Each(suite =>
            {
                Console.WriteLine(suite.FindElement(By.CssSelector("a.description")).Text);

                suite.FindElements(By.CssSelector("div.spec.failed a.description"))
                    .Each(spec => Console.WriteLine("    " + spec.Text));
            });
        }

        private void run()
        {
            _kayak = new FubuKayakApplication(_application);
            _kayak.RunApplication(_input.PortFlag, watchAssetFiles);

            _reset.Set();
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


            var applicationSettings = new ApplicationSettings{
                RootUrl = "http://localhost:" + _input.PortFlag
            };

            var browserBuilder = _input.GetBrowser();

            _applicationUnderTest = new ApplicationUnderTest(_application, applicationSettings, browserBuilder);

            _driver = new NavigationDriver(_applicationUnderTest);
        }
    }
}