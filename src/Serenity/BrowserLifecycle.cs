using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Serenity
{
    public abstract class BrowserLifecycle : IBrowserLifecycle
    {
        private Lazy<IWebDriver> _driver;
        private readonly IList<IBrowserSessionInitializer> _initializers = new List<IBrowserSessionInitializer>();


        protected BrowserLifecycle()
        {
            reset();
        }

        private void reset()
        {
            _driver = new Lazy<IWebDriver>(initializeDriver);
        }

        private bool cleanUpFlag()
        {
            bool cleanUp;
            Boolean.TryParse(Environment.GetEnvironmentVariable("serenityci"), out cleanUp);
            return cleanUp;
        }

        private IWebDriver initializeDriver()
        {
            if (cleanUpFlag()) { preCleanUp(); }

            var driver = buildDriver();
            _initializers.Each(x => x.InitializeSession(driver));

            return driver;
        }

        protected abstract void preCleanUp();
        protected abstract IWebDriver buildDriver();

        public void Dispose()
        {
            if (_driver.IsValueCreated)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _driver.Value.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

                task.Wait(2000);
                
                cleanUp(_driver.Value);
                _driver.Value.Dispose();
            }
        }

        ~BrowserLifecycle()
        {
            Dispose();
        }

        protected virtual void cleanUp(IWebDriver value)
        {
            // do nothing for most browsers
        }

        public void UseInitializer(IBrowserSessionInitializer initializer)
        {
            _initializers.Add(initializer);
        }

        public IWebDriver Driver
        {
            get { return _driver.Value; }
        }

        public void Recycle()
        {
            Dispose();
            reset();
        }

        public bool HasBeenStarted()
        {
            return _driver.IsValueCreated;
        }
    }
}