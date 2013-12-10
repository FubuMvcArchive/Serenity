using NUnit.Framework;
using OpenQA.Selenium;

namespace Serenity.Testing
{
    [SetUpFixture]
    public class BrowserForTesting
    {
        private static readonly FirefoxBrowser _browser = new FirefoxBrowser();

        public static IWebDriver Driver
        {
            get { return _browser.Driver; }
        }

        [TearDown]
        public void Teardown()
        {
            _browser.Dispose();
        }
    }
}