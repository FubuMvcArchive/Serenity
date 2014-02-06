using OpenQA.Selenium;
using OpenQA.Selenium.IE;

namespace Serenity
{
    public class InternetExplorerBrowser : BrowserLifecycle
    {
        public const string Process = "IEXPLORE";

        protected override IWebDriver buildDriver()
        {
            return new InternetExplorerDriver();
        }

        protected override void cleanUp(IWebDriver driver)
        {
            driver.Close();
            driver.Dispose();
        }

        protected override void aggressiveCleanup()
        {
            Kill.Processes(Process);
        }
    }
}