using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Serenity
{
    public class FirefoxBrowser : BrowserLifecycle
    {
        public const string Process = "firefox";

        protected override void preCleanUp()
        {
            Kill.Processes(Process);
        }

        protected override IWebDriver buildDriver()
        {
            return new FirefoxDriver();
        }
    }
}