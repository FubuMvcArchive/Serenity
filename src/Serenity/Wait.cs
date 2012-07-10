using System;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Serenity
{
    public static class Wait
    {
        public static void Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (condition()) return;

            var clock = new Stopwatch();
            clock.Start();

            while (clock.ElapsedMilliseconds < timeoutInMilliseconds)
            {
                Thread.Yield();
                Thread.Sleep(500);

                if (condition()) return;
            }
        }

        public static IWebElement UntilElement(this IApplicationUnderTest testApp, Func<IWebElement> condition, int timeoutSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(testApp.Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until<IWebElement>((d) =>
            {
                return condition();
            });
        }
    }
}