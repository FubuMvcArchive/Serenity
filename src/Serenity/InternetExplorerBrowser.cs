using OpenQA.Selenium;
using OpenQA.Selenium.IE;

namespace Serenity
{
    public class InternetExplorerBrowser : BrowserLifecycle
    {
        public const string Process = "IEXPLORE";

        protected override void preCleanUp()
        {
            Kill.Processes(Process);
        }

        protected override IWebDriver buildDriver()
        {
            return new InternetExplorerDriver();
        }
    }
}