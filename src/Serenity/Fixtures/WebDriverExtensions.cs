using System;
using FubuCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Serenity.Fixtures
{
    public static class WebDriverExtensions
    {
        public static By ByCss(this string css)
        {
            return css.IsEmpty() ? null : By.CssSelector(css);
        }

        public static By ByName(this string name)
        {
            return name.IsEmpty() ? null : By.Name(name);
        }

        public static By ById(this string id)
        {
            return id.IsEmpty() ? null : By.Id(id);
        }

        // the interface just isn't very discoverable
        public static object InjectJavascript(this IWebDriver driver, string script)
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        public static T InjectJavascript<T>(this IWebDriver driver, string script)
        {
            return (T)driver.InjectJavascript(script);
        }

        public static IWebElement FindElementByData(this IWebDriver driver, string attribute, string value)
        {
            return driver.FindElement(By.CssSelector("*[data-{0}={1}]".ToFormat(attribute, value)));
        }

        public static IWebElement WaitUntil(this IWebDriver driver, Func<IWebElement> condition, int timeoutSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until<IWebElement>((d) =>
            {
                return condition();
            });
        }
    }
}