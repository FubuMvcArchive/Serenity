using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Serenity
{
    public class FirefoxBrowser : BrowserLifecycle
    {
        protected override IWebDriver buildDriver()
        {
            return new FirefoxDriver();
        }
    }
}