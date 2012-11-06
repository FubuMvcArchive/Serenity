using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serenity.Fixtures.Handlers;

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

        public static IWebElement InputFor<T>(this ISearchContext context, Expression<Func<T, object>> property)
        {
            return context.FindElement(By.Name(property.ToAccessor().Name));
        }

        public static IWebElement LabelFor<T>(this ISearchContext context, Expression<Func<T, object>> property)
        {
            return context.FindElement(By.CssSelector("label[for='{0}']".ToFormat(property.ToAccessor().Name)));
        }

        public static string Data(this IWebElement element, string attribute)
        {
            return element.GetAttribute("data-{0}".ToFormat(attribute));
        }

        public static IWebElement Parent(this IWebElement element)
        {
            return element.FindElement(By.XPath(".."));
        }

        public static IEnumerable<string> GetClasses(this IWebElement element)
        {
            return element
                .GetAttribute("class")
                .Split(' ');
        }

        public static bool HasClass(this IWebElement element, string className)
        {
            return element
                .GetClasses()
                .Contains(className);
        }

        public static string Value(this IWebElement element)
        {
            return element.GetAttribute("value");
        }

        public static string Id(this IWebElement element)
        {
            return element.GetAttribute("id");
        }

        public static string FindClasses(this IWebElement element, params string[] classes)
        {
            return classes.Where(c => element.HasClass(c)).Join(" ");
        }

        public static string GetData(this ISearchContext context, IWebElement element)
        {
            return ElementHandlers.FindHandler(element).GetData(context, element);
        }

        public static void SetData(this ISearchContext context, IWebElement element, string value)
        {
            ElementHandlers.FindHandler(element).EnterData(context, element, value);
        }
    }
}