using OpenQA.Selenium;
using OpenQA.Selenium.IE;

namespace Serenity
{
    public class InternetExplorerBrowser : BrowserLifecycle
    {
        protected override IWebDriver buildDriver()
        {
            return new InternetExplorerDriver();
        }
    }
}