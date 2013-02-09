using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FubuCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using Serenity.StoryTeller;
using StoryTeller;

namespace Serenity
{
    public interface IBrowserSessionInitializer
    {
        void InitializeSession(IWebDriver driver);
    }

    public interface IBrowserLifecycle : IDisposable
    {
        void UseInitializer(IBrowserSessionInitializer initializer);

        IWebDriver Driver { get; }
        void Recycle();
        bool HasBeenStarted();
    }

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

        private IWebDriver initializeDriver()
        {
            var driver = buildDriver();
            _initializers.Each(x => x.InitializeSession(driver));

            return driver;
        }

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

	public class PhantomBrowser : BrowserLifecycle
	{
		public const string File = "phantomjs.exe";

		protected override IWebDriver buildDriver()
		{
			var fileSystem = new FileSystem();
			var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();

			if (fileSystem.FileExists(settings.WorkingDir, File))
			{
				return new PhantomJSDriver(settings.WorkingDir);
			}

			return new PhantomJSDriver(AppDomain.CurrentDomain.BaseDirectory);
		}
	}

    public class InternetExplorerBrowser : BrowserLifecycle
    {
        protected override IWebDriver buildDriver()
        {
            return new InternetExplorerDriver();
        }
    }

    public class FirefoxBrowser : BrowserLifecycle
    {
        protected override IWebDriver buildDriver()
        {
            return new FirefoxDriver();
        }
    }

    public class ChromeBrowser : BrowserLifecycle
    {
        protected override IWebDriver buildDriver()
        {
            return new ChromeDriver();
        }

        protected override void cleanUp(IWebDriver value)
        {
            Kill.Processes("chromedriver", "chromedriver.exe");
        }
    }

    public static class Kill
    {
        public static void Processes(params string[] names)
        {
            names.Each(process =>
            {
                try
                {
                    Process.GetProcessesByName(process).Each(x =>
                    {
                        Console.WriteLine("Trying to kill process " + x);
                        x.Kill();
                    });
                }
                catch (Exception e)
                {
                    // send it out to Console, but otherwise just kill it
                    Console.WriteLine(e);
                }
            });
        }
    }
}