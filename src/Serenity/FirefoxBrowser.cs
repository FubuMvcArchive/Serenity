using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Serenity
{
    public class FirefoxBrowser : BrowserLifecycle
    {
        public const string Process = "firefox";

        protected override IWebDriver buildDriver()
        {
            return new FirefoxDriver();
        }

        protected override void cleanUp(IWebDriver value)
        {
            value.Dispose();
        }

        protected override void aggressiveCleanup()
        {
            Kill.Processes(Process);
        }
    }
}