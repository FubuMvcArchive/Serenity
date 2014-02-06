using System;
using FubuCore;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using StoryTeller;

namespace Serenity
{
    public class PhantomBrowser : BrowserLifecycle
    {
        public const string Process = "phantomjs";
        public const string File = "phantomjs.exe";

        protected override IWebDriver buildDriver()
        {
            var fileSystem = new FileSystem();
            var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();

            if (fileSystem.FileExists(settings.WorkingDir, File))
            {
                return new PhantomJSDriver(settings.WorkingDir);
            }

            return new PhantomJSDriver(AppDomain.CurrentDomain.BaseDirectory);
        }

        protected override void cleanUp(IWebDriver value)
        {
            value.Close();
            value.Dispose();
        }

        protected override void aggressiveCleanup()
        {
            Kill.Processes(Process, File);
        }
    }
}