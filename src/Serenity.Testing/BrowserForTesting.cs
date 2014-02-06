using System;
using FubuCore.Util;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Serenity.Testing
{
    [SetUpFixture]
    public class BrowserForTesting
    {
        private static readonly Cache<Type, IBrowserLifecycle> Browsers = new Cache<Type, IBrowserLifecycle>();

        public static IWebDriver Driver { get; private set; }

        public static void Use<TBrowser>() where TBrowser : IBrowserLifecycle, new()
        {
            var type = typeof (TBrowser);

            if (!Browsers.Has(type))
            {
                Browsers.Fill(type, key => new TBrowser());
            }

            Driver = Browsers[type].Driver;
        }

        [TearDown]
        public void Teardown()
        {
            Browsers.Each(x => x.Dispose());
            Browsers.ClearAll();
        }
    }
}